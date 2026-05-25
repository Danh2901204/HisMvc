using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Cashier.Controllers;

[Area("Cashier")]
[Authorize(Roles = AppRoles.CASHIER + "," + AppRoles.ADMIN)]
public class HomeController : Controller
{
    private readonly AppDbContext _db;
    private readonly InsuranceService _insuranceService;
    
    public HomeController(AppDbContext db, InsuranceService insuranceService)
    {
        _db = db;
        _insuranceService = insuranceService;
    }

    public async Task<IActionResult> Index(string status = "", DateOnly? date = null)
    {
        var query = _db.Invoices
            .Include(x => x.Encounter)!.ThenInclude(e => e.Patient)
            .Include(x => x.Encounter)!.ThenInclude(e => e.Doctor)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            if (status == "Unpaid")
                query = query.Where(x => x.Status == InvoiceStatus.Unpaid);
            else if (status == "Paid")
                query = query.Where(x => x.Status == InvoiceStatus.Paid);
        }

        if (date.HasValue)
        {
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) == date.Value);
        }
        else
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) == today);
        }

        var invoices = await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        ViewBag.CurrentStatus = status;
        ViewBag.Date = date ?? DateOnly.FromDateTime(DateTime.Today);
        return View(invoices);
    }

    public async Task<IActionResult> Pending(DateOnly? date = null)
    {
        var d = date ?? DateOnly.FromDateTime(DateTime.Today);

        var completedEncounters = await _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .Include(x => x.Appointment)
            .Where(x => x.Status == EncounterStatus.Completed)
            .Where(x => DateOnly.FromDateTime(x.CheckInAt) == d)
            .ToListAsync();

        var invoicedEncounterIds = await _db.Invoices
            .Select(x => x.EncounterId)
            .ToListAsync();

        var pending = completedEncounters
            .Where(x => !invoicedEncounterIds.Contains(x.EncounterId))
            .OrderByDescending(x => x.EndAt)
            .ToList();

        ViewBag.Date = d;
        return View(pending);
    }

    public async Task<IActionResult> Create(int encounterId)
    {
        var encounter = await _db.Encounters
            .Include(x => x.Patient)
            .Include(x => x.Doctor)
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId);

        if (encounter == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Pending));
        }

        if (encounter.Status != EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham chua hoan thanh!";
            return RedirectToAction(nameof(Pending));
        }

        var existingInvoice = await _db.Invoices
            .FirstOrDefaultAsync(x => x.EncounterId == encounterId);

        if (existingInvoice != null)
        {
            TempData["Error"] = "Luot kham nay da co hoa don!";
            return RedirectToAction(nameof(Detail), new { id = existingInvoice.InvoiceId });
        }

        var orders = await _db.Orders
            .Include(x => x.Service)
            .Where(x => x.EncounterId == encounterId)
            .ToListAsync();

        decimal examFee = HisConstants.EXAM_FEE;
        decimal totalOrderPrice = orders.Sum(x => x.Service?.Price ?? 0);
        decimal totalAmount = examFee + totalOrderPrice;

        // Tính toán BHYT
        var insuranceCalc = await _insuranceService.CalculateInsuranceForEncounter(encounterId);

        ViewBag.Encounter = encounter;
        ViewBag.Orders = orders;
        ViewBag.ExamFee = examFee;
        ViewBag.TotalOrderPrice = totalOrderPrice;
        ViewBag.TotalAmount = totalAmount;
        ViewBag.InsuranceCalculation = insuranceCalc;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvoice(int encounterId, string? note, bool useInsurance = false)
    {
        var encounter = await _db.Encounters.FindAsync(encounterId);
        if (encounter == null)
        {
            TempData["Error"] = "Khong tim thay luot kham!";
            return RedirectToAction(nameof(Pending));
        }

        if (encounter.Status != EncounterStatus.Completed)
        {
            TempData["Error"] = "Luot kham chua hoan thanh!";
            return RedirectToAction(nameof(Pending));
        }

        if (await _db.Invoices.AnyAsync(x => x.EncounterId == encounterId))
        {
            TempData["Error"] = "Luot kham nay da co hoa don!";
            return RedirectToAction(nameof(Index));
        }

        var orders = await _db.Orders
            .Include(x => x.Service)
            .Where(x => x.EncounterId == encounterId)
            .ToListAsync();

        decimal examFee = HisConstants.EXAM_FEE;
        decimal totalOrderPrice = orders.Sum(x => x.Service?.Price ?? 0);
        decimal totalAmount = examFee + totalOrderPrice;

        var invoiceCode = $"INV{DateTime.Now:yyyyMMddHHmmss}";

        // Tính toán BHYT n?u có
        decimal insuranceAmount = 0;
        decimal patientAmount = totalAmount;
        int? claimId = null;

        if (useInsurance)
        {
            var insuranceCalc = await _insuranceService.CalculateInsuranceForEncounter(encounterId);
            
            if (insuranceCalc.IsValid)
            {
                // T?o giám ??nh BHYT
                var claim = await _insuranceService.CreateInsuranceClaim(encounterId, insuranceCalc);
                claimId = claim.InsuranceClaimId;
                insuranceAmount = insuranceCalc.InsurancePays;
                patientAmount = insuranceCalc.PatientPays;
            }
        }

        var invoice = new Invoice
        {
            EncounterId = encounterId,
            InvoiceCode = invoiceCode,
            TotalAmount = totalAmount,
            InsuranceAmount = insuranceAmount,
            PatientAmount = patientAmount,
            HasInsurance = useInsurance,
            InsuranceClaimId = claimId,
            Status = InvoiceStatus.Unpaid,
            Note = note
        };

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Da tao hoa don {invoiceCode} thanh cong!";
        return RedirectToAction(nameof(Detail), new { id = invoice.InvoiceId });
    }

    public async Task<IActionResult> Detail(int id)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Encounter)!.ThenInclude(e => e.Patient)
            .Include(x => x.Encounter)!.ThenInclude(e => e.Doctor)
            .FirstOrDefaultAsync(x => x.InvoiceId == id);

        if (invoice == null)
        {
            TempData["Error"] = "Khong tim thay hoa don!";
            return RedirectToAction(nameof(Index));
        }

        var orders = await _db.Orders
            .Include(x => x.Service)
            .Where(x => x.EncounterId == invoice.EncounterId)
            .ToListAsync();

        ViewBag.Orders = orders;
        ViewBag.ExamFee = HisConstants.EXAM_FEE;

        return View(invoice);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int id)
    {
        var invoice = await _db.Invoices.FindAsync(id);
        if (invoice == null)
        {
            TempData["Error"] = "Khong tim thay hoa don!";
            return RedirectToAction(nameof(Index));
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            TempData["Error"] = "Hoa don da duoc thanh toan!";
            return RedirectToAction(nameof(Detail), new { id });
        }

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        invoice.PaidBy = User.Identity?.Name ?? "cashier";

        await _db.SaveChangesAsync();

        TempData["Success"] = "Da thu tien thanh cong!";
        return RedirectToAction(nameof(Detail), new { id });
    }
}

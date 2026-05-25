using HisMvc.Areas.Cashier.Models;
using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Areas.Cashier.Services;

/// <summary>
/// Doc du lieu cho View (Cashier Area) — không thay doi trạng thái luồng KCB.
/// </summary>
public class CashierViewService
{
    private readonly AppDbContext _db;

    public CashierViewService(AppDbContext db) => _db = db;

    public async Task<CashierDashboardViewModel> BuildDashboardAsync()
    {
        var todayStart = DateTime.Today;
        var todayEnd = todayStart.AddDays(1);

        var vm = new CashierDashboardViewModel
        {
            Kpi = new CashierKpiViewModel
            {
                UnpaidExamFee = await _db.Invoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid && i.InvoiceType == InvoiceType.ExamFee),
                UnpaidFinal = await _db.Invoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid && i.InvoiceType == InvoiceType.Final),
                Unpaid = await _db.Invoices.CountAsync(i => i.Status == InvoiceStatus.Unpaid),
                PaidToday = await _db.Invoices.CountAsync(i =>
                    i.Status == InvoiceStatus.Paid && i.PaidAt >= todayStart && i.PaidAt < todayEnd),
                RevenueToday = await _db.Invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= todayStart && i.PaidAt < todayEnd)
                    .SumAsync(i => i.PatientAmount),
                InsuranceToday = await _db.Invoices
                    .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= todayStart && i.PaidAt < todayEnd)
                    .SumAsync(i => i.InsuranceAmount)
            },
            UnpaidInvoices = await _db.Invoices
                .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
                .Where(i => i.Status == InvoiceStatus.Unpaid)
                .OrderBy(i => i.InvoiceType).ThenByDescending(i => i.CreatedAt)
                .Take(20)
                .ToListAsync()
        };

        vm.Activities = await BuildActivitiesAsync(todayStart);
        return vm;
    }

    public async Task<InvoiceIndexViewModel> GetInvoiceListAsync(string status, string type, DateOnly date)
    {
        var query = _db.Invoices
            .Include(x => x.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(x => x.Encounter)!.ThenInclude(e => e!.Doctor)
            .AsQueryable();

        if (status == "Unpaid") query = query.Where(x => x.Status == InvoiceStatus.Unpaid);
        else if (status == "Paid") query = query.Where(x => x.Status == InvoiceStatus.Paid);

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<InvoiceType>(type, out var t))
            query = query.Where(x => x.InvoiceType == t);

        query = query.Where(x => DateOnly.FromDateTime(x.CreatedAt) == date);

        return new InvoiceIndexViewModel
        {
            Invoices = await query.OrderByDescending(x => x.CreatedAt).ToListAsync(),
            CurrentStatus = status,
            CurrentType = type,
            Date = date
        };
    }

    public async Task<PendingPaymentViewModel> GetPendingPaymentsAsync(DateOnly date)
    {
        var encounters = await _db.Encounters
            .Include(x => x.Patient).Include(x => x.Doctor).Include(x => x.Department)
            .Where(x => x.Status == EncounterStatus.WaitingFinalPayment)
            .Where(x => DateOnly.FromDateTime(x.CheckInAt) == date)
            .OrderByDescending(x => x.EndAt)
            .ToListAsync();

        var encIds = encounters.Select(e => e.EncounterId).ToList();
        var finalInvoices = await _db.Invoices
            .Where(i => i.EncounterId != null && encIds.Contains(i.EncounterId.Value)
                && i.InvoiceType == InvoiceType.Final && i.Status == InvoiceStatus.Unpaid)
            .ToDictionaryAsync(i => i.EncounterId!.Value);

        return new PendingPaymentViewModel
        {
            Encounters = encounters,
            FinalInvoices = finalInvoices,
            Date = date
        };
    }

    public async Task<InvoiceDetailViewModel?> GetInvoiceDetailAsync(int invoiceId)
    {
        var invoice = await _db.Invoices
            .Include(x => x.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(x => x.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(x => x.Encounter)!.ThenInclude(e => e!.Department)
            .Include(x => x.PaidByStaff)
            .FirstOrDefaultAsync(x => x.InvoiceId == invoiceId);

        if (invoice == null) return null;

        var orders = await _db.Orders
            .Include(x => x.Service)
            .Where(x => x.EncounterId == invoice.EncounterId && x.Status != OrderStatus.Cancelled)
            .ToListAsync();

        var prescription = await _db.Prescriptions
            .Include(p => p.Items)!.ThenInclude(it => it.Medicine)
            .FirstOrDefaultAsync(p => p.EncounterId == invoice.EncounterId && p.Status != PrescriptionStatus.Cancelled);

        decimal examFee = invoice.ExamFeeAmount > 0
            ? invoice.ExamFeeAmount
            : invoice.InvoiceType == InvoiceType.ExamFee ? invoice.TotalAmount : 0m;

        return new InvoiceDetailViewModel
        {
            Invoice = invoice,
            Orders = orders,
            Prescription = prescription,
            ExamFee = examFee,
            InvoiceTypeLabel = InvoiceDisplay.TypeLabel(invoice.InvoiceType),
            NextStepHint = InvoiceDisplay.NextStepHint(invoice)
        };
    }

    private async Task<List<DashboardActivity>> BuildActivitiesAsync(DateTime todayStart)
    {
        var activities = new List<DashboardActivity>();

        var paidInvoices = await _db.Invoices
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
            .Where(i => i.Status == InvoiceStatus.Paid && i.PaidAt >= todayStart.AddDays(-1))
            .OrderByDescending(i => i.PaidAt).Take(10).ToListAsync();

        foreach (var i in paidInvoices)
        {
            activities.Add(new DashboardActivity
            {
                At = i.PaidAt!.Value,
                Icon = "bi-cash-coin",
                Title = $"Thu tien {i.InvoiceCode} ({InvoiceDisplay.TypeLabel(i.InvoiceType)})",
                Detail = $"{i.PatientAmount:N0} VND - {i.Encounter?.Patient?.FullName}",
                Url = $"/Cashier/Home/Detail/{i.InvoiceId}",
                Tag = "Đã thu",
                Priority = "success"
            });
        }

        var newInvoices = await _db.Invoices
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
            .Where(i => i.CreatedAt >= todayStart && i.Status == InvoiceStatus.Unpaid)
            .OrderByDescending(i => i.CreatedAt).Take(10).ToListAsync();

        foreach (var i in newInvoices)
        {
            activities.Add(new DashboardActivity
            {
                At = i.CreatedAt,
                Icon = "bi-receipt",
                Title = $"Hoa don moi {i.InvoiceCode} ({InvoiceDisplay.TypeLabel(i.InvoiceType)})",
                Detail = $"{i.TotalAmount:N0} VND - BN {i.Encounter?.Patient?.FullName}",
                Url = $"/Cashier/Home/Detail/{i.InvoiceId}",
                Tag = "Chờ thu",
                Priority = i.InvoiceType == InvoiceType.ExamFee ? "high" : "warning"
            });
        }

        return activities.OrderByDescending(x => x.At).Take(20).ToList();
    }
}

using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// Truy van DB dung chung cho cac buoc luồng KCB.
/// </summary>
internal class WorkflowDbHelper
{
    private readonly AppDbContext _db;

    public WorkflowDbHelper(AppDbContext db) => _db = db;

    public async Task<decimal> GetDefaultExamFeeAsync()
    {
        var examService = await _db.Services
            .Where(s => s.Type == "EXAM" && s.IsActive)
            .OrderBy(s => s.ServiceId)
            .FirstOrDefaultAsync();

        return examService?.Price ?? 50_000m;
    }

    public async Task<int> GetNextQueueNumberAsync(int doctorId, DateOnly date)
    {
        var dayStart = date.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        int? max = await _db.Encounters
            .Where(e => e.DoctorId == doctorId
                && e.QueuedAt.HasValue
                && e.QueuedAt.Value >= dayStart
                && e.QueuedAt.Value < dayEnd)
            .MaxAsync(e => (int?)e.QueueNumber);

        return (max ?? 0) + 1;
    }

    public async Task<decimal> CalculateServicesAmountAsync(int encounterId)
    {
        var orders = await _db.Orders
            .Include(o => o.Service)
            .Where(o => o.EncounterId == encounterId && o.Status != OrderStatus.Cancelled)
            .ToListAsync();

        return orders.Sum(o => (o.Service?.Price ?? 0) * Math.Max(1, o.Quantity));
    }

    public async Task<decimal> CalculateMedicineAmountAsync(int encounterId)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Items)!.ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.EncounterId == encounterId && p.Status != PrescriptionStatus.Cancelled);

        if (prescription == null || prescription.Items.Count == 0)
            return 0;

        var medicineIds = prescription.Items.Select(i => i.MedicineId).Distinct().ToList();
        var batchPrices = await _db.MedicineBatches
            .Where(b => medicineIds.Contains(b.MedicineId) && b.IsActive)
            .GroupBy(b => b.MedicineId)
            .Select(g => new { MedicineId = g.Key, Price = g.OrderByDescending(b => b.MedicineBatchId).Select(b => b.UnitPrice).FirstOrDefault() })
            .ToDictionaryAsync(x => x.MedicineId, x => x.Price);

        decimal total = 0;
        foreach (var item in prescription.Items)
        {
            decimal unitPrice = item.Medicine?.BhytPrice ?? 0;
            if (unitPrice <= 0 && batchPrices.TryGetValue(item.MedicineId, out var batchPrice))
                unitPrice = batchPrice;
            total += unitPrice * item.Quantity;
        }

        return total;
    }

    public async Task CreateOrUpdateFinalInvoiceAsync(Encounter encounter, decimal servicesAmount, decimal medicineAmount, decimal total)
    {
        var (insurancePay, patientPay) = BillingHelper.SplitByBhyt(total, encounter.Patient);

        var invoice = await _db.Invoices
            .FirstOrDefaultAsync(i => i.EncounterId == encounter.EncounterId && i.InvoiceType == InvoiceType.Final);

        if (invoice == null)
        {
            invoice = new Invoice
            {
                EncounterId = encounter.EncounterId,
                InvoiceCode = $"IV{DateTime.Now:yyyyMMddHHmmss}-FN",
                InvoiceType = InvoiceType.Final,
                Status = InvoiceStatus.Unpaid,
                CreatedAt = DateTime.UtcNow,
                Note = "Chi phí phát sinh (CLS + thuốc)"
            };
            _db.Invoices.Add(invoice);
        }

        invoice.TotalAmount = total;
        invoice.ServicesAmount = servicesAmount;
        invoice.MedicineAmount = medicineAmount;
        invoice.HasInsurance = BillingHelper.HasBhyt(encounter.Patient);
        invoice.InsuranceAmount = insurancePay;
        invoice.PatientAmount = patientPay;
    }
}

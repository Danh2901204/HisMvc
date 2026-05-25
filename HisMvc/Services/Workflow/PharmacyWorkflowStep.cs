using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// BUOC 9 — Nha thuốc cấp phát thuốc.
/// </summary>
public class PharmacyWorkflowStep
{
    private readonly AppDbContext _db;

    public PharmacyWorkflowStep(AppDbContext db) => _db = db;

    public static bool CanDispenseMedicine(Encounter? encounter, Prescription prescription)
    {
        if (prescription.Status != PrescriptionStatus.Pending)
            return false;
        if (encounter == null)
            return true;
        return encounter.Status == EncounterStatus.WaitingMedicine
            || encounter.Status == EncounterStatus.Completed;
    }

    public async Task<WorkflowResult> DispensePrescriptionAsync(int prescriptionId, int pharmacistStaffId, string? note)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.Encounter)
            .Include(p => p.Items)!.ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

        if (prescription == null)
            return WorkflowResult.Fail("Don thuốc không tồn tai.");

        if (prescription.Status != PrescriptionStatus.Pending)
            return WorkflowResult.Fail("Don thuốc đã cấp phát hoặc đã huy.");

        if (prescription.Encounter != null && !CanDispenseMedicine(prescription.Encounter, prescription))
            return WorkflowResult.Fail("BN chua thanh toán chi phí cuoi tai Thu ngan. Không thể cấp phát thuốc.");

        var medicineIds = prescription.Items.Select(i => i.MedicineId).Distinct().ToList();
        var allBatches = await _db.MedicineBatches
            .Where(b => medicineIds.Contains(b.MedicineId)
                && b.IsActive
                && b.ExpiryDate > DateTime.Today.AddMonths(1)
                && b.QuantityInStock > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();

        var batchesByMedicine = allBatches.GroupBy(b => b.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var dispense = new PharmacyDispense
            {
                PrescriptionId = prescriptionId,
                DispensedBy = pharmacistStaffId,
                DispensedAt = DateTime.UtcNow,
                Note = note,
                Items = new List<DispenseItem>()
            };

            foreach (var item in prescription.Items)
            {
                int remaining = item.Quantity;
                var batches = batchesByMedicine.GetValueOrDefault(item.MedicineId) ?? new List<MedicineBatch>();

                foreach (var batch in batches)
                {
                    if (remaining <= 0) break;
                    int take = Math.Min(remaining, batch.QuantityInStock);

                    dispense.Items.Add(new DispenseItem
                    {
                        MedicineBatchId = batch.MedicineBatchId,
                        Quantity = take,
                        UnitPrice = batch.UnitPrice,
                        TotalPrice = take * batch.UnitPrice
                    });

                    batch.QuantityInStock -= take;
                    _db.InventoryTransactions.Add(new InventoryTransaction
                    {
                        TransactionCode = $"OUT-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                        MedicineBatchId = batch.MedicineBatchId,
                        Type = TransactionType.Export,
                        Quantity = -take,
                        TransactionDate = DateTime.UtcNow,
                        StaffId = pharmacistStaffId,
                        CreatedBy = pharmacistStaffId,
                        Note = $"Cap phat đơn thuốc {prescription.Code}",
                        ReferenceCode = prescription.Code
                    });
                    remaining -= take;
                }

                if (remaining > 0)
                {
                    await transaction.RollbackAsync();
                    return WorkflowResult.Fail($"Không du ton kho cho thuốc: {item.Medicine?.Name}");
                }
            }

            prescription.Status = PrescriptionStatus.Dispensed;
            _db.PharmacyDispenses.Add(dispense);

            if (prescription.EncounterId.HasValue)
            {
                var encounter = await _db.Encounters.FindAsync(prescription.EncounterId.Value);
                if (encounter != null && encounter.Status == EncounterStatus.WaitingMedicine)
                {
                    encounter.Status = EncounterStatus.Completed;
                    encounter.EndAt = DateTime.UtcNow;
                }
            }

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return WorkflowResult.Ok("Cấp phát thuốc thành công! Lượt khám hoàn thành.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

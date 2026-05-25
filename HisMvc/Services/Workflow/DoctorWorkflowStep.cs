using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using HisMvc.Services;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// BUOC 4 & 7 — Bác sĩ gọi BN va chot lượt khám.
/// </summary>
public class DoctorWorkflowStep
{
    private readonly AppDbContext _db;
    private readonly WorkflowDbHelper _helper;
    private readonly Icd10Service _icd10;

    public DoctorWorkflowStep(AppDbContext db, Icd10Service icd10)
    {
        _db = db;
        _helper = new WorkflowDbHelper(db);
        _icd10 = icd10;
    }

    public async Task<WorkflowResult> CallPatientAsync(int encounterId, string? roomNumber)
    {
        var encounter = await _db.Encounters.FindAsync(encounterId);
        if (encounter == null)
            return WorkflowResult.Fail("Không tìm thay lượt khám!");

        if (encounter.Status != EncounterStatus.WaitingExam)
            return WorkflowResult.Fail("BN chua thanh toán phi kham hoặc đã được gọi.");

        encounter.Status = EncounterStatus.InService;
        encounter.StartedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(roomNumber))
            encounter.RoomNumber = roomNumber.Trim();

        await _db.SaveChangesAsync();
        return WorkflowResult.Ok($"Đã gọi STT {encounter.QueueNumber} vào phòng khám.");
    }

    public async Task<WorkflowResult> CloseEncounterAsync(int encounterId)
    {
        var encounter = await _db.Encounters
            .Include(e => e.Patient)
            .FirstOrDefaultAsync(e => e.EncounterId == encounterId);

        if (encounter == null)
            return WorkflowResult.Fail("Không tìm thay lượt khám!");

        if (encounter.Status == EncounterStatus.Completed || encounter.Status == EncounterStatus.Cancelled)
            return WorkflowResult.Fail("Lượt khám đã chốt/huy!");

        if (encounter.Status != EncounterStatus.InService && encounter.Status != EncounterStatus.WaitingResult)
            return WorkflowResult.Fail("Chi co the chot khi Đang khám (InService/WaitingResult).");

        if (string.IsNullOrWhiteSpace(encounter.Diagnosis))
            return WorkflowResult.Fail("Chưa có chẩn đoán! Vui lòng nhập chẩn đoán trước khi chot.");

        if (string.IsNullOrWhiteSpace(encounter.Icd10Primary))
        {
            await _icd10.ApplyParsedToEncounterAsync(encounter);
            if (!string.IsNullOrWhiteSpace(encounter.Icd10Primary))
                await _db.SaveChangesAsync();
        }

        if (string.IsNullOrWhiteSpace(encounter.Icd10Primary))
            return WorkflowResult.Fail("Bắt buộc nhập ma ICD-10 benh chinh (TT 56/2017).");

        int pendingOrders = await _db.Orders.CountAsync(o =>
            o.EncounterId == encounterId &&
            (o.Status == OrderStatus.Requested || o.Status == OrderStatus.InProgress));

        if (pendingOrders > 0)
            return WorkflowResult.Fail($"Còn {pendingOrders} chi dinh chua có kết quả! Không thể chot.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            decimal servicesAmount = await _helper.CalculateServicesAmountAsync(encounterId);
            decimal medicineAmount = await _helper.CalculateMedicineAmountAsync(encounterId);
            decimal totalExtra = servicesAmount + medicineAmount;

            if (totalExtra <= 0)
            {
                encounter.Status = EncounterStatus.Completed;
                encounter.EndAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                return WorkflowResult.Ok("Đã chot kham. Không phát sinh them chi phí — lượt khám hoàn thành.");
            }

            await _helper.CreateOrUpdateFinalInvoiceAsync(encounter, servicesAmount, medicineAmount, totalExtra);
            encounter.Status = EncounterStatus.WaitingFinalPayment;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return WorkflowResult.Ok("Đã chot kham. Tạo hóa đơn tong hop — chuyen BN sang Thu ngan thu chi phí phát sinh.");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

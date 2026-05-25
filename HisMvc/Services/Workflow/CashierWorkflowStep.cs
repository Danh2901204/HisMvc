using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// BUOC 2 & 8 — Thu ngan thu tien.
/// </summary>
public class CashierWorkflowStep
{
    private readonly AppDbContext _db;
    private readonly WorkflowDbHelper _helper;

    public CashierWorkflowStep(AppDbContext db)
    {
        _db = db;
        _helper = new WorkflowDbHelper(db);
    }

    public async Task<WorkflowResult> PayInvoiceAsync(int invoiceId, string paidByUserName, int? paidByStaffId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        if (invoice == null)
            return WorkflowResult.Fail("Không tìm thay hóa đơn!");

        if (invoice.Status == InvoiceStatus.Paid)
            return WorkflowResult.Fail("Hoa don da duoc thanh toán!");

        if (invoice.Status == InvoiceStatus.Cancelled)
            return WorkflowResult.Fail("Hoa don đã hủy.");

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;
            invoice.PaidBy = paidByUserName;
            invoice.PaidByStaffId = paidByStaffId;

            string message = invoice.InvoiceType switch
            {
                InvoiceType.ExamFee => await AfterExamFeePaidAsync(invoice),
                InvoiceType.Final => await AfterFinalPaidAsync(invoice),
                _ => "Đã thu tien thành công."
            };

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return WorkflowResult.Ok(message);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<string> AfterExamFeePaidAsync(Invoice invoice)
    {
        var encounter = invoice.Encounter;
        if (encounter == null)
            return "Đã thu phi kham.";

        if (encounter.Status != EncounterStatus.CheckedIn)
            return "Đã thu phi kham (trạng thái lượt khám không đổi).";

        int queueNumber = await _helper.GetNextQueueNumberAsync(encounter.DoctorId, DateOnly.FromDateTime(DateTime.Today));
        encounter.QueueNumber = queueNumber;
        encounter.QueuedAt = DateTime.UtcNow;
        encounter.Status = EncounterStatus.WaitingExam;

        return $"Thu phi kham OK. BN được cấp STT {queueNumber}, Đang chờ vào phòng khám.";
    }

    private async Task<string> AfterFinalPaidAsync(Invoice invoice)
    {
        var encounter = invoice.Encounter;
        if (encounter == null)
            return "Đã thu chi phí phát sinh.";

        if (encounter.Status != EncounterStatus.WaitingFinalPayment)
            return "Đã thu chi phí phát sinh (trạng thái lượt khám không đổi).";

        bool hasRx = await _db.Prescriptions.AnyAsync(p =>
            p.EncounterId == encounter.EncounterId && p.Status == PrescriptionStatus.Pending);

        if (hasRx)
        {
            encounter.Status = EncounterStatus.WaitingMedicine;
            return "Thu chi phí phát sinh OK. BN sang Nha thuốc linh thuốc.";
        }

        encounter.Status = EncounterStatus.Completed;
        encounter.EndAt = DateTime.UtcNow;
        return "Thu chi phí phát sinh OK. Lan kham hoàn thành (không có đơn thuốc).";
    }
}

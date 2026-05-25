using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services.Workflow;

/// <summary>
/// BUOC 1 — Tiep nhan check-in.
/// </summary>
public class ReceptionWorkflowStep
{
    private readonly AppDbContext _db;
    private readonly WorkflowDbHelper _helper;

    public ReceptionWorkflowStep(AppDbContext db)
    {
        _db = db;
        _helper = new WorkflowDbHelper(db);
    }

    public async Task<WorkflowResult> CheckInAsync(int appointmentId)
    {
        var appointment = await _db.Appointments
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

        if (appointment == null)
            return WorkflowResult.Fail("Không tìm thay lịch hẹn!");

        if (appointment.Status != AppointmentStatus.Booked)
            return WorkflowResult.Fail("Lịch hẹn không o trạng thái Booked!");

        if (appointment.DoctorId == null)
            return WorkflowResult.Fail("Lịch hẹn chua gan bac si!");

        if (await _db.Encounters.AnyAsync(e => e.AppointmentId == appointmentId))
            return WorkflowResult.Fail("Lịch hẹn da duoc check-in!");

        decimal examFee = await _helper.GetDefaultExamFeeAsync();
        var now = DateTime.UtcNow;

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var encounter = new Encounter
            {
                EncounterCode = $"LK{DateTime.Now:yyyyMMddHHmmss}",
                PatientId = appointment.PatientId,
                AppointmentId = appointment.AppointmentId,
                DoctorId = appointment.DoctorId.Value,
                DepartmentId = appointment.DepartmentId,
                Status = EncounterStatus.CheckedIn,
                CheckInAt = now,
                EndAt = now
            };
            _db.Encounters.Add(encounter);
            await _db.SaveChangesAsync();

            var (insurancePay, patientPay) = BillingHelper.SplitByBhyt(examFee, appointment.Patient);
            var examInvoice = new Invoice
            {
                EncounterId = encounter.EncounterId,
                InvoiceCode = $"IV{DateTime.Now:yyyyMMddHHmmss}-EF",
                InvoiceType = InvoiceType.ExamFee,
                TotalAmount = examFee,
                ExamFeeAmount = examFee,
                HasInsurance = BillingHelper.HasBhyt(appointment.Patient),
                InsuranceAmount = insurancePay,
                PatientAmount = patientPay,
                Status = InvoiceStatus.Unpaid,
                Note = "Phí khám ban đầu"
            };
            _db.Invoices.Add(examInvoice);

            appointment.Status = AppointmentStatus.CheckedIn;
            appointment.CheckedInAt = now;

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            string name = appointment.Patient?.FullName ?? "BN";
            return WorkflowResult.Ok(
                $"Check-in OK. {name} → quay Thu ngan thu phi kham ({examInvoice.InvoiceCode}, {patientPay:N0} VND).");
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}

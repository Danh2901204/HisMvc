using HisMvc.Models;

namespace HisMvc.Services.Workflow;

/*
 * Facade — Controller chi inject class nay.
 * Logic tung buoc nam o cac file *WorkflowStep.cs cung thu muc.
 *
 * BUOC 1  ReceptionWorkflowStep.CheckInAsync
 * BUOC 2,8 CashierWorkflowStep.PayInvoiceAsync
 * BUOC 4    DoctorWorkflowStep.CallPatientAsync
 * BUOC 7    DoctorWorkflowStep.CloseEncounterAsync
 * BUOC 6    LabWorkflowStep.SaveLabResultAsync
 * BUOC 9    PharmacyWorkflowStep.DispensePrescriptionAsync
 */
public class OutpatientWorkflowService
{
    private readonly ReceptionWorkflowStep _reception;
    private readonly CashierWorkflowStep _cashier;
    private readonly DoctorWorkflowStep _doctor;
    private readonly LabWorkflowStep _lab;
    private readonly PharmacyWorkflowStep _pharmacy;

    public OutpatientWorkflowService(
        ReceptionWorkflowStep reception,
        CashierWorkflowStep cashier,
        DoctorWorkflowStep doctor,
        LabWorkflowStep lab,
        PharmacyWorkflowStep pharmacy)
    {
        _reception = reception;
        _cashier = cashier;
        _doctor = doctor;
        _lab = lab;
        _pharmacy = pharmacy;
    }

    public Task<WorkflowResult> CheckInAsync(int appointmentId)
        => _reception.CheckInAsync(appointmentId);

    public Task<WorkflowResult> PayInvoiceAsync(int invoiceId, string paidByUserName, int? paidByStaffId)
        => _cashier.PayInvoiceAsync(invoiceId, paidByUserName, paidByStaffId);

    public Task<WorkflowResult> CallPatientAsync(int encounterId, string? roomNumber)
        => _doctor.CallPatientAsync(encounterId, roomNumber);

    public Task<WorkflowResult> CloseEncounterAsync(int encounterId)
        => _doctor.CloseEncounterAsync(encounterId);

    public Task<WorkflowResult> SaveLabResultAsync(int orderId, string resultText, string resultedBy, int? staffId)
        => _lab.SaveLabResultAsync(orderId, resultText, resultedBy, staffId);

    public Task<WorkflowResult> DispensePrescriptionAsync(int prescriptionId, int pharmacistStaffId, string? note)
        => _pharmacy.DispensePrescriptionAsync(prescriptionId, pharmacistStaffId, note);

    public static bool CanDispenseMedicine(Entities.Encounter? encounter, Entities.Prescription prescription)
        => PharmacyWorkflowStep.CanDispenseMedicine(encounter, prescription);
}

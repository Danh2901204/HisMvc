using HisMvc.Entities;

namespace HisMvc.Services.Workflow;

public static class BillingHelper
{
    public static (decimal insurancePay, decimal patientPay) SplitByBhyt(decimal totalAmount, Patient? patient)
    {
        if (patient == null || string.IsNullOrEmpty(patient.InsuranceNumber))
            return (0, totalAmount);

        if (patient.InsuranceCoveragePercent <= 0)
            return (0, totalAmount);

        decimal insurancePay = Math.Round(totalAmount * patient.InsuranceCoveragePercent / 100m, 0);
        return (insurancePay, totalAmount - insurancePay);
    }

    public static bool HasBhyt(Patient? patient)
        => patient != null && !string.IsNullOrEmpty(patient.InsuranceNumber);
}

using HisMvc.Entities;
using HisMvc.Models;

namespace HisMvc.Areas.Cashier.Models;

public class CashierDashboardViewModel
{
    public CashierKpiViewModel Kpi { get; set; } = new();
    public List<Invoice> UnpaidInvoices { get; set; } = new();
    public List<DashboardActivity> Activities { get; set; } = new();
}

public class CashierKpiViewModel
{
    public int UnpaidExamFee { get; set; }
    public int UnpaidFinal { get; set; }
    public int Unpaid { get; set; }
    public int PaidToday { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal InsuranceToday { get; set; }
}

public class InvoiceIndexViewModel
{
    public List<Invoice> Invoices { get; set; } = new();
    public string CurrentStatus { get; set; } = "";
    public string CurrentType { get; set; } = "";
    public DateOnly Date { get; set; }
}

public class PendingPaymentViewModel
{
    public List<Encounter> Encounters { get; set; } = new();
    public Dictionary<int, Invoice> FinalInvoices { get; set; } = new();
    public DateOnly Date { get; set; }
}

public class InvoiceDetailViewModel
{
    public Invoice Invoice { get; set; } = null!;
    public List<Order> Orders { get; set; } = new();
    public Prescription? Prescription { get; set; }
    public decimal ExamFee { get; set; }
    public string InvoiceTypeLabel { get; set; } = "";
    public string NextStepHint { get; set; } = "";
}

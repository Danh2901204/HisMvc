namespace HisMvc.Models.Common;

public class DonThuocPrintViewModel
{
    public HospitalPrintSettings Hospital { get; set; } = new();
    public string PatientCode { get; set; } = "";
    public string PrescriptionCode { get; set; } = "";
    public string EncounterCode { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string Dob { get; set; } = "";
    public string Gender { get; set; } = "";
    public string IdentityNumber { get; set; } = "";
    public string Address { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string DoctorNote { get; set; } = "";
    public DateTime PrescribedAt { get; set; }
    public List<DonThuocLineItem> Items { get; set; } = new();
}

public class DonThuocLineItem
{
    public int Index { get; set; }
    public string MedicineName { get; set; } = "";
    public string ActiveIngredient { get; set; } = "";
    public string Dosage { get; set; } = "";
    public string Instructions { get; set; } = "";
    public int Duration { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = "";
}

public class PhieuThuTienPrintViewModel
{
    public HospitalPrintSettings Hospital { get; set; } = new();
    public string Title { get; set; } = "PHIẾU THU TIỀN";
    public string PrescriptionCode { get; set; } = "";
    public string ExportCode { get; set; } = "";
    public string PatientCode { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string DoctorName { get; set; } = "";
    public string CashierName { get; set; } = "";
    public DateTime PrintTime { get; set; } = DateTime.Now;
    public List<PhieuThuLineItem> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public string TotalInWords { get; set; } = "";
    public string QrPayload { get; set; } = "";
}

public class PhieuThuLineItem
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public string Unit { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

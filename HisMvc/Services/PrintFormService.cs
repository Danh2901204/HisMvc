using HisMvc.Data;
using HisMvc.Entities;
using HisMvc.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HisMvc.Services;

public class PrintFormService
{
    private readonly AppDbContext _db;
    private readonly HospitalPrintSettings _hospital;

    public PrintFormService(AppDbContext db, IOptions<HospitalPrintSettings> hospital)
    {
        _db = db;
        _hospital = hospital.Value;
    }

    public async Task<DonThuocPrintViewModel?> BuildDonThuocAsync(int prescriptionId)
    {
        var rx = await LoadPrescriptionAsync(prescriptionId);
        if (rx == null) return null;

        var enc = rx.Encounter;
        var patient = enc?.Patient;

        return new DonThuocPrintViewModel
        {
            Hospital = _hospital,
            PatientCode = FormatPatientCode(patient),
            PrescriptionCode = rx.Code,
            EncounterCode = enc?.EncounterCode ?? "",
            PatientName = patient?.FullName ?? "",
            Dob = patient?.Dob?.ToString("dd/MM/yyyy") ?? "",
            Gender = FormatGender(patient?.Gender ?? Gender.Unknown),
            IdentityNumber = patient?.IdentityNumber ?? "",
            Address = patient?.Address ?? "",
            Diagnosis = BuildDiagnosis(enc),
            DoctorName = rx.Doctor?.FullName ?? "",
            DoctorNote = rx.Note ?? "",
            PrescribedAt = rx.PrescribedAt,
            Items = rx.Items.Select((item, i) => new DonThuocLineItem
            {
                Index = i + 1,
                MedicineName = item.Medicine?.Name ?? "",
                ActiveIngredient = item.Medicine?.ActiveIngredient ?? "",
                Dosage = item.Dosage,
                Instructions = item.Instructions ?? "",
                Duration = item.Duration,
                Quantity = item.Quantity,
                Unit = item.Medicine?.Unit ?? "Viên"
            }).ToList()
        };
    }

    public async Task<PhieuThuTienPrintViewModel?> BuildPhieuThuFromPrescriptionAsync(int prescriptionId, string? cashierName = null)
    {
        var rx = await LoadPrescriptionAsync(prescriptionId);
        if (rx == null) return null;

        var lines = new List<PhieuThuLineItem>();
        var total = await AppendPrescriptionLinesAsync(lines, rx.Items, startIndex: 1);
        return BuildPhieuThu(rx, lines, total, cashierName);
    }

    public async Task<PhieuThuTienPrintViewModel?> BuildPhieuThuForPrescriptionAsync(int prescriptionId, string? cashierName = null)
    {
        var dispenseId = await _db.PharmacyDispenses
            .Where(d => d.PrescriptionId == prescriptionId)
            .Select(d => d.PharmacyDispenseId)
            .FirstOrDefaultAsync();

        if (dispenseId > 0)
            return await BuildPhieuThuFromDispenseAsync(dispenseId);

        return await BuildPhieuThuFromPrescriptionAsync(prescriptionId, cashierName);
    }

    public async Task<PhieuThuTienPrintViewModel?> BuildPhieuThuFromInvoiceAsync(int invoiceId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(i => i.Encounter)!.ThenInclude(e => e!.Doctor)
            .Include(i => i.PaidByStaff)
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

        if (invoice == null) return null;

        var lines = new List<PhieuThuLineItem>();
        var idx = 1;
        decimal total = 0;
        Prescription? prescription = null;

        if (invoice.InvoiceType == InvoiceType.ExamFee)
        {
            total = AddLine(lines, ref idx, "Phí khám bệnh", "Lần", 1, invoice.PatientAmount);
        }
        else
        {
            if (invoice.ExamFeeAmount > 0)
                total += AddLine(lines, ref idx, "Phí khám bệnh", "Lần", 1, invoice.ExamFeeAmount);

            var orders = await _db.Orders
                .Include(o => o.Service)
                .Where(o => o.EncounterId == invoice.EncounterId && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            foreach (var order in orders)
            {
                var unitPrice = order.Service?.Price ?? 0;
                total += AddLine(lines, ref idx, order.Service?.Name ?? "Dịch vụ CLS", "Lần", order.Quantity, unitPrice);
            }

            prescription = await _db.Prescriptions
                .Include(p => p.Items).ThenInclude(i => i.Medicine)
                .FirstOrDefaultAsync(p => p.EncounterId == invoice.EncounterId && p.Status != PrescriptionStatus.Cancelled);

            if (prescription?.Items.Count > 0)
                total += await AppendPrescriptionLinesAsync(lines, prescription.Items, idx);
            else if (invoice.MedicineAmount > 0)
                total += AddLine(lines, ref idx, "Tiền thuốc theo đơn", "Đơn", 1, invoice.MedicineAmount);
        }

        if (lines.Count == 0)
            total = AddLine(lines, ref idx, "Thanh toán KCB", "Lần", 1, invoice.PatientAmount);

        var vm = new PhieuThuTienPrintViewModel
        {
            Hospital = _hospital,
            PrescriptionCode = prescription?.Code ?? invoice.InvoiceCode,
            ExportCode = invoice.InvoiceCode,
            PatientCode = FormatPatientCode(invoice.Encounter?.Patient),
            PatientName = invoice.Encounter?.Patient?.FullName ?? "",
            DoctorName = invoice.Encounter?.Doctor?.FullName ?? "",
            CashierName = invoice.PaidByStaff?.FullName ?? invoice.PaidBy ?? "",
            PrintTime = invoice.PaidAt ?? DateTime.Now,
            Items = lines,
            TotalAmount = total > 0 ? total : invoice.PatientAmount,
            QrPayload = invoice.InvoiceCode
        };
        vm.TotalInWords = VietnameseCurrencyHelper.ToWords((long)Math.Round(vm.TotalAmount, 0));
        return vm;
    }

    public async Task<PhieuThuTienPrintViewModel?> BuildPhieuThuFromDispenseAsync(int dispenseId)
    {
        var dispense = await _db.PharmacyDispenses
            .Include(d => d.Prescription)!.ThenInclude(p => p!.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(d => d.Prescription)!.ThenInclude(p => p!.Doctor)
            .Include(d => d.Pharmacist)
            .Include(d => d.Items)!.ThenInclude(i => i.MedicineBatch)!.ThenInclude(b => b!.Medicine)
            .FirstOrDefaultAsync(d => d.PharmacyDispenseId == dispenseId);

        if (dispense?.Prescription == null) return null;

        var lines = dispense.Items.Select((item, i) => new PhieuThuLineItem
        {
            Index = i + 1,
            Name = BuildMedicineDisplayName(item.MedicineBatch?.Medicine),
            Unit = item.MedicineBatch?.Medicine?.Unit ?? "Viên",
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.TotalPrice
        }).ToList();

        var total = lines.Sum(x => x.TotalPrice);
        return BuildPhieuThu(dispense.Prescription, lines, total, dispense.Pharmacist?.FullName, dispense.PharmacyDispenseId);
    }

    private PhieuThuTienPrintViewModel BuildPhieuThu(
        Prescription rx,
        List<PhieuThuLineItem> lines,
        decimal total,
        string? cashierName,
        int? dispenseId = null)
    {
        var vm = new PhieuThuTienPrintViewModel
        {
            Hospital = _hospital,
            PrescriptionCode = rx.Code,
            ExportCode = dispenseId.HasValue ? dispenseId.Value.ToString("D12") : rx.PrescriptionId.ToString("D12"),
            PatientCode = FormatPatientCode(rx.Encounter?.Patient),
            PatientName = rx.Encounter?.Patient?.FullName ?? "",
            DoctorName = rx.Doctor?.FullName ?? "",
            CashierName = cashierName ?? "",
            PrintTime = DateTime.Now,
            Items = lines,
            TotalAmount = total,
            QrPayload = rx.Code
        };
        vm.TotalInWords = VietnameseCurrencyHelper.ToWords((long)Math.Round(total, 0));
        return vm;
    }

    private async Task<decimal> AppendPrescriptionLinesAsync(
        List<PhieuThuLineItem> lines,
        IEnumerable<PrescriptionItem> items,
        int startIndex)
    {
        var idx = startIndex;
        decimal total = 0;

        foreach (var item in items)
        {
            var unitPrice = item.Medicine?.BhytPrice ?? await GetBatchPriceAsync(item.MedicineId);
            total += AddLine(lines, ref idx, BuildMedicineDisplayName(item.Medicine), item.Medicine?.Unit ?? "Viên", item.Quantity, unitPrice);
        }

        return total;
    }

    private static decimal AddLine(
        List<PhieuThuLineItem> lines,
        ref int index,
        string name,
        string unit,
        int quantity,
        decimal unitPrice)
    {
        var totalPrice = unitPrice * quantity;
        lines.Add(new PhieuThuLineItem
        {
            Index = index++,
            Name = name,
            Unit = unit,
            Quantity = quantity,
            UnitPrice = unitPrice,
            TotalPrice = totalPrice
        });
        return totalPrice;
    }

    private async Task<Prescription?> LoadPrescriptionAsync(int prescriptionId) =>
        await _db.Prescriptions
            .Include(p => p.Encounter)!.ThenInclude(e => e!.Patient)
            .Include(p => p.Doctor)
            .Include(p => p.Items).ThenInclude(i => i.Medicine)
            .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId);

    private async Task<decimal> GetBatchPriceAsync(int medicineId) =>
        await _db.MedicineBatches
            .Where(b => b.MedicineId == medicineId && b.IsActive)
            .OrderBy(b => b.ExpiryDate)
            .Select(b => b.UnitPrice)
            .FirstOrDefaultAsync();

    private static string FormatPatientCode(Patient? patient) =>
        patient?.PatientCode ?? patient?.PatientId.ToString("D10") ?? "";

    private static string FormatGender(Gender gender) => gender switch
    {
        Gender.Male => "Nam",
        Gender.Female => "Nữ",
        _ => ""
    };

    private static string BuildMedicineDisplayName(Medicine? med)
    {
        if (med == null) return "";
        if (!string.IsNullOrWhiteSpace(med.ActiveIngredient))
            return $"{med.Name} ({med.ActiveIngredient})";
        return med.Name;
    }

    private static string BuildDiagnosis(Encounter? enc)
    {
        if (enc == null) return "";
        if (!string.IsNullOrWhiteSpace(enc.Icd10PrimaryName))
            return $"{enc.Icd10Primary} - {enc.Icd10PrimaryName}";
        if (!string.IsNullOrWhiteSpace(enc.Icd10Primary))
            return enc.Icd10Primary;
        return enc.Diagnosis ?? "";
    }
}

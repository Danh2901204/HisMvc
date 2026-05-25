using HisMvc.Entities;

namespace HisMvc.Models.Common;

/// <summary>
/// Hien thi hóa đơn tren View — không chua logic nghiep vu.
/// </summary>
public static class InvoiceDisplay
{
    public static string TypeLabel(InvoiceType type) => type switch
    {
        InvoiceType.ExamFee => "Phí khám",
        InvoiceType.Services => "CLS",
        InvoiceType.Medicine => "Thuốc",
        InvoiceType.Final => "Tổng hop",
        InvoiceType.Inpatient => "Noi tru",
        _ => type.ToString()
    };

    public static string NextStepHint(Invoice invoice)
    {
        if (invoice.Status == InvoiceStatus.Paid)
        {
            if (invoice.InvoiceType == InvoiceType.ExamFee)
                return "BN da được cấp STT, chuyển sang phòng khám.";
            if (invoice.InvoiceType == InvoiceType.Final)
                return "BN chuyển sang Nha thuốc linh thuốc (nếu có don).";
            return "Đã hoan tat.";
        }

        if (invoice.InvoiceType == InvoiceType.ExamFee)
            return "Thu phí khám để cap STT va đưa BN vào hàng doi.";
        if (invoice.InvoiceType == InvoiceType.Final)
            return "Thu chi phí phát sinh (CLS + thuốc) sau khi BS chot kham.";
        return "Thu tien tu bệnh nhân.";
    }
}

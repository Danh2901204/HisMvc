using HisMvc.Entities;

namespace HisMvc.Services;

/// <summary>Danh mục khoa lâm sàng ngoại trú — đặt lịch công khai (Thông tư BYT).</summary>
public static class ClinicalDepartmentCatalog
{
    public static readonly IReadOnlyList<(string Code, string Name)> OutpatientClinical =
    [
        ("KB", "Khoa Khám bệnh"),
        ("NOI", "Khoa Nội tổng hợp"),
        ("NGOAI", "Khoa Ngoại tổng hợp"),
        ("SAN", "Khoa Sản"),
        ("NHI", "Khoa Nhi"),
        ("TIM", "Khoa Tim mạch"),
        ("TMH", "Khoa Tai Mũi Họng"),
        ("MAT", "Khoa Mắt"),
        ("RHM", "Khoa Răng Hàm Mặt"),
        ("DA", "Khoa Da liễu"),
        ("YHCT", "Khoa Y học Cổ truyền"),
        ("NOITIET", "Khoa Nội tiết"),
        ("TIEUHOA", "Khoa Tiêu hóa"),
        ("HH", "Khoa Hô hấp"),
        ("TK", "Khoa Thần kinh")
    ];

    private static readonly HashSet<string> OutpatientCodes =
        OutpatientClinical.Select(x => x.Code).ToHashSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsOutpatientClinical(string? code) =>
        !string.IsNullOrWhiteSpace(code) && OutpatientCodes.Contains(code);

    public static string NormalizeNameKey(string name)
    {
        var n = name.Trim().ToLowerInvariant();
        if (n.StartsWith("khoa "))
            n = n[5..];
        return n.Replace(" ", "");
    }
}

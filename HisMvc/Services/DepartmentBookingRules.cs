using HisMvc.Entities;

namespace HisMvc.Services;

/// <summary>
/// Quy tắc lọc khoa/phòng cho đặt lịch khám ngoại trú công khai (Thông tư BYT).
/// </summary>
public static class DepartmentBookingRules
{
    public static IQueryable<Department> BookableForPublic(IQueryable<Department> query) =>
        query.Where(d => d.Kind == DepartmentKind.Clinical);

    public static string GetKindLabel(DepartmentKind kind) => kind switch
    {
        DepartmentKind.Clinical => "Khoa lâm sàng (đặt lịch được)",
        DepartmentKind.Administrative => "Phòng hành chính / quản trị",
        DepartmentKind.Paraclinical => "Khoa cận lâm sàng",
        DepartmentKind.InpatientOnly => "Nội trú / hồi sức",
        _ => "Không xác định"
    };
}

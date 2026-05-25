using System.ComponentModel.DataAnnotations;

namespace HisMvc.Entities;

/// <summary>Danh muc ICD-10 tra cuu khi kham (TT 56/2017).</summary>
public class Icd10Catalog
{
    [Key, MaxLength(10)]
    public string Code { get; set; } = "";

    [MaxLength(300)]
    public string Name { get; set; } = "";

    [MaxLength(80)]
    public string? Chapter { get; set; }

    /// <summary>Benh hay gap — uu tien hien thi khi tim kiem.</summary>
    public bool IsCommon { get; set; } = true;
}

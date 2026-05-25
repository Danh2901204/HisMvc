namespace HisMvc.Models;

/// <summary>
/// Constants for HIS system
/// </summary>
public static class HisConstants
{
    /// <summary>
    /// Phí khám c? ??nh
    /// </summary>
    public const decimal EXAM_FEE = 100000;
    
    /// <summary>
    /// T? l? BHYT chi tr? m?c ??nh
    /// </summary>
    public const decimal DEFAULT_INSURANCE_COVERAGE = 80;
    
    /// <summary>
    /// S? tháng c?nh báo thu?c s?p h?t h?n
    /// </summary>
    public const int MEDICINE_EXPIRY_WARNING_MONTHS = 3;
    
    /// <summary>
    /// S? tháng t?i thi?u còn h?n khi c?p phát thu?c
    /// </summary>
    public const int MEDICINE_MIN_EXPIRY_MONTHS = 1;
    
    /// <summary>
    /// ?? dài mã th? BHYT
    /// </summary>
    public const int INSURANCE_NUMBER_LENGTH = 15;
    
    /// <summary>
    /// Các lo?i th? BHYT
    /// </summary>
    public static class InsuranceTypes
    {
        public const string KC = "KC"; // Khám ch?a b?nh
        public const string QN = "QN"; // Quân nhân
        public const string TE = "TE"; // Tr? em d??i 6 tu?i
        public const string CB = "CB"; // Công ch?c viên ch?c
        public const string NN = "NN"; // Nông dân
    }
}

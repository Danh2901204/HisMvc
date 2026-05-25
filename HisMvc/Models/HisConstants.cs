namespace HisMvc.Models;

/// <summary>
/// Constants for HIS system
/// </summary>
public static class HisConstants
{
    /// <summary>
    /// Phí khám co dinh
    /// </summary>
    public const decimal EXAM_FEE = 100000;
    
    /// <summary>
    /// Tỷ lệ BHYT chi trả mặc định
    /// </summary>
    public const decimal DEFAULT_INSURANCE_COVERAGE = 80;
    
    /// <summary>
    /// So tháng canh bao thuốc sắp hết hạn
    /// </summary>
    public const int MEDICINE_EXPIRY_WARNING_MONTHS = 3;
    
    /// <summary>
    /// So tháng toi thieu còn hạn khi cấp phát thuốc
    /// </summary>
    public const int MEDICINE_MIN_EXPIRY_MONTHS = 1;
    
    /// <summary>
    /// Do dai ma the BHYT
    /// </summary>
    public const int INSURANCE_NUMBER_LENGTH = 15;
    
    /// <summary>
    /// Cac loại thẻ BHYT
    /// </summary>
    public static class InsuranceTypes
    {
        public const string KC = "KC"; // Kham chua benh
        public const string QN = "QN"; // Quan nhan
        public const string TE = "TE"; // Tre em duoi 6 tuổi
        public const string CB = "CB"; // Cong chuc vien chuc
        public const string NN = "NN"; // Nong dan
    }
}

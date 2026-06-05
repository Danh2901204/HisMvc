using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using HisMvc.Entities;

namespace HisMvc.Services.Chatbot;

/// <summary>Tiện ích parse văn bản tiếng Việt cho chatbot.</summary>
public static class ChatbotTextHelper
{
    public static readonly Regex PhoneRegex = new(@"^0\d{9}$", RegexOptions.Compiled);
    public static readonly Regex AppointmentCodeRegex = new(@"(?i)APT\s*\d{10,20}", RegexOptions.Compiled);

    public static bool Matches(string text, params string[] keywords)
    {
        var normalized = Normalize(text);
        return keywords.Any(k => normalized.Contains(NormalizeKeyword(k), StringComparison.Ordinal));
    }

    public static bool IsConfirm(string message) =>
        Matches(message, "xac nhan", "dong y", "ok", "co", "yes", "1");

    public static string? ExtractAppointmentCode(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;
        var match = AppointmentCodeRegex.Match(text);
        if (!match.Success) return null;
        return Regex.Replace(match.Value, @"\s+", "").ToUpperInvariant();
    }

    public static int? ParseChoiceIndex(string message)
    {
        var match = Regex.Match(message.Trim(), @"^(\d{1,2})\b");
        return match.Success && int.TryParse(match.Groups[1].Value, out var index) ? index : null;
    }

    public static DateOnly? ParseDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        var text = input.Trim().ToLowerInvariant();
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (text.Contains("hom nay") || text.Contains("hôm nay")) return today;
        if (text.Contains("ngay mai") || text.Contains("ngày mai")) return today.AddDays(1);
        if (text.Contains("ngay kia") || text.Contains("ngày kia")) return today.AddDays(2);

        foreach (var format in new[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "yyyy-MM-dd" })
        {
            if (DateOnly.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var exact))
                return exact;
        }

        return DateOnly.TryParse(text, CultureInfo.GetCultureInfo("vi-VN"), DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    public static Gender? ParseGender(string message)
    {
        var normalized = Normalize(message);
        if (normalized is "nam" or "male" or "1") return Gender.Male;
        if (normalized is "nu" or "female" or "2") return Gender.Female;
        return null;
    }

    public static string NormalizePhone(string input) =>
        Regex.Replace(input ?? "", @"[^\d]", "");

    public static bool IsValidPhone(string phone) => PhoneRegex.IsMatch(phone);

    public static string Normalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";

        var normalized = input.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return Regex.Replace(builder.ToString(), @"\s+", " ").Trim();
    }

    private static string NormalizeKeyword(string value) =>
        Regex.Replace(value.ToLowerInvariant(), @"[^a-z0-9\s]", "").Trim();
}

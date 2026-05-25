using System.Text.RegularExpressions;
using HisMvc.Data;
using HisMvc.Entities;
using Microsoft.EntityFrameworkCore;

namespace HisMvc.Services;

public class Icd10Service
{
    private static readonly Regex CodeRegex = new(
        @"\b([A-TV-Z][0-9]{2}(?:\.[0-9]{1,2})?)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly AppDbContext _db;

    public Icd10Service(AppDbContext db) => _db = db;

    public async Task<List<Icd10Catalog>> SearchAsync(string? query, int limit = 15)
    {
        query = (query ?? "").Trim();
        if (query.Length < 1)
        {
            return await _db.Icd10Catalogs
                .Where(x => x.IsCommon)
                .OrderBy(x => x.Code)
                .Take(limit)
                .ToListAsync();
        }

        var upper = query.ToUpperInvariant();
        var normalized = RemoveDiacritics(query).ToLowerInvariant();

        return await _db.Icd10Catalogs
            .Where(x =>
                x.Code.StartsWith(upper) ||
                x.Name.Contains(query) ||
                x.Name.ToLower().Contains(normalized))
            .OrderByDescending(x => x.IsCommon)
            .ThenBy(x => x.Code.StartsWith(upper) ? 0 : 1)
            .ThenBy(x => x.Code)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<Icd10ParseResult> ParseDiagnosisAsync(string? diagnosis)
    {
        var result = new Icd10ParseResult();
        if (string.IsNullOrWhiteSpace(diagnosis))
            return result;

        var matches = CodeRegex.Matches(diagnosis);
        if (matches.Count == 0)
            return result;

        var codes = matches
            .Select(m => m.Groups[1].Value.ToUpperInvariant())
            .Distinct()
            .ToList();

        result.PrimaryCode = codes[0];
        if (codes.Count > 1)
            result.SecondaryCodes = string.Join(", ", codes.Skip(1));

        result.PrimaryName = ExtractNameForCode(diagnosis, result.PrimaryCode)
            ?? await LookupNameAsync(result.PrimaryCode);

        return result;
    }

    public async Task ApplyParsedToEncounterAsync(Encounter enc)
    {
        if (!string.IsNullOrWhiteSpace(enc.Icd10Primary))
            return;

        var parsed = await ParseDiagnosisAsync(enc.Diagnosis);
        if (string.IsNullOrWhiteSpace(parsed.PrimaryCode))
            return;

        enc.Icd10Primary = parsed.PrimaryCode;
        enc.Icd10PrimaryName ??= parsed.PrimaryName;
        if (string.IsNullOrWhiteSpace(enc.Icd10Secondary) && !string.IsNullOrWhiteSpace(parsed.SecondaryCodes))
            enc.Icd10Secondary = parsed.SecondaryCodes;
    }

    private async Task<string?> LookupNameAsync(string code)
    {
        var item = await _db.Icd10Catalogs.FindAsync(code);
        return item?.Name;
    }

    private static string? ExtractNameForCode(string diagnosis, string code)
    {
        var pattern = $@"{Regex.Escape(code)}\s*[-–—:]\s*([^/|\n\r]+)";
        var m = Regex.Match(diagnosis, pattern, RegexOptions.IgnoreCase);
        if (!m.Success)
            return null;

        var name = m.Groups[1].Value.Trim();
        return name.Length > 200 ? name[..200] : name;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(System.Text.NormalizationForm.FormD);
        var chars = normalized.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
            != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC);
    }
}

public class Icd10ParseResult
{
    public string? PrimaryCode { get; set; }
    public string? PrimaryName { get; set; }
    public string? SecondaryCodes { get; set; }
}

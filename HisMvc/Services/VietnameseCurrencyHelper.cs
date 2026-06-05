namespace HisMvc.Services;

public static class VietnameseCurrencyHelper
{
    private static readonly string[] Units = { "", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };

    public static string FormatNumber(decimal amount) =>
        ((long)Math.Round(amount, 0)).ToString("N0").Replace(",", ".");

    public static string ToWords(long number)
    {
        if (number == 0) return "Không đồng";
        if (number < 0) return "Âm " + ToWords(-number);

        var parts = new List<string>();
        var units = new[] { "", " nghìn", " triệu", " tỷ" };
        var i = 0;

        while (number > 0)
        {
            var chunk = (int)(number % 1000);
            if (chunk > 0)
            {
                var chunkText = ReadChunk(chunk);
                if (i > 0 && chunk < 100 && number >= 1000)
                    chunkText = "lẻ " + chunkText;
                parts.Insert(0, chunkText + units[i]);
            }
            number /= 1000;
            i++;
        }

        var text = string.Join("", parts).Trim();
        if (string.IsNullOrEmpty(text)) text = "không";
        return char.ToUpper(text[0]) + text[1..] + " đồng";
    }

    private static string ReadChunk(int number)
    {
        var hundred = number / 100;
        var ten = (number % 100) / 10;
        var unit = number % 10;
        var result = "";

        if (hundred > 0)
            result += Units[hundred] + " trăm";

        if (ten > 1)
        {
            result += (result.Length > 0 ? " " : "") + Units[ten] + " mươi";
            if (unit == 1) result += " mốt";
            else if (unit == 5) result += " lăm";
            else if (unit > 0) result += " " + Units[unit];
        }
        else if (ten == 1)
        {
            result += (result.Length > 0 ? " " : "") + "mười";
            if (unit == 5) result += " lăm";
            else if (unit > 0) result += " " + Units[unit];
        }
        else if (ten == 0 && unit > 0)
        {
            if (hundred > 0) result += " lẻ";
            result += (result.Length > 0 ? " " : "") + Units[unit];
        }

        return result;
    }
}

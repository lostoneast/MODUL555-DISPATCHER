using ClosedXML.Excel;

namespace DispatcherApp.Infrastructure.Services;

public static class ExcelHelper
{
    public static byte[] Export(IReadOnlyList<IReadOnlyDictionary<string, object?>> rows)
    {
        if (rows.Count == 0)
            return Export([], Array.Empty<IReadOnlyList<object?>>());

        var headers = rows[0].Keys.ToList();
        var data = rows
            .Select(r => (IReadOnlyList<object?>)headers.Select(h => r.TryGetValue(h, out var v) ? v : null).ToList())
            .ToList();
        return Export(headers, data);
    }

    public static byte[] Export(IReadOnlyList<string> headers, IEnumerable<IReadOnlyList<object?>> rows)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Data");

        for (var c = 0; c < headers.Count; c++)
            sheet.Cell(1, c + 1).Value = headers[c];

        var rowIndex = 2;
        foreach (var row in rows)
        {
            for (var c = 0; c < headers.Count; c++)
            {
                var value = c < row.Count ? row[c] : null;
                SetCellValue(sheet.Cell(rowIndex, c + 1), value);
            }

            rowIndex++;
        }

        if (headers.Count > 0)
            sheet.Columns(1, headers.Count).AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public static List<Dictionary<string, string>> ReadRows(Stream stream)
    {
        using var workbook = new XLWorkbook(stream);
        var sheet = workbook.Worksheets.First();
        var range = sheet.RangeUsed();
        if (range is null)
            return [];

        var firstRow = range.FirstRow();
        var colCount = range.ColumnCount();
        var headers = new string[colCount];
        for (var c = 1; c <= colCount; c++)
        {
            var header = firstRow.Cell(c).GetString().Trim();
            headers[c - 1] = string.IsNullOrEmpty(header) ? $"Column{c}" : header;
        }

        var result = new List<Dictionary<string, string>>();
        foreach (var row in range.RowsUsed().Skip(1))
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var empty = true;
            for (var c = 1; c <= colCount; c++)
            {
                var text = row.Cell(c).GetFormattedString().Trim();
                if (!string.IsNullOrEmpty(text))
                    empty = false;
                dict[headers[c - 1]] = text;
            }

            if (!empty)
                result.Add(dict);
        }

        return result;
    }

    public static string GetString(IReadOnlyDictionary<string, string> row, string key, string defaultValue = "")
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return defaultValue;
        return value.Trim();
    }

    public static string? GetStringOrNull(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;
        return value.Trim();
    }

    public static int? GetInt(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;
        return int.TryParse(value.Trim(), out var n) ? n : null;
    }

    public static long? GetLong(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;
        return long.TryParse(value.Trim(), out var n) ? n : null;
    }

    public static decimal? GetDecimal(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;
        return decimal.TryParse(value.Trim(), out var n) ? n : null;
    }

    public static bool? GetBool(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;

        var v = value.Trim();
        if (bool.TryParse(v, out var b))
            return b;
        if (v is "1" or "yes" or "y" or "да" or "истина")
            return true;
        if (v is "0" or "no" or "n" or "нет" or "ложь")
            return false;
        return null;
    }

    public static DateOnly? GetDateOnly(IReadOnlyDictionary<string, string> row, string key)
    {
        if (!TryGet(row, key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;
        if (DateOnly.TryParse(value.Trim(), out var d))
            return d;
        if (DateTime.TryParse(value.Trim(), out var dt))
            return DateOnly.FromDateTime(dt);
        return null;
    }

    private static bool TryGet(IReadOnlyDictionary<string, string> row, string key, out string value)
    {
        if (row.TryGetValue(key, out value!))
            return true;

        foreach (var pair in row)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = "";
        return false;
    }

    private static void SetCellValue(IXLCell cell, object? value)
    {
        switch (value)
        {
            case null:
                cell.Value = Blank.Value;
                break;
            case string s:
                cell.Value = s;
                break;
            case bool b:
                cell.Value = b;
                break;
            case int i:
                cell.Value = i;
                break;
            case long l:
                cell.Value = l;
                break;
            case decimal d:
                cell.Value = d;
                break;
            case double db:
                cell.Value = db;
                break;
            case float f:
                cell.Value = f;
                break;
            case DateOnly dateOnly:
                cell.Value = dateOnly.ToDateTime(TimeOnly.MinValue);
                cell.Style.DateFormat.Format = "yyyy-MM-dd";
                break;
            case DateTime dt:
                cell.Value = dt;
                break;
            case DateTimeOffset dto:
                cell.Value = dto.UtcDateTime;
                break;
            case Enum e:
                cell.Value = e.ToString();
                break;
            default:
                cell.Value = value.ToString();
                break;
        }
    }
}

using System.Globalization;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ObjectProductImportService(DispatcherDbContext db)
{
    // Keep the two naming rules here so they can be changed independently of Excel and HTTP.
    public static string GenerateProductCode(string mark, string suffix, long number)
    {
        var code = string.Join("-", new[] { mark.Trim(), suffix.Trim(), number.ToString("D3", CultureInfo.InvariantCulture) }
            .Where(x => x.Length > 0));
        if (code.Length > 64)
            throw new ArgumentException("Сформированный код длиннее 64 символов. Сократите марку или суффикс.");
        return code;
    }

    public static string GetProductTypeCode(string mark)
    {
        var prefix = Regex.Match(mark.Trim(), @"^\p{L}+").Value.ToUpperInvariant();
        if (prefix.Length == 0)
            throw new ArgumentException("Не удалось определить тип по началу марки. Укажите тип изделия в файле.");
        if (prefix.Length > 64) throw new ArgumentException("Код типа изделия длиннее 64 символов.");
        return prefix;
    }

    public async Task<ImportResult> ImportAsync(int objectId, Stream stream, string? suffix, int startNumber, CancellationToken ct)
    {
        suffix = suffix?.Trim() ?? "";
        if (suffix.Length > 20 || startNumber < 1)
            throw new ArgumentException("Суффикс — до 20 символов, начальный номер — положительное целое число.");
        var obj = await db.ConstructionObjects.AsNoTracking().SingleOrDefaultAsync(x => x.Id == objectId, ct)
            ?? throw new KeyNotFoundException();
        if (!obj.IsActive) throw new ConflictException("Нельзя импортировать изделия в неактивный объект.");

        XLWorkbook workbook;
        try { workbook = new XLWorkbook(stream); }
        catch (Exception ex) when (ex is not OperationCanceledException and not OutOfMemoryException)
        { throw new ArgumentException("Не удалось прочитать Excel. Используйте файл .xlsx по примеру импорта.", ex); }
        using var workbookScope = workbook;
        var sheet = workbook.Worksheets.FirstOrDefault() ?? throw new ArgumentException("В файле нет листов.");
        var range = sheet.RangeUsed() ?? throw new ArgumentException("Файл пуст.");
        if (range.RowCount() > 5001) throw new ArgumentException("За один импорт можно добавить не более 5000 изделий.");
        var headers = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var cell in range.FirstRow().Cells())
        {
            var header = cell.GetString().Trim();
            if (header.Length > 0 && !headers.TryAdd(header, cell.Address.ColumnNumber))
                throw new ArgumentException($"Повторяющийся заголовок: {header}.");
        }
        if (!headers.ContainsKey("Марка") && !headers.ContainsKey("Mark"))
            throw new ArgumentException("Не найден столбец «Марка» (Mark).");

        string? Text(IXLRow row, string english, string russian)
        {
            var column = headers.GetValueOrDefault(english, headers.GetValueOrDefault(russian));
            if (column == 0) return null;
            var cell = row.Cell(column);
            if (cell.HasFormula) throw new ArgumentException($"«{russian}»: замените формулу её значением.");
            var value = cell.GetFormattedString(CultureInfo.InvariantCulture).Trim();
            return value.Length == 0 ? null : value;
        }
        decimal? Number(IXLRow row, string english, string russian)
        {
            var text = Text(row, english, russian);
            if (text == null) return null;
            var column = headers.GetValueOrDefault(english, headers.GetValueOrDefault(russian));
            if (row.Cell(column).DataType == XLDataType.Number && row.Cell(column).TryGetValue<decimal>(out var numeric))
                return numeric;
            var normalized = text.Replace("\u00a0", "").Replace(" ", "").Replace(',', '.');
            if (!decimal.TryParse(normalized, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
                throw new ArgumentException($"«{russian}»: некорректное число «{text}».");
            return value;
        }
        int? Integer(IXLRow row, string english, string russian)
        {
            var value = Number(row, english, russian);
            if (value == null) return null;
            if (value < 1 || value > int.MaxValue || decimal.Truncate(value.Value) != value)
                throw new ArgumentException($"«{russian}»: требуется положительное целое число.");
            return (int)value.Value;
        }

        var rows = Enumerable.Range(range.RangeAddress.FirstAddress.RowNumber + 1, range.RowCount() - 1)
            .Select(sheet.Row).Where(r => !r.IsEmpty()).ToList();
        if (rows.Count == 0) throw new ArgumentException("В файле нет изделий.");
        // Reserve explicit codes in advance: generated codes must not steal a later row's code.
        var reservedCodes = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            var column = headers.GetValueOrDefault("ProductCode", headers.GetValueOrDefault("Код изделия"));
            if (column > 0 && !row.Cell(column).HasFormula) reservedCodes.Add(row.Cell(column).GetFormattedString().Trim());
        }
        var usedCodes = new HashSet<string>(StringComparer.Ordinal);
        var types = await db.ProductTypes.ToListAsync(ct);
        var sections = await db.BuildingSections.Where(x => x.ConstructionObjectId == objectId).Select(x => x.Id).ToListAsync(ct);
        var floors = await db.Floors.Where(x => x.BuildingSection.ConstructionObjectId == objectId).Select(x => new { x.Id, x.BuildingSectionId }).ToListAsync(ct);
        var result = new ImportResult();
        long nextNumber = startNumber;
        var now = DateTimeOffset.UtcNow;
        foreach (var row in rows)
        {
            ct.ThrowIfCancellationRequested();
            try
            {
                var mark = Text(row, "Mark", "Марка") ?? throw new ArgumentException("Не указана марка.");
                var code = Text(row, "ProductCode", "Код изделия");
                if (code == null)
                {
                    do { code = GenerateProductCode(mark, suffix, nextNumber++); }
                    while (reservedCodes.Contains(code) || usedCodes.Contains(code) || await db.Products.AnyAsync(x => x.ProductCode == code, ct));
                }
                if (!usedCodes.Add(code) || await db.Products.AnyAsync(x => x.ProductCode == code, ct))
                    throw new ArgumentException($"Код «{code}» уже существует или повторяется в файле.");

                var typeId = Integer(row, "ProductTypeId", "ID типа изделия");
                var typeCode = Text(row, "ProductTypeCode", "Тип изделия");
                if (!typeId.HasValue) typeCode ??= GetProductTypeCode(mark);
                if (typeCode?.Length > 64) throw new ArgumentException("Код типа изделия длиннее 64 символов.");
                var type = typeId.HasValue ? types.SingleOrDefault(x => x.Id == typeId.Value)
                    : types.FirstOrDefault(x => string.Equals(x.Code, typeCode, StringComparison.OrdinalIgnoreCase));
                if (typeId.HasValue && type == null) throw new ArgumentException($"Тип изделия с ID {typeId} не найден.");
                if (type is { IsActive: false }) throw new ArgumentException($"Тип «{type.Code}» неактивен.");

                var dto = new ObjectProductWriteDto
                {
                    ProductCode = code, Mark = mark,
                    WidthMm = Number(row, "WidthMm", "Ширина, мм") ?? 0,
                    HeightMm = Number(row, "HeightMm", "Высота, мм") ?? 0,
                    ThicknessMm = Number(row, "ThicknessMm", "Толщина, мм") ?? 0,
                    CorniceWidthIncreaseMm = Number(row, "CorniceWidthIncreaseMm", "Припуск карниза, мм") ?? 0,
                    TotalWidthWithCorniceMm = Number(row, "TotalWidthWithCorniceMm", "Ширина с карнизом, мм") ?? 0,
                    ThicknessIncreaseMm = Number(row, "ThicknessIncreaseMm", "Припуск толщины, мм") ?? 0,
                    RightBendMm = Number(row, "RightBendMm", "Правый загиб, мм") ?? 0,
                    LeftBendMm = Number(row, "LeftBendMm", "Левый загиб, мм") ?? 0,
                    CladdingWidthWithBendsMm = Number(row, "CladdingWidthWithBendsMm", "Обшивка с загибами, мм") ?? 0,
                    WeightKg = Number(row, "WeightKg", "Масса, кг"),
                    InstallationNumber = Text(row, "InstallationNumber", "Монтажный номер"),
                    AdditionalInfo = Text(row, "AdditionalInfo", "Примечание"),
                    BuildingSectionId = Integer(row, "BuildingSectionId", "ID секции"),
                    FloorId = Integer(row, "FloorId", "ID этажа"),
                };
                ObjectProductsService.Validate(dto);
                if (dto.BuildingSectionId.HasValue && !sections.Contains(dto.BuildingSectionId.Value))
                    throw new ArgumentException("Секция не принадлежит открытому объекту.");
                if (dto.FloorId.HasValue && !floors.Any(x => x.Id == dto.FloorId && x.BuildingSectionId == dto.BuildingSectionId))
                    throw new ArgumentException("Этаж не принадлежит выбранной секции объекта.");
                if (type == null)
                {
                    type = new ProductType { Code = typeCode!.ToUpperInvariant(), Name = typeCode.ToUpperInvariant(), IsActive = true };
                    db.ProductTypes.Add(type);
                    types.Add(type);
                }
                db.Products.Add(new Product
                {
                    ProductCode = code, ProductType = type, Mark = mark,
                    WidthMm = dto.WidthMm, HeightMm = dto.HeightMm, ThicknessMm = dto.ThicknessMm,
                    CorniceWidthIncreaseMm = dto.CorniceWidthIncreaseMm, TotalWidthWithCorniceMm = dto.TotalWidthWithCorniceMm,
                    ThicknessIncreaseMm = dto.ThicknessIncreaseMm, RightBendMm = dto.RightBendMm, LeftBendMm = dto.LeftBendMm,
                    CladdingWidthWithBendsMm = dto.CladdingWidthWithBendsMm, WeightKg = dto.WeightKg,
                    AdditionalInfo = dto.AdditionalInfo, Status = ProductStatus.Created, Version = 1, CreatedAt = now, UpdatedAt = now,
                    ProjectPosition = new ProjectPosition { ConstructionObjectId = objectId,
                        BuildingSectionId = dto.BuildingSectionId, FloorId = dto.FloorId, InstallationNumber = dto.InstallationNumber },
                });
            }
            catch (ArgumentException ex) { result.Errors.Add($"Строка {row.RowNumber()}: {ex.Message}"); }
        }
        // Save the complete file in one EF transaction. Errors never leave half an import or orphan types.
        if (result.Errors.Count > 0) return result;
        await db.SaveChangesAsync(ct);
        result.Created = rows.Count;
        return result;
    }

    public static byte[] ExampleFile() => ExcelHelper.Export(
        ["Марка", "Код изделия", "Тип изделия", "Монтажный номер", "Ширина, мм", "Высота, мм", "Толщина, мм", "Масса, кг", "Примечание"],
        new IReadOnlyList<object?>[]
        {
            new object?[] { "НС-1", null, null, "001", 3000, 2800, 300, 4500, "Наружная стеновая панель" },
            new object?[] { "НС-2", null, null, "002", 3000, 2800, 300, 4500, null },
        });
}

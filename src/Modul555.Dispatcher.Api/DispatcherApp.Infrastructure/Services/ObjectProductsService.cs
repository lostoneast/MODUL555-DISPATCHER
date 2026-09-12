using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class ObjectProductsService(DispatcherDbContext db, CatalogService catalogs)
{
    // An active assignment overrides the original project position. Historical assignments do not.
    public IQueryable<ObjectProductDto> Query(int objectId, ObjectProductsQuery query)
    {
        var rows = from p in db.Products.AsNoTracking()
                   let a = p.Assignments.Where(x => x.ValidTo == null).OrderByDescending(x => x.ValidFrom).ThenByDescending(x => x.Id).FirstOrDefault()
                   where a != null ? a.ConstructionObjectId == objectId : p.ProjectPosition != null && p.ProjectPosition.ConstructionObjectId == objectId
                   select new ObjectProductDto
                   {
                       Id = p.Id, ProductCode = p.ProductCode, ProductTypeId = p.ProductTypeId,
                       ProductTypeName = p.ProductType.Name, Mark = p.Mark,
                       WidthMm = p.WidthMm, HeightMm = p.HeightMm, ThicknessMm = p.ThicknessMm,
                       CorniceWidthIncreaseMm = p.CorniceWidthIncreaseMm, TotalWidthWithCorniceMm = p.TotalWidthWithCorniceMm,
                       ThicknessIncreaseMm = p.ThicknessIncreaseMm, RightBendMm = p.RightBendMm,
                       LeftBendMm = p.LeftBendMm, CladdingWidthWithBendsMm = p.CladdingWidthWithBendsMm,
                       WeightKg = p.WeightKg, Status = p.Status, AdditionalInfo = p.AdditionalInfo, Version = p.Version,
                       BuildingSectionId = a != null ? a.BuildingSectionId : p.ProjectPosition!.BuildingSectionId,
                       BuildingSectionName = a != null ? (a.BuildingSection == null ? null : a.BuildingSection.Name) : (p.ProjectPosition!.BuildingSection == null ? null : p.ProjectPosition.BuildingSection.Name),
                       FloorId = a != null ? a.FloorId : p.ProjectPosition!.FloorId,
                       FloorName = a != null ? (a.Floor == null ? null : a.Floor.Name) : (p.ProjectPosition!.Floor == null ? null : p.ProjectPosition.Floor.Name),
                       InstallationNumber = a != null ? a.InstallationNumber : p.ProjectPosition!.InstallationNumber,
                       BindingSource = a != null ? "assignment" : "project",
                       CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt,
                   };
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(x => x.ProductCode.Contains(term) || x.Mark.Contains(term)
                || (x.InstallationNumber != null && x.InstallationNumber.Contains(term))
                || (x.AdditionalInfo != null && x.AdditionalInfo.Contains(term)));
        }
        if (query.ProductTypeId.HasValue) rows = rows.Where(x => x.ProductTypeId == query.ProductTypeId);
        if (query.Status.HasValue) rows = rows.Where(x => x.Status == query.Status);
        if (query.BuildingSectionId.HasValue) rows = rows.Where(x => x.BuildingSectionId == query.BuildingSectionId);
        if (query.FloorId.HasValue) rows = rows.Where(x => x.FloorId == query.FloorId);
        var sorted = query.SortBy switch
        {
            "mark" => query.Descending ? rows.OrderByDescending(x => x.Mark) : rows.OrderBy(x => x.Mark),
            "productTypeName" => query.Descending ? rows.OrderByDescending(x => x.ProductTypeName) : rows.OrderBy(x => x.ProductTypeName),
            "status" => query.Descending ? rows.OrderByDescending(x => x.Status) : rows.OrderBy(x => x.Status),
            "weightKg" => query.Descending ? rows.OrderByDescending(x => x.WeightKg) : rows.OrderBy(x => x.WeightKg),
            "updatedAt" => query.Descending ? rows.OrderByDescending(x => x.UpdatedAt) : rows.OrderBy(x => x.UpdatedAt),
            "widthMm" => query.Descending ? rows.OrderByDescending(x => x.WidthMm) : rows.OrderBy(x => x.WidthMm),
            "heightMm" => query.Descending ? rows.OrderByDescending(x => x.HeightMm) : rows.OrderBy(x => x.HeightMm),
            "thicknessMm" => query.Descending ? rows.OrderByDescending(x => x.ThicknessMm) : rows.OrderBy(x => x.ThicknessMm),
            "installationNumber" => query.Descending ? rows.OrderByDescending(x => x.InstallationNumber) : rows.OrderBy(x => x.InstallationNumber),
            "productCode" => query.Descending ? rows.OrderByDescending(x => x.ProductCode) : rows.OrderBy(x => x.ProductCode),
            _ => throw new ArgumentException("Неизвестный столбец сортировки."),
        };
        return sorted.ThenBy(x => x.Id);
    }

    private async Task RequireObject(int id, bool editing, CancellationToken ct)
    {
        var obj = await db.ConstructionObjects.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException();
        if (editing && !obj.IsActive) throw new ConflictException("Объект неактивен. Редактирование изделий недоступно.");
    }

    public async Task<PagedResult<ObjectProductDto>> ListAsync(int id, ObjectProductsQuery query, CancellationToken ct)
    {
        await RequireObject(id, false, ct);
        var rows = Query(id, query);
        return new() { Total = await rows.CountAsync(ct), Page = query.Page, PageSize = query.PageSize,
            Items = await rows.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct) };
    }

    public async Task<ObjectStructureDto> StructureAsync(int id, CancellationToken ct)
    {
        await RequireObject(id, false, ct);
        return new(
            await db.BuildingSections.Where(x => x.ConstructionObjectId == id).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => new SectionOption(x.Id, x.Name)).ToListAsync(ct),
            await db.Floors.Where(x => x.BuildingSection.ConstructionObjectId == id).OrderBy(x => x.SortOrder).ThenBy(x => x.Id).Select(x => new FloorOption(x.Id, x.BuildingSectionId, x.Name)).ToListAsync(ct));
    }

    public static void Validate(ObjectProductWriteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ProductCode) || dto.ProductCode.Length > 64
            || string.IsNullOrWhiteSpace(dto.Mark) || dto.Mark.Length > 128 || dto.AdditionalInfo?.Length > 2000
            || dto.InstallationNumber?.Length > 64)
            throw new ArgumentException("Проверьте код (до 64 символов), марку (до 128), монтажный номер (до 64) и примечание (до 2000).");
        if (dto.WidthMm <= 0 || dto.HeightMm <= 0 || dto.ThicknessMm <= 0 || dto.WeightKg < 0
            || dto.CorniceWidthIncreaseMm < 0 || dto.TotalWidthWithCorniceMm < 0 || dto.ThicknessIncreaseMm < 0
            || dto.RightBendMm < 0 || dto.LeftBendMm < 0 || dto.CladdingWidthWithBendsMm < 0)
            throw new ArgumentException("Основные размеры должны быть положительными, остальные размеры и масса — неотрицательными.");
        if (!Enum.IsDefined(dto.Status)) throw new ArgumentException("Неизвестный статус изделия.");
    }

    private async Task ValidateLinks(int id, ObjectProductWriteDto dto, CancellationToken ct)
    {
        if (!await db.ProductTypes.AnyAsync(x => x.Id == dto.ProductTypeId, ct))
            throw new ArgumentException("Тип изделия не найден.");
        if (dto.BuildingSectionId.HasValue && !await db.BuildingSections.AnyAsync(x => x.Id == dto.BuildingSectionId && x.ConstructionObjectId == id, ct))
            throw new ArgumentException("Секция не принадлежит этому объекту.");
        if (dto.FloorId.HasValue && (!dto.BuildingSectionId.HasValue || !await db.Floors.AnyAsync(x => x.Id == dto.FloorId && x.BuildingSectionId == dto.BuildingSectionId, ct)))
            throw new ArgumentException("Этаж не принадлежит выбранной секции.");
    }

    public async Task<ObjectProductDto> UpdateAsync(int id, long productId, ObjectProductWriteDto dto, CancellationToken ct)
    {
        Validate(dto);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await RequireObject(id, true, ct);
        var current = await Query(id, new()).SingleOrDefaultAsync(x => x.Id == productId, ct)
            ?? throw new KeyNotFoundException();
        if (dto.Version != current.Version) throw new ConflictException("Изделие изменено другим пользователем. Обновите таблицу; введённые значения пока сохранены в форме.");
        if (dto.ProductCode != current.ProductCode) throw new ArgumentException("Код изделия неизменяемый.");
        // Status changes belong to production/logistics workflows; do not fabricate physical state here.
        if (dto.Status != current.Status) throw new ArgumentException("Статус изделия изменяется операциями производства и логистики.");
        await ValidateLinks(id, dto, ct);
        var product = await db.Products.SingleAsync(x => x.Id == productId, ct);
        db.Entry(product).Property(x => x.Version).OriginalValue = dto.Version;
        if (dto.BuildingSectionId != current.BuildingSectionId || dto.FloorId != current.FloorId || dto.InstallationNumber != current.InstallationNumber)
        {
            var now = DateTimeOffset.UtcNow;
            var previous = await db.ProductAssignments.Where(x => x.ProductId == productId && x.ValidTo == null).ToListAsync(ct);
            foreach (var assignment in previous) assignment.ValidTo = now;
            db.ProductAssignments.Add(new ProductAssignment
            {
                ProductId = productId, ConstructionObjectId = id, BuildingSectionId = dto.BuildingSectionId,
                FloorId = dto.FloorId, InstallationNumber = dto.InstallationNumber,
                Reason = AssignmentReason.Correction, ValidFrom = now, Comment = "Изменение расположения в таблице объекта",
            });
        }
        await catalogs.UpdateProductAsync(productId, dto, ct);
        var result = await Query(id, new()).SingleAsync(x => x.Id == productId, ct);
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task<ObjectProductDto> CreateAsync(int id, ObjectProductWriteDto dto, CancellationToken ct)
    {
        Validate(dto);
        if (dto.Status != ProductStatus.Created) throw new ArgumentException("Новое изделие должно иметь статус «Создано».");
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await RequireObject(id, true, ct);
        await ValidateLinks(id, dto, ct);
        var created = await catalogs.CreateProductAsync(dto, ct);
        db.ProjectPositions.Add(new ProjectPosition
        {
            ProductId = created.Id, ConstructionObjectId = id, BuildingSectionId = dto.BuildingSectionId,
            FloorId = dto.FloorId, InstallationNumber = dto.InstallationNumber,
        });
        await db.SaveChangesAsync(ct);
        var result = await Query(id, new()).SingleAsync(x => x.Id == created.Id, ct);
        await tx.CommitAsync(ct);
        return result;
    }

    public async Task<byte[]> ExportAsync(int id, ObjectProductsQuery query, CancellationToken ct)
    {
        await RequireObject(id, false, ct);
        var rows = Query(id, query);
        if (await rows.CountAsync(ct) > 50000) throw new ArgumentException("Экспорт ограничен 50 000 изделий. Уточните фильтры.");
        var properties = typeof(ObjectProductDto).GetProperties();
        var data = await rows.ToListAsync(ct);
        return ExcelHelper.Export(properties.Select(x => x.Name).ToArray(),
            data.Select(x => (IReadOnlyList<object?>)properties.Select(p => p.GetValue(x)).ToArray()));
    }
}

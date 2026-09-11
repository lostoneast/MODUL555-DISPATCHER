using System.Linq.Expressions;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class CatalogService
{
    private readonly DispatcherDbContext _db;

    public CatalogService(DispatcherDbContext db) => _db = db;

    // ── helpers ──────────────────────────────────────────────────────────────

    private static (int Page, int PageSize) Normalize(PagedQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : Math.Min(query.PageSize, 200);
        return (page, pageSize);
    }

    private static async Task<PagedResult<T>> PageAsync<T>(
        IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken ct
    )
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize,
        };
    }

    private static IQueryable<T> ApplySearch<T>(
        IQueryable<T> query,
        string? search,
        Func<string, Expression<Func<T, bool>>> predicateFactory
    )
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;
        return query.Where(predicateFactory(search.Trim()));
    }

    private static IQueryable<T> ApplyActive<T>(
        IQueryable<T> query,
        bool? activeOnly,
        Expression<Func<T, bool>> isActivePredicate
    )
    {
        if (activeOnly != false)
            return query.Where(isActivePredicate);
        return query;
    }

    private static void ValidateStorageOwner(int? plantId, int? constructionObjectId)
    {
        var hasPlant = plantId.HasValue;
        var hasObject = constructionObjectId.HasValue;
        if (hasPlant == hasObject)
            throw new ArgumentException(
                "Storage area must have exactly one of PlantId or ConstructionObjectId set."
            );
    }

    private static Dictionary<string, object?> ToDict(object dto)
    {
        var result = new Dictionary<string, object?>();
        foreach (var prop in dto.GetType().GetProperties())
            result[prop.Name] = prop.GetValue(dto);
        return result;
    }

    // ── product-types ────────────────────────────────────────────────────────

    public async Task<PagedResult<ProductTypeDto>> ListProductTypesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.ProductTypes.AsNoTracking().AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Code);
        var projected = q.Select(x => new ProductTypeDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<ProductTypeDto> CreateProductTypeAsync(
        ProductTypeWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new ProductType
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsActive = dto.IsActive,
        };
        _db.ProductTypes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapProductType(entity);
    }

    public async Task<ProductTypeDto> UpdateProductTypeAsync(
        int id,
        ProductTypeWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.ProductTypes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductType {id} not found.");
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return MapProductType(entity);
    }

    public async Task DeactivateProductTypeAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ProductTypes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductType {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateProductTypeAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ProductTypes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductType {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static ProductTypeDto MapProductType(ProductType x) =>
        new()
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        };

    // ── plants ───────────────────────────────────────────────────────────────

    public async Task<PagedResult<PlantDto>> ListPlantsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.Plants.AsNoTracking().AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Code);
        var projected = q.Select(x => new PlantDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Address = x.Address,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<PlantDto> CreatePlantAsync(PlantWriteDto dto, CancellationToken ct = default)
    {
        var entity = new Plant
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            IsActive = dto.IsActive,
        };
        _db.Plants.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapPlant(entity);
    }

    public async Task<PlantDto> UpdatePlantAsync(
        int id,
        PlantWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.Plants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plant {id} not found.");
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Address = dto.Address.Trim();
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return MapPlant(entity);
    }

    public async Task DeactivatePlantAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.Plants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plant {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivatePlantAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.Plants.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Plant {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static PlantDto MapPlant(Plant x) =>
        new()
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Address = x.Address,
            IsActive = x.IsActive,
        };

    // ── production-lines ─────────────────────────────────────────────────────

    public async Task<PagedResult<ProductionLineDto>> ListProductionLinesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.ProductionLines.AsNoTracking().Include(x => x.Plant).AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Code);
        var projected = q.Select(x => new ProductionLineDto
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant.Name,
            Code = x.Code,
            Name = x.Name,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<ProductionLineDto> CreateProductionLineAsync(
        ProductionLineWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new ProductionLine
        {
            PlantId = dto.PlantId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            IsActive = dto.IsActive,
        };
        _db.ProductionLines.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        return MapProductionLine(entity);
    }

    public async Task<ProductionLineDto> UpdateProductionLineAsync(
        int id,
        ProductionLineWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.ProductionLines.Include(x => x.Plant).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductionLine {id} not found.");
        entity.PlantId = dto.PlantId;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        return MapProductionLine(entity);
    }

    public async Task DeactivateProductionLineAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ProductionLines.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductionLine {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateProductionLineAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ProductionLines.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ProductionLine {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static ProductionLineDto MapProductionLine(ProductionLine x) =>
        new()
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant?.Name,
            Code = x.Code,
            Name = x.Name,
            IsActive = x.IsActive,
        };

    // ── line-capabilities ────────────────────────────────────────────────────

    public async Task<PagedResult<LineCapabilityDto>> ListLineCapabilitiesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .LineCapabilities.AsNoTracking()
            .Include(x => x.ProductionLine)
            .Include(x => x.ProductType)
            .AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s =>
                x =>
                    x.ProductionLine.Name.Contains(s)
                    || x.ProductionLine.Code.Contains(s)
                    || x.ProductType.Name.Contains(s)
                    || x.ProductType.Code.Contains(s)
        );
        q = q.OrderBy(x => x.Id);
        var projected = q.Select(x => new LineCapabilityDto
        {
            Id = x.Id,
            ProductionLineId = x.ProductionLineId,
            ProductionLineName = x.ProductionLine.Name,
            ProductTypeId = x.ProductTypeId,
            ProductTypeName = x.ProductType.Name,
            DefaultDailyCapacityUnits = x.DefaultDailyCapacityUnits,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<LineCapabilityDto> CreateLineCapabilityAsync(
        LineCapabilityWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new LineCapability
        {
            ProductionLineId = dto.ProductionLineId,
            ProductTypeId = dto.ProductTypeId,
            DefaultDailyCapacityUnits = dto.DefaultDailyCapacityUnits,
            IsActive = dto.IsActive,
        };
        _db.LineCapabilities.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductionLine).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductType).LoadAsync(ct);
        return MapLineCapability(entity);
    }

    public async Task<LineCapabilityDto> UpdateLineCapabilityAsync(
        int id,
        LineCapabilityWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .LineCapabilities.Include(x => x.ProductionLine)
                .Include(x => x.ProductType)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"LineCapability {id} not found.");
        entity.ProductionLineId = dto.ProductionLineId;
        entity.ProductTypeId = dto.ProductTypeId;
        entity.DefaultDailyCapacityUnits = dto.DefaultDailyCapacityUnits;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductionLine).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductType).LoadAsync(ct);
        return MapLineCapability(entity);
    }

    public async Task DeactivateLineCapabilityAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.LineCapabilities.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"LineCapability {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateLineCapabilityAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.LineCapabilities.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"LineCapability {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static LineCapabilityDto MapLineCapability(LineCapability x) =>
        new()
        {
            Id = x.Id,
            ProductionLineId = x.ProductionLineId,
            ProductionLineName = x.ProductionLine?.Name,
            ProductTypeId = x.ProductTypeId,
            ProductTypeName = x.ProductType?.Name,
            DefaultDailyCapacityUnits = x.DefaultDailyCapacityUnits,
            IsActive = x.IsActive,
        };

    // ── construction-objects ─────────────────────────────────────────────────

    public async Task<PagedResult<ConstructionObjectDto>> ListConstructionObjectsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.ConstructionObjects.AsNoTracking().AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Code);
        var projected = q.Select(x => new ConstructionObjectDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Address = x.Address,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<ConstructionObjectDto> CreateConstructionObjectAsync(
        ConstructionObjectWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new ConstructionObject
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Address = dto.Address.Trim(),
            IsActive = dto.IsActive,
        };
        _db.ConstructionObjects.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapConstructionObject(entity);
    }

    public async Task<ConstructionObjectDto> UpdateConstructionObjectAsync(
        int id,
        ConstructionObjectWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.ConstructionObjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ConstructionObject {id} not found.");
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Address = dto.Address.Trim();
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return MapConstructionObject(entity);
    }

    public async Task DeactivateConstructionObjectAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ConstructionObjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ConstructionObject {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateConstructionObjectAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.ConstructionObjects.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ConstructionObject {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static ConstructionObjectDto MapConstructionObject(ConstructionObject x) =>
        new()
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Address = x.Address,
            IsActive = x.IsActive,
        };

    // ── building-sections ────────────────────────────────────────────────────

    public async Task<PagedResult<BuildingSectionDto>> ListBuildingSectionsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .BuildingSections.AsNoTracking()
            .Include(x => x.ConstructionObject)
            .AsQueryable();
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.SortOrder).ThenBy(x => x.Code);
        var projected = q.Select(x => new BuildingSectionDto
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject.Name,
            Code = x.Code,
            Name = x.Name,
            SortOrder = x.SortOrder,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<BuildingSectionDto> CreateBuildingSectionAsync(
        BuildingSectionWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new BuildingSection
        {
            ConstructionObjectId = dto.ConstructionObjectId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            SortOrder = dto.SortOrder,
        };
        _db.BuildingSections.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapBuildingSection(entity);
    }

    public async Task<BuildingSectionDto> UpdateBuildingSectionAsync(
        int id,
        BuildingSectionWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .BuildingSections.Include(x => x.ConstructionObject)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"BuildingSection {id} not found.");
        entity.ConstructionObjectId = dto.ConstructionObjectId;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapBuildingSection(entity);
    }

    private static BuildingSectionDto MapBuildingSection(BuildingSection x) =>
        new()
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject?.Name,
            Code = x.Code,
            Name = x.Name,
            SortOrder = x.SortOrder,
        };

    // ── floors ───────────────────────────────────────────────────────────────

    public async Task<PagedResult<FloorDto>> ListFloorsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.Floors.AsNoTracking().Include(x => x.BuildingSection).AsQueryable();
        q = ApplySearch(q, query.Search, s => x => x.Name.Contains(s));
        q = q.OrderBy(x => x.SortOrder).ThenBy(x => x.Name);
        var projected = q.Select(x => new FloorDto
        {
            Id = x.Id,
            BuildingSectionId = x.BuildingSectionId,
            BuildingSectionName = x.BuildingSection.Name,
            Number = x.Number,
            Name = x.Name,
            SortOrder = x.SortOrder,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<FloorDto> CreateFloorAsync(FloorWriteDto dto, CancellationToken ct = default)
    {
        var entity = new Floor
        {
            BuildingSectionId = dto.BuildingSectionId,
            Number = dto.Number,
            Name = dto.Name.Trim(),
            SortOrder = dto.SortOrder,
        };
        _db.Floors.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.BuildingSection).LoadAsync(ct);
        return MapFloor(entity);
    }

    public async Task<FloorDto> UpdateFloorAsync(
        int id,
        FloorWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.Floors.Include(x => x.BuildingSection).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Floor {id} not found.");
        entity.BuildingSectionId = dto.BuildingSectionId;
        entity.Number = dto.Number;
        entity.Name = dto.Name.Trim();
        entity.SortOrder = dto.SortOrder;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.BuildingSection).LoadAsync(ct);
        return MapFloor(entity);
    }

    private static FloorDto MapFloor(Floor x) =>
        new()
        {
            Id = x.Id,
            BuildingSectionId = x.BuildingSectionId,
            BuildingSectionName = x.BuildingSection?.Name,
            Number = x.Number,
            Name = x.Name,
            SortOrder = x.SortOrder,
        };

    // ── unloading-points ─────────────────────────────────────────────────────

    public async Task<PagedResult<UnloadingPointDto>> ListUnloadingPointsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .UnloadingPoints.AsNoTracking()
            .Include(x => x.ConstructionObject)
            .AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(q, query.Search, s => x => x.Name.Contains(s));
        q = q.OrderBy(x => x.Name);
        var projected = q.Select(x => new UnloadingPointDto
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject.Name,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<UnloadingPointDto> CreateUnloadingPointAsync(
        UnloadingPointWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new UnloadingPoint
        {
            ConstructionObjectId = dto.ConstructionObjectId,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            IsActive = dto.IsActive,
        };
        _db.UnloadingPoints.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapUnloadingPoint(entity);
    }

    public async Task<UnloadingPointDto> UpdateUnloadingPointAsync(
        int id,
        UnloadingPointWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .UnloadingPoints.Include(x => x.ConstructionObject)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"UnloadingPoint {id} not found.");
        entity.ConstructionObjectId = dto.ConstructionObjectId;
        entity.Name = dto.Name.Trim();
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapUnloadingPoint(entity);
    }

    public async Task DeactivateUnloadingPointAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.UnloadingPoints.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"UnloadingPoint {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateUnloadingPointAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.UnloadingPoints.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"UnloadingPoint {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static UnloadingPointDto MapUnloadingPoint(UnloadingPoint x) =>
        new()
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject?.Name,
            Name = x.Name,
            Description = x.Description,
            IsActive = x.IsActive,
        };

    // ── storage-areas ────────────────────────────────────────────────────────

    public async Task<PagedResult<StorageAreaDto>> ListStorageAreasAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .StorageAreas.AsNoTracking()
            .Include(x => x.Plant)
            .Include(x => x.ConstructionObject)
            .AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(q, query.Search, s => x => x.Name.Contains(s));
        q = q.OrderBy(x => x.Name);
        var projected = q.Select(x => new StorageAreaDto
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant != null ? x.Plant.Name : null,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject != null ? x.ConstructionObject.Name : null,
            Name = x.Name,
            CapacityUnits = x.CapacityUnits,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<StorageAreaDto> CreateStorageAreaAsync(
        StorageAreaWriteDto dto,
        CancellationToken ct = default
    )
    {
        ValidateStorageOwner(dto.PlantId, dto.ConstructionObjectId);
        var entity = new StorageArea
        {
            PlantId = dto.PlantId,
            ConstructionObjectId = dto.ConstructionObjectId,
            Name = dto.Name.Trim(),
            CapacityUnits = dto.CapacityUnits,
            IsActive = dto.IsActive,
        };
        _db.StorageAreas.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapStorageArea(entity);
    }

    public async Task<StorageAreaDto> UpdateStorageAreaAsync(
        int id,
        StorageAreaWriteDto dto,
        CancellationToken ct = default
    )
    {
        ValidateStorageOwner(dto.PlantId, dto.ConstructionObjectId);
        var entity =
            await _db
                .StorageAreas.Include(x => x.Plant)
                .Include(x => x.ConstructionObject)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"StorageArea {id} not found.");
        entity.PlantId = dto.PlantId;
        entity.ConstructionObjectId = dto.ConstructionObjectId;
        entity.Name = dto.Name.Trim();
        entity.CapacityUnits = dto.CapacityUnits;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapStorageArea(entity);
    }

    public async Task DeactivateStorageAreaAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.StorageAreas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"StorageArea {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateStorageAreaAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.StorageAreas.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"StorageArea {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static StorageAreaDto MapStorageArea(StorageArea x) =>
        new()
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant?.Name,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject?.Name,
            Name = x.Name,
            CapacityUnits = x.CapacityUnits,
            IsActive = x.IsActive,
        };

    // ── carriers ─────────────────────────────────────────────────────────────

    public async Task<PagedResult<CarrierDto>> ListCarriersAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.Carriers.AsNoTracking().AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(q, query.Search, s => x => x.Name.Contains(s));
        q = q.OrderBy(x => x.Name);
        var projected = q.Select(x => new CarrierDto
        {
            Id = x.Id,
            Name = x.Name,
            ContactInfo = x.ContactInfo,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<CarrierDto> CreateCarrierAsync(
        CarrierWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new Carrier
        {
            Name = dto.Name.Trim(),
            ContactInfo = dto.ContactInfo,
            IsActive = dto.IsActive,
        };
        _db.Carriers.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapCarrier(entity);
    }

    public async Task<CarrierDto> UpdateCarrierAsync(
        int id,
        CarrierWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.Carriers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Carrier {id} not found.");
        entity.Name = dto.Name.Trim();
        entity.ContactInfo = dto.ContactInfo;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        return MapCarrier(entity);
    }

    public async Task DeactivateCarrierAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.Carriers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Carrier {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateCarrierAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.Carriers.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Carrier {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static CarrierDto MapCarrier(Carrier x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            ContactInfo = x.ContactInfo,
            IsActive = x.IsActive,
        };

    // ── vehicle-types ────────────────────────────────────────────────────────

    public async Task<PagedResult<VehicleTypeDto>> ListVehicleTypesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.VehicleTypes.AsNoTracking().AsQueryable();
        q = ApplySearch(q, query.Search, s => x => x.Name.Contains(s));
        q = q.OrderBy(x => x.Name);
        var projected = q.Select(x => new VehicleTypeDto
        {
            Id = x.Id,
            Name = x.Name,
            RollingStockType = x.RollingStockType,
            MinPayloadKg = x.MinPayloadKg,
            MaxPayloadKg = x.MaxPayloadKg,
            LoadingPlatformCount = x.LoadingPlatformCount,
            LoadingPlatformLengthMm = x.LoadingPlatformLengthMm,
            LoadingPlatformWidthMm = x.LoadingPlatformWidthMm,
            AllowedRightLeftImbalanceKg = x.AllowedRightLeftImbalanceKg,
            MaxCargoHeightMm = x.MaxCargoHeightMm,
            CargoVolumeM3 = x.CargoVolumeM3,
            TotalTrainLengthMm = x.TotalTrainLengthMm,
            TurningRadiusMm = x.TurningRadiusMm,
            Notes = x.Notes,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<VehicleTypeDto> CreateVehicleTypeAsync(
        VehicleTypeWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = MapVehicleTypeWrite(dto);
        _db.VehicleTypes.Add(entity);
        await _db.SaveChangesAsync(ct);
        return MapVehicleType(entity);
    }

    public async Task<VehicleTypeDto> UpdateVehicleTypeAsync(
        int id,
        VehicleTypeWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.VehicleTypes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"VehicleType {id} not found.");
        ApplyVehicleTypeWrite(entity, dto);
        await _db.SaveChangesAsync(ct);
        return MapVehicleType(entity);
    }

    private static VehicleType MapVehicleTypeWrite(VehicleTypeWriteDto dto)
    {
        var entity = new VehicleType();
        ApplyVehicleTypeWrite(entity, dto);
        return entity;
    }

    private static void ApplyVehicleTypeWrite(VehicleType entity, VehicleTypeWriteDto dto)
    {
        entity.Name = dto.Name.Trim();
        entity.RollingStockType = dto.RollingStockType;
        entity.MinPayloadKg = dto.MinPayloadKg;
        entity.MaxPayloadKg = dto.MaxPayloadKg;
        entity.LoadingPlatformCount = dto.LoadingPlatformCount;
        entity.LoadingPlatformLengthMm = dto.LoadingPlatformLengthMm;
        entity.LoadingPlatformWidthMm = dto.LoadingPlatformWidthMm;
        entity.AllowedRightLeftImbalanceKg = dto.AllowedRightLeftImbalanceKg;
        entity.MaxCargoHeightMm = dto.MaxCargoHeightMm;
        entity.CargoVolumeM3 = dto.CargoVolumeM3;
        entity.TotalTrainLengthMm = dto.TotalTrainLengthMm;
        entity.TurningRadiusMm = dto.TurningRadiusMm;
        entity.Notes = dto.Notes;
    }

    private static VehicleTypeDto MapVehicleType(VehicleType x) =>
        new()
        {
            Id = x.Id,
            Name = x.Name,
            RollingStockType = x.RollingStockType,
            MinPayloadKg = x.MinPayloadKg,
            MaxPayloadKg = x.MaxPayloadKg,
            LoadingPlatformCount = x.LoadingPlatformCount,
            LoadingPlatformLengthMm = x.LoadingPlatformLengthMm,
            LoadingPlatformWidthMm = x.LoadingPlatformWidthMm,
            AllowedRightLeftImbalanceKg = x.AllowedRightLeftImbalanceKg,
            MaxCargoHeightMm = x.MaxCargoHeightMm,
            CargoVolumeM3 = x.CargoVolumeM3,
            TotalTrainLengthMm = x.TotalTrainLengthMm,
            TurningRadiusMm = x.TurningRadiusMm,
            Notes = x.Notes,
        };

    // ── vehicles ─────────────────────────────────────────────────────────────

    public async Task<PagedResult<VehicleDto>> ListVehiclesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .Vehicles.AsNoTracking()
            .Include(x => x.VehicleType)
            .Include(x => x.Carrier)
            .AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s =>
                x =>
                    x.RegistrationNumber.Contains(s)
                    || x.Make.Contains(s)
                    || x.Model.Contains(s)
        );
        q = q.OrderBy(x => x.RegistrationNumber);
        var projected = q.Select(x => new VehicleDto
        {
            Id = x.Id,
            VehicleTypeId = x.VehicleTypeId,
            VehicleTypeName = x.VehicleType.Name,
            CarrierId = x.CarrierId,
            CarrierName = x.Carrier.Name,
            Make = x.Make,
            Model = x.Model,
            RegistrationNumber = x.RegistrationNumber,
            IsActive = x.IsActive,
            Notes = x.Notes,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<VehicleDto> CreateVehicleAsync(
        VehicleWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new Vehicle
        {
            VehicleTypeId = dto.VehicleTypeId,
            CarrierId = dto.CarrierId,
            Make = dto.Make.Trim(),
            Model = dto.Model.Trim(),
            RegistrationNumber = dto.RegistrationNumber.Trim(),
            IsActive = dto.IsActive,
            Notes = dto.Notes,
        };
        _db.Vehicles.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.VehicleType).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.Carrier).LoadAsync(ct);
        return MapVehicle(entity);
    }

    public async Task<VehicleDto> UpdateVehicleAsync(
        long id,
        VehicleWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .Vehicles.Include(x => x.VehicleType)
                .Include(x => x.Carrier)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");
        entity.VehicleTypeId = dto.VehicleTypeId;
        entity.CarrierId = dto.CarrierId;
        entity.Make = dto.Make.Trim();
        entity.Model = dto.Model.Trim();
        entity.RegistrationNumber = dto.RegistrationNumber.Trim();
        entity.IsActive = dto.IsActive;
        entity.Notes = dto.Notes;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.VehicleType).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.Carrier).LoadAsync(ct);
        return MapVehicle(entity);
    }

    public async Task DeactivateVehicleAsync(long id, CancellationToken ct = default)
    {
        var entity =
            await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateVehicleAsync(long id, CancellationToken ct = default)
    {
        var entity =
            await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Vehicle {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static VehicleDto MapVehicle(Vehicle x) =>
        new()
        {
            Id = x.Id,
            VehicleTypeId = x.VehicleTypeId,
            VehicleTypeName = x.VehicleType?.Name,
            CarrierId = x.CarrierId,
            CarrierName = x.Carrier?.Name,
            Make = x.Make,
            Model = x.Model,
            RegistrationNumber = x.RegistrationNumber,
            IsActive = x.IsActive,
            Notes = x.Notes,
        };

    // ── transport-routes ─────────────────────────────────────────────────────

    public async Task<PagedResult<TransportRouteDto>> ListTransportRoutesAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .TransportRoutes.AsNoTracking()
            .Include(x => x.Plant)
            .Include(x => x.ConstructionObject)
            .AsQueryable();
        q = ApplyActive(q, query.ActiveOnly, x => x.IsActive);
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Plant.Name.Contains(s) || x.ConstructionObject.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Id);
        var projected = q.Select(x => new TransportRouteDto
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant.Name,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject.Name,
            DistanceKm = x.DistanceKm,
            EstimatedTravelMinutes = x.EstimatedTravelMinutes,
            TurnoverCoefficientPerDay = x.TurnoverCoefficientPerDay,
            IsActive = x.IsActive,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<TransportRouteDto> CreateTransportRouteAsync(
        TransportRouteWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity = new TransportRoute
        {
            PlantId = dto.PlantId,
            ConstructionObjectId = dto.ConstructionObjectId,
            DistanceKm = dto.DistanceKm,
            EstimatedTravelMinutes = dto.EstimatedTravelMinutes,
            TurnoverCoefficientPerDay = dto.TurnoverCoefficientPerDay,
            IsActive = dto.IsActive,
        };
        _db.TransportRoutes.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapTransportRoute(entity);
    }

    public async Task<TransportRouteDto> UpdateTransportRouteAsync(
        int id,
        TransportRouteWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .TransportRoutes.Include(x => x.Plant)
                .Include(x => x.ConstructionObject)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"TransportRoute {id} not found.");
        entity.PlantId = dto.PlantId;
        entity.ConstructionObjectId = dto.ConstructionObjectId;
        entity.DistanceKm = dto.DistanceKm;
        entity.EstimatedTravelMinutes = dto.EstimatedTravelMinutes;
        entity.TurnoverCoefficientPerDay = dto.TurnoverCoefficientPerDay;
        entity.IsActive = dto.IsActive;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.Plant).LoadAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapTransportRoute(entity);
    }

    public async Task DeactivateTransportRouteAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.TransportRoutes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"TransportRoute {id} not found.");
        entity.IsActive = false;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ActivateTransportRouteAsync(int id, CancellationToken ct = default)
    {
        var entity =
            await _db.TransportRoutes.FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"TransportRoute {id} not found.");
        entity.IsActive = true;
        await _db.SaveChangesAsync(ct);
    }

    private static TransportRouteDto MapTransportRoute(TransportRoute x) =>
        new()
        {
            Id = x.Id,
            PlantId = x.PlantId,
            PlantName = x.Plant?.Name,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject?.Name,
            DistanceKm = x.DistanceKm,
            EstimatedTravelMinutes = x.EstimatedTravelMinutes,
            TurnoverCoefficientPerDay = x.TurnoverCoefficientPerDay,
            IsActive = x.IsActive,
        };

    // ── construction-takts ───────────────────────────────────────────────────

    public async Task<PagedResult<ConstructionTaktDto>> ListConstructionTaktsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db
            .ConstructionTakts.AsNoTracking()
            .Include(x => x.ConstructionObject)
            .AsQueryable();
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.Code.Contains(s) || x.Name.Contains(s)
        );
        q = q.OrderBy(x => x.Sequence).ThenBy(x => x.Code);
        var projected = q.Select(x => new ConstructionTaktDto
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject.Name,
            Code = x.Code,
            Name = x.Name,
            Sequence = x.Sequence,
            PlannedProductionStartDate = x.PlannedProductionStartDate,
            PlannedProductionEndDate = x.PlannedProductionEndDate,
            Status = x.Status,
            Comment = x.Comment,
            Version = x.Version,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<ConstructionTaktDto> CreateConstructionTaktAsync(
        ConstructionTaktWriteDto dto,
        CancellationToken ct = default
    )
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new ConstructionTakt
        {
            ConstructionObjectId = dto.ConstructionObjectId,
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Sequence = dto.Sequence,
            PlannedProductionStartDate = dto.PlannedProductionStartDate,
            PlannedProductionEndDate = dto.PlannedProductionEndDate,
            Status = dto.Status,
            Comment = dto.Comment,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };
        _db.ConstructionTakts.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapConstructionTakt(entity);
    }

    public async Task<ConstructionTaktDto> UpdateConstructionTaktAsync(
        long id,
        ConstructionTaktWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db
                .ConstructionTakts.Include(x => x.ConstructionObject)
                .FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"ConstructionTakt {id} not found.");
        entity.ConstructionObjectId = dto.ConstructionObjectId;
        entity.Code = dto.Code.Trim();
        entity.Name = dto.Name.Trim();
        entity.Sequence = dto.Sequence;
        entity.PlannedProductionStartDate = dto.PlannedProductionStartDate;
        entity.PlannedProductionEndDate = dto.PlannedProductionEndDate;
        entity.Status = dto.Status;
        entity.Comment = dto.Comment;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Version++;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ConstructionObject).LoadAsync(ct);
        return MapConstructionTakt(entity);
    }

    private static ConstructionTaktDto MapConstructionTakt(ConstructionTakt x) =>
        new()
        {
            Id = x.Id,
            ConstructionObjectId = x.ConstructionObjectId,
            ConstructionObjectName = x.ConstructionObject?.Name,
            Code = x.Code,
            Name = x.Name,
            Sequence = x.Sequence,
            PlannedProductionStartDate = x.PlannedProductionStartDate,
            PlannedProductionEndDate = x.PlannedProductionEndDate,
            Status = x.Status,
            Comment = x.Comment,
            Version = x.Version,
        };

    // ── products ─────────────────────────────────────────────────────────────

    public async Task<PagedResult<ProductDto>> ListProductsAsync(
        PagedQuery query,
        CancellationToken ct = default
    )
    {
        var (page, pageSize) = Normalize(query);
        var q = _db.Products.AsNoTracking().Include(x => x.ProductType).AsQueryable();
        q = ApplySearch(
            q,
            query.Search,
            s => x => x.ProductCode.Contains(s) || x.Mark.Contains(s)
        );
        q = q.OrderBy(x => x.ProductCode);
        var projected = q.Select(x => new ProductDto
        {
            Id = x.Id,
            ProductCode = x.ProductCode,
            ProductTypeId = x.ProductTypeId,
            ProductTypeName = x.ProductType.Name,
            Mark = x.Mark,
            WidthMm = x.WidthMm,
            HeightMm = x.HeightMm,
            ThicknessMm = x.ThicknessMm,
            CorniceWidthIncreaseMm = x.CorniceWidthIncreaseMm,
            TotalWidthWithCorniceMm = x.TotalWidthWithCorniceMm,
            ThicknessIncreaseMm = x.ThicknessIncreaseMm,
            RightBendMm = x.RightBendMm,
            LeftBendMm = x.LeftBendMm,
            CladdingWidthWithBendsMm = x.CladdingWidthWithBendsMm,
            WeightKg = x.WeightKg,
            Status = x.Status,
            AdditionalInfo = x.AdditionalInfo,
            Version = x.Version,
        });
        return await PageAsync(projected, page, pageSize, ct);
    }

    public async Task<ProductDto> CreateProductAsync(
        ProductWriteDto dto,
        CancellationToken ct = default
    )
    {
        var now = DateTimeOffset.UtcNow;
        var entity = new Product
        {
            ProductCode = dto.ProductCode.Trim(),
            ProductTypeId = dto.ProductTypeId,
            Mark = dto.Mark.Trim(),
            WidthMm = dto.WidthMm,
            HeightMm = dto.HeightMm,
            ThicknessMm = dto.ThicknessMm,
            CorniceWidthIncreaseMm = dto.CorniceWidthIncreaseMm,
            TotalWidthWithCorniceMm = dto.TotalWidthWithCorniceMm,
            ThicknessIncreaseMm = dto.ThicknessIncreaseMm,
            RightBendMm = dto.RightBendMm,
            LeftBendMm = dto.LeftBendMm,
            CladdingWidthWithBendsMm = dto.CladdingWidthWithBendsMm,
            WeightKg = dto.WeightKg,
            Status = dto.Status,
            AdditionalInfo = dto.AdditionalInfo,
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };
        _db.Products.Add(entity);
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductType).LoadAsync(ct);
        return MapProduct(entity);
    }

    public async Task<ProductDto> UpdateProductAsync(
        long id,
        ProductWriteDto dto,
        CancellationToken ct = default
    )
    {
        var entity =
            await _db.Products.Include(x => x.ProductType).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new KeyNotFoundException($"Product {id} not found.");
        entity.ProductCode = dto.ProductCode.Trim();
        entity.ProductTypeId = dto.ProductTypeId;
        entity.Mark = dto.Mark.Trim();
        entity.WidthMm = dto.WidthMm;
        entity.HeightMm = dto.HeightMm;
        entity.ThicknessMm = dto.ThicknessMm;
        entity.CorniceWidthIncreaseMm = dto.CorniceWidthIncreaseMm;
        entity.TotalWidthWithCorniceMm = dto.TotalWidthWithCorniceMm;
        entity.ThicknessIncreaseMm = dto.ThicknessIncreaseMm;
        entity.RightBendMm = dto.RightBendMm;
        entity.LeftBendMm = dto.LeftBendMm;
        entity.CladdingWidthWithBendsMm = dto.CladdingWidthWithBendsMm;
        entity.WeightKg = dto.WeightKg;
        entity.Status = dto.Status;
        entity.AdditionalInfo = dto.AdditionalInfo;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
        entity.Version++;
        await _db.SaveChangesAsync(ct);
        await _db.Entry(entity).Reference(x => x.ProductType).LoadAsync(ct);
        return MapProduct(entity);
    }

    private static ProductDto MapProduct(Product x) =>
        new()
        {
            Id = x.Id,
            ProductCode = x.ProductCode,
            ProductTypeId = x.ProductTypeId,
            ProductTypeName = x.ProductType?.Name,
            Mark = x.Mark,
            WidthMm = x.WidthMm,
            HeightMm = x.HeightMm,
            ThicknessMm = x.ThicknessMm,
            CorniceWidthIncreaseMm = x.CorniceWidthIncreaseMm,
            TotalWidthWithCorniceMm = x.TotalWidthWithCorniceMm,
            ThicknessIncreaseMm = x.ThicknessIncreaseMm,
            RightBendMm = x.RightBendMm,
            LeftBendMm = x.LeftBendMm,
            CladdingWidthWithBendsMm = x.CladdingWidthWithBendsMm,
            WeightKg = x.WeightKg,
            Status = x.Status,
            AdditionalInfo = x.AdditionalInfo,
            Version = x.Version,
        };

    // ── lookups / export / import / dispatchers ───────────────────────────────

    public async Task<List<LookupItem>> LookupsAsync(
        string entity,
        CancellationToken ct = default
    )
    {
        return entity.ToLowerInvariant() switch
        {
            "product-types" => await _db
                .ProductTypes.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "plants" => await _db
                .Plants.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "production-lines" => await _db
                .ProductionLines.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "line-capabilities" => await _db
                .LineCapabilities.AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.ProductionLine)
                .Include(x => x.ProductType)
                .OrderBy(x => x.Id)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.ProductionLine.Name + " / " + x.ProductType.Name,
                })
                .ToListAsync(ct),
            "construction-objects" => await _db
                .ConstructionObjects.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "building-sections" => await _db
                .BuildingSections.AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "floors" => await _db
                .Floors.AsNoTracking()
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "unloading-points" => await _db
                .UnloadingPoints.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "storage-areas" => await _db
                .StorageAreas.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "carriers" => await _db
                .Carriers.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "vehicle-types" => await _db
                .VehicleTypes.AsNoTracking()
                .OrderBy(x => x.Name)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "vehicles" => await _db
                .Vehicles.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.RegistrationNumber)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.RegistrationNumber,
                    Name = x.Make + " " + x.Model,
                })
                .ToListAsync(ct),
            "transport-routes" => await _db
                .TransportRoutes.AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.Plant)
                .Include(x => x.ConstructionObject)
                .OrderBy(x => x.Id)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = null,
                    Name = x.Plant.Name + " → " + x.ConstructionObject.Name,
                })
                .ToListAsync(ct),
            "construction-takts" => await _db
                .ConstructionTakts.AsNoTracking()
                .OrderBy(x => x.Sequence)
                .ThenBy(x => x.Code)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.Code,
                    Name = x.Name,
                })
                .ToListAsync(ct),
            "products" => await _db
                .Products.AsNoTracking()
                .OrderBy(x => x.ProductCode)
                .Select(x => new LookupItem
                {
                    Id = x.Id.ToString(),
                    Code = x.ProductCode,
                    Name = x.Mark,
                })
                .ToListAsync(ct),
            _ => throw new ArgumentException($"Unknown entity '{entity}'."),
        };
    }

    public async Task<(byte[] Bytes, string FileName)> ExportAsync(
        string entity,
        CancellationToken ct = default
    )
    {
        var all = new PagedQuery
        {
            Page = 1,
            PageSize = 100_000,
            ActiveOnly = false,
            Search = null,
        };
        IReadOnlyList<IReadOnlyDictionary<string, object?>> rows = entity.ToLowerInvariant() switch
        {
            "product-types" => (await ListProductTypesAsync(all, ct)).Items.Select(ToDict).ToList(),
            "plants" => (await ListPlantsAsync(all, ct)).Items.Select(ToDict).ToList(),
            "production-lines" => (await ListProductionLinesAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "line-capabilities" => (await ListLineCapabilitiesAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "construction-objects" => (await ListConstructionObjectsAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "building-sections" => (await ListBuildingSectionsAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "floors" => (await ListFloorsAsync(all, ct)).Items.Select(ToDict).ToList(),
            "unloading-points" => (await ListUnloadingPointsAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "storage-areas" => (await ListStorageAreasAsync(all, ct)).Items.Select(ToDict).ToList(),
            "carriers" => (await ListCarriersAsync(all, ct)).Items.Select(ToDict).ToList(),
            "vehicle-types" => (await ListVehicleTypesAsync(all, ct)).Items.Select(ToDict).ToList(),
            "vehicles" => (await ListVehiclesAsync(all, ct)).Items.Select(ToDict).ToList(),
            "transport-routes" => (await ListTransportRoutesAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "construction-takts" => (await ListConstructionTaktsAsync(all, ct))
                .Items.Select(ToDict)
                .ToList(),
            "products" => (await ListProductsAsync(all, ct)).Items.Select(ToDict).ToList(),
            _ => throw new ArgumentException($"Unknown entity '{entity}'."),
        };

        var bytes = ExcelHelper.Export(rows);
        return (bytes, $"{entity}.xlsx");
    }

    public async Task<ImportResult> ImportAsync(
        string entity,
        Stream stream,
        CancellationToken ct = default
    )
    {
        var rows = ExcelHelper.ReadRows(stream);
        return entity.ToLowerInvariant() switch
        {
            "product-types" => await ImportProductTypesAsync(rows, ct),
            "plants" => await ImportPlantsAsync(rows, ct),
            "production-lines" => await ImportProductionLinesAsync(rows, ct),
            "line-capabilities" => await ImportLineCapabilitiesAsync(rows, ct),
            "construction-objects" => await ImportConstructionObjectsAsync(rows, ct),
            "building-sections" => await ImportBuildingSectionsAsync(rows, ct),
            "floors" => await ImportFloorsAsync(rows, ct),
            "unloading-points" => await ImportUnloadingPointsAsync(rows, ct),
            "storage-areas" => await ImportStorageAreasAsync(rows, ct),
            "carriers" => await ImportCarriersAsync(rows, ct),
            "vehicle-types" => await ImportVehicleTypesAsync(rows, ct),
            "vehicles" => await ImportVehiclesAsync(rows, ct),
            "transport-routes" => await ImportTransportRoutesAsync(rows, ct),
            "construction-takts" => await ImportConstructionTaktsAsync(rows, ct),
            "products" => await ImportProductsAsync(rows, ct),
            _ => throw new ArgumentException($"Unknown entity '{entity}'."),
        };
    }

    // ── import implementations ───────────────────────────────────────────────

    private async Task<ImportResult> ImportProductTypesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var dto = new ProductTypeWriteDto
                {
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    Description = ExcelHelper.GetStringOrNull(row, "Description"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.ProductTypes.FirstOrDefaultAsync(x => x.Code == code, ct);
                if (existing is null)
                {
                    await CreateProductTypeAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateProductTypeAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportPlantsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var dto = new PlantWriteDto
                {
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    Address = ExcelHelper.GetString(row, "Address"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.Plants.FirstOrDefaultAsync(x => x.Code == code, ct);
                if (existing is null)
                {
                    await CreatePlantAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdatePlantAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportProductionLinesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var plantId =
                    ExcelHelper.GetInt(row, "PlantId")
                    ?? throw new ArgumentException("PlantId is required.");
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var dto = new ProductionLineWriteDto
                {
                    PlantId = plantId,
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.ProductionLines.FirstOrDefaultAsync(
                    x => x.PlantId == plantId && x.Code == code,
                    ct
                );
                if (existing is null)
                {
                    await CreateProductionLineAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateProductionLineAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportLineCapabilitiesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var lineId =
                    ExcelHelper.GetInt(row, "ProductionLineId")
                    ?? throw new ArgumentException("ProductionLineId is required.");
                var typeId =
                    ExcelHelper.GetInt(row, "ProductTypeId")
                    ?? throw new ArgumentException("ProductTypeId is required.");
                var dto = new LineCapabilityWriteDto
                {
                    ProductionLineId = lineId,
                    ProductTypeId = typeId,
                    DefaultDailyCapacityUnits = ExcelHelper.GetInt(row, "DefaultDailyCapacityUnits") ?? 0,
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.LineCapabilities.FirstOrDefaultAsync(
                    x => x.ProductionLineId == lineId && x.ProductTypeId == typeId,
                    ct
                );
                if (existing is null)
                {
                    await CreateLineCapabilityAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateLineCapabilityAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportConstructionObjectsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var dto = new ConstructionObjectWriteDto
                {
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    Address = ExcelHelper.GetString(row, "Address"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.ConstructionObjects.FirstOrDefaultAsync(
                    x => x.Code == code,
                    ct
                );
                if (existing is null)
                {
                    await CreateConstructionObjectAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateConstructionObjectAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportBuildingSectionsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var objectId =
                    ExcelHelper.GetInt(row, "ConstructionObjectId")
                    ?? throw new ArgumentException("ConstructionObjectId is required.");
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var dto = new BuildingSectionWriteDto
                {
                    ConstructionObjectId = objectId,
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    SortOrder = ExcelHelper.GetInt(row, "SortOrder") ?? 0,
                };
                var existing = await _db.BuildingSections.FirstOrDefaultAsync(
                    x => x.ConstructionObjectId == objectId && x.Code == code,
                    ct
                );
                if (existing is null)
                {
                    await CreateBuildingSectionAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateBuildingSectionAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportFloorsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var dto = new FloorWriteDto
                {
                    BuildingSectionId =
                        ExcelHelper.GetInt(row, "BuildingSectionId")
                        ?? throw new ArgumentException("BuildingSectionId is required."),
                    Number = ExcelHelper.GetInt(row, "Number"),
                    Name = ExcelHelper.GetString(row, "Name"),
                    SortOrder = ExcelHelper.GetInt(row, "SortOrder") ?? 0,
                };
                var id = ExcelHelper.GetInt(row, "Id");
                if (id is null)
                {
                    await CreateFloorAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateFloorAsync(id.Value, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportUnloadingPointsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var objectId =
                    ExcelHelper.GetInt(row, "ConstructionObjectId")
                    ?? throw new ArgumentException("ConstructionObjectId is required.");
                var name = ExcelHelper.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name is required.");
                var dto = new UnloadingPointWriteDto
                {
                    ConstructionObjectId = objectId,
                    Name = name,
                    Description = ExcelHelper.GetStringOrNull(row, "Description"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.UnloadingPoints.FirstOrDefaultAsync(
                    x => x.ConstructionObjectId == objectId && x.Name == name,
                    ct
                );
                if (existing is null)
                {
                    await CreateUnloadingPointAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateUnloadingPointAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportStorageAreasAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var plantId = ExcelHelper.GetInt(row, "PlantId");
                var objectId = ExcelHelper.GetInt(row, "ConstructionObjectId");
                ValidateStorageOwner(plantId, objectId);
                var name = ExcelHelper.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name is required.");
                var dto = new StorageAreaWriteDto
                {
                    PlantId = plantId,
                    ConstructionObjectId = objectId,
                    Name = name,
                    CapacityUnits = ExcelHelper.GetInt(row, "CapacityUnits") ?? 0,
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                StorageArea? existing;
                if (plantId.HasValue)
                {
                    existing = await _db.StorageAreas.FirstOrDefaultAsync(
                        x => x.PlantId == plantId && x.Name == name,
                        ct
                    );
                }
                else
                {
                    existing = await _db.StorageAreas.FirstOrDefaultAsync(
                        x => x.ConstructionObjectId == objectId && x.Name == name,
                        ct
                    );
                }

                if (existing is null)
                {
                    await CreateStorageAreaAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateStorageAreaAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportCarriersAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var name = ExcelHelper.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name is required.");
                var dto = new CarrierWriteDto
                {
                    Name = name,
                    ContactInfo = ExcelHelper.GetStringOrNull(row, "ContactInfo"),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.Carriers.FirstOrDefaultAsync(x => x.Name == name, ct);
                if (existing is null)
                {
                    await CreateCarrierAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateCarrierAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportVehicleTypesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var name = ExcelHelper.GetString(row, "Name");
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Name is required.");
                var dto = new VehicleTypeWriteDto
                {
                    Name = name,
                    RollingStockType = ExcelHelper.GetStringOrNull(row, "RollingStockType"),
                    MinPayloadKg = ExcelHelper.GetDecimal(row, "MinPayloadKg"),
                    MaxPayloadKg = ExcelHelper.GetDecimal(row, "MaxPayloadKg"),
                    LoadingPlatformCount = ExcelHelper.GetInt(row, "LoadingPlatformCount") ?? 0,
                    LoadingPlatformLengthMm = ExcelHelper.GetDecimal(row, "LoadingPlatformLengthMm"),
                    LoadingPlatformWidthMm = ExcelHelper.GetDecimal(row, "LoadingPlatformWidthMm"),
                    AllowedRightLeftImbalanceKg = ExcelHelper.GetDecimal(
                        row,
                        "AllowedRightLeftImbalanceKg"
                    ),
                    MaxCargoHeightMm = ExcelHelper.GetDecimal(row, "MaxCargoHeightMm"),
                    CargoVolumeM3 = ExcelHelper.GetDecimal(row, "CargoVolumeM3"),
                    TotalTrainLengthMm = ExcelHelper.GetDecimal(row, "TotalTrainLengthMm"),
                    TurningRadiusMm = ExcelHelper.GetDecimal(row, "TurningRadiusMm"),
                    Notes = ExcelHelper.GetStringOrNull(row, "Notes"),
                };
                var existing = await _db.VehicleTypes.FirstOrDefaultAsync(x => x.Name == name, ct);
                if (existing is null)
                {
                    await CreateVehicleTypeAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateVehicleTypeAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportVehiclesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var reg = ExcelHelper.GetString(row, "RegistrationNumber");
                if (string.IsNullOrWhiteSpace(reg))
                    throw new ArgumentException("RegistrationNumber is required.");
                var dto = new VehicleWriteDto
                {
                    VehicleTypeId =
                        ExcelHelper.GetInt(row, "VehicleTypeId")
                        ?? throw new ArgumentException("VehicleTypeId is required."),
                    CarrierId =
                        ExcelHelper.GetInt(row, "CarrierId")
                        ?? throw new ArgumentException("CarrierId is required."),
                    Make = ExcelHelper.GetString(row, "Make"),
                    Model = ExcelHelper.GetString(row, "Model"),
                    RegistrationNumber = reg,
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                    Notes = ExcelHelper.GetStringOrNull(row, "Notes"),
                };
                var existing = await _db.Vehicles.FirstOrDefaultAsync(
                    x => x.RegistrationNumber == reg,
                    ct
                );
                if (existing is null)
                {
                    await CreateVehicleAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateVehicleAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportTransportRoutesAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var plantId =
                    ExcelHelper.GetInt(row, "PlantId")
                    ?? throw new ArgumentException("PlantId is required.");
                var objectId =
                    ExcelHelper.GetInt(row, "ConstructionObjectId")
                    ?? throw new ArgumentException("ConstructionObjectId is required.");
                var dto = new TransportRouteWriteDto
                {
                    PlantId = plantId,
                    ConstructionObjectId = objectId,
                    DistanceKm = ExcelHelper.GetDecimal(row, "DistanceKm"),
                    EstimatedTravelMinutes = ExcelHelper.GetInt(row, "EstimatedTravelMinutes"),
                    TurnoverCoefficientPerDay = ExcelHelper.GetDecimal(
                        row,
                        "TurnoverCoefficientPerDay"
                    ),
                    IsActive = ExcelHelper.GetBool(row, "IsActive") ?? true,
                };
                var existing = await _db.TransportRoutes.FirstOrDefaultAsync(
                    x => x.PlantId == plantId && x.ConstructionObjectId == objectId,
                    ct
                );
                if (existing is null)
                {
                    await CreateTransportRouteAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateTransportRouteAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportConstructionTaktsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var objectId =
                    ExcelHelper.GetInt(row, "ConstructionObjectId")
                    ?? throw new ArgumentException("ConstructionObjectId is required.");
                var code = ExcelHelper.GetString(row, "Code");
                if (string.IsNullOrWhiteSpace(code))
                    throw new ArgumentException("Code is required.");
                var statusText = ExcelHelper.GetStringOrNull(row, "Status");
                var status = ConstructionTaktStatus.Draft;
                if (
                    !string.IsNullOrWhiteSpace(statusText)
                    && !Enum.TryParse(statusText, true, out status)
                )
                    throw new ArgumentException($"Invalid Status '{statusText}'.");
                var start =
                    ExcelHelper.GetDateOnly(row, "PlannedProductionStartDate")
                    ?? throw new ArgumentException("PlannedProductionStartDate is required.");
                var end =
                    ExcelHelper.GetDateOnly(row, "PlannedProductionEndDate")
                    ?? throw new ArgumentException("PlannedProductionEndDate is required.");
                var dto = new ConstructionTaktWriteDto
                {
                    ConstructionObjectId = objectId,
                    Code = code,
                    Name = ExcelHelper.GetString(row, "Name"),
                    Sequence = ExcelHelper.GetInt(row, "Sequence") ?? 0,
                    PlannedProductionStartDate = start,
                    PlannedProductionEndDate = end,
                    Status = status,
                    Comment = ExcelHelper.GetStringOrNull(row, "Comment"),
                };
                var existing = await _db.ConstructionTakts.FirstOrDefaultAsync(
                    x => x.ConstructionObjectId == objectId && x.Code == code,
                    ct
                );
                if (existing is null)
                {
                    await CreateConstructionTaktAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateConstructionTaktAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }

    private async Task<ImportResult> ImportProductsAsync(
        List<Dictionary<string, string>> rows,
        CancellationToken ct
    )
    {
        var result = new ImportResult();
        for (var i = 0; i < rows.Count; i++)
        {
            var rowNum = i + 2;
            try
            {
                var row = rows[i];
                var productCode = ExcelHelper.GetString(row, "ProductCode");
                if (string.IsNullOrWhiteSpace(productCode))
                    throw new ArgumentException("ProductCode is required.");
                var statusText = ExcelHelper.GetStringOrNull(row, "Status");
                var status = ProductStatus.Created;
                if (
                    !string.IsNullOrWhiteSpace(statusText)
                    && !Enum.TryParse(statusText, true, out status)
                )
                    throw new ArgumentException($"Invalid Status '{statusText}'.");
                var dto = new ProductWriteDto
                {
                    ProductCode = productCode,
                    ProductTypeId =
                        ExcelHelper.GetInt(row, "ProductTypeId")
                        ?? throw new ArgumentException("ProductTypeId is required."),
                    Mark = ExcelHelper.GetString(row, "Mark"),
                    WidthMm = ExcelHelper.GetDecimal(row, "WidthMm") ?? 0,
                    HeightMm = ExcelHelper.GetDecimal(row, "HeightMm") ?? 0,
                    ThicknessMm = ExcelHelper.GetDecimal(row, "ThicknessMm") ?? 0,
                    CorniceWidthIncreaseMm = ExcelHelper.GetDecimal(row, "CorniceWidthIncreaseMm") ?? 0,
                    TotalWidthWithCorniceMm =
                        ExcelHelper.GetDecimal(row, "TotalWidthWithCorniceMm") ?? 0,
                    ThicknessIncreaseMm = ExcelHelper.GetDecimal(row, "ThicknessIncreaseMm") ?? 0,
                    RightBendMm = ExcelHelper.GetDecimal(row, "RightBendMm") ?? 0,
                    LeftBendMm = ExcelHelper.GetDecimal(row, "LeftBendMm") ?? 0,
                    CladdingWidthWithBendsMm =
                        ExcelHelper.GetDecimal(row, "CladdingWidthWithBendsMm") ?? 0,
                    WeightKg = ExcelHelper.GetDecimal(row, "WeightKg"),
                    Status = status,
                    AdditionalInfo = ExcelHelper.GetStringOrNull(row, "AdditionalInfo"),
                };
                var existing = await _db.Products.FirstOrDefaultAsync(
                    x => x.ProductCode == productCode,
                    ct
                );
                if (existing is null)
                {
                    await CreateProductAsync(dto, ct);
                    result.Created++;
                }
                else
                {
                    await UpdateProductAsync(existing.Id, dto, ct);
                    result.Updated++;
                }
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Row {rowNum}: {ex.Message}");
            }
        }

        return result;
    }
}

using DispatcherApp.Domain.Entities;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

/// <summary>
/// Демо-данные для тестирования.
/// Seed — явные вставки сущностей в коде (можно править руками).
/// Clear — полный сброс бизнес-таблиц (порядок удаления с учётом FK Restrict).
/// </summary>
public sealed class DemoDataService
{
    private readonly DispatcherDbContext _db;

    public DemoDataService(DispatcherDbContext db) => _db = db;

    public sealed class DemoDataResult
    {
        public required string Message { get; init; }
        public int Affected { get; init; }
    }

    /// <summary>Удаляет все бизнес-данные. Справочники тоже.</summary>
    public async Task<DemoDataResult> ClearAsync(CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        var affected = 0;

        // Цикл Demand ↔ DemandRevision: сначала обнуляем CurrentRevisionId
        affected += await _db.Demands.ExecuteUpdateAsync(
            s => s.SetProperty(d => d.CurrentRevisionId, (long?)null),
            ct
        );

        affected += await DeleteAllAsync(_db.TripItems, ct);
        affected += await DeleteAllAsync(_db.Trips, ct);
        affected += await DeleteAllAsync(_db.ProductionAssignments, ct);
        affected += await DeleteAllAsync(_db.StoragePlacements, ct);
        affected += await DeleteAllAsync(_db.CapacityOverrides, ct);
        affected += await DeleteAllAsync(_db.DemandRevisions, ct);
        affected += await DeleteAllAsync(_db.Demands, ct);
        affected += await DeleteAllAsync(_db.TaktAssignments, ct);
        affected += await DeleteAllAsync(_db.ProductAssignments, ct);
        affected += await DeleteAllAsync(_db.ProductIdentifiers, ct);
        affected += await DeleteAllAsync(_db.ProjectPositions, ct);
        affected += await DeleteAllAsync(_db.Products, ct);
        affected += await DeleteAllAsync(_db.LineCapabilities, ct);
        affected += await DeleteAllAsync(_db.ProductionLines, ct);
        affected += await DeleteAllAsync(_db.ConstructionTakts, ct);
        affected += await DeleteAllAsync(_db.Floors, ct);
        affected += await DeleteAllAsync(_db.BuildingSections, ct);
        affected += await DeleteAllAsync(_db.UnloadingPoints, ct);
        affected += await DeleteAllAsync(_db.TransportRoutes, ct);
        affected += await DeleteAllAsync(_db.StorageAreas, ct);
        affected += await DeleteAllAsync(_db.Vehicles, ct);
        affected += await DeleteAllAsync(_db.VehicleTypes, ct);
        affected += await DeleteAllAsync(_db.Carriers, ct);
        affected += await DeleteAllAsync(_db.Plants, ct);
        affected += await DeleteAllAsync(_db.ConstructionObjects, ct);
        affected += await DeleteAllAsync(_db.ProductTypes, ct);
        affected += await DeleteAllAsync(_db.AuditEvents, ct);

        await transaction.CommitAsync(ct);
        return new DemoDataResult
            {
                Message = "Все данные очищены.",
                Affected = affected,
            };
    }

    /// <summary>
    /// Заполняет БД демо-справочниками.
    /// Править набор данных — ниже в методе SeedCoreAsync (явные new Entity { ... }).
    /// </summary>
    public async Task<DemoDataResult> SeedAsync(CancellationToken ct)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);
        if (await _db.Plants.AnyAsync(ct) || await _db.Products.AnyAsync(ct))
        {
            throw new ArgumentException("База не пустая. Сначала выполните сброс (clear).");
        }

        var count = await SeedCoreAsync(ct);
        await transaction.CommitAsync(ct);
        return new DemoDataResult
            {
                Message = "Демо-данные загружены. Набор правится в DemoDataService.SeedCoreAsync.",
                Affected = count,
            };
    }

    /// <summary>
    /// === РЕДАКТИРУЙ ЗДЕСЬ ===
    /// Явная вставка сущностей для тестов. Без рандома — всё задано в коде.
    /// </summary>
    private async Task<int> SeedCoreAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var count = 0;

        // --- Типы изделий ---
        var ptPanel = new ProductType
        {
            Code = "PANEL",
            Name = "Стеновая панель",
            Description = "Наружная стеновая панель",
            IsActive = true,
        };
        var ptFloor = new ProductType
        {
            Code = "FLOOR",
            Name = "Плита перекрытия",
            Description = "Сборная плита",
            IsActive = true,
        };
        _db.ProductTypes.AddRange(ptPanel, ptFloor);
        await _db.SaveChangesAsync(ct);
        count += 2;

        // --- Завод и линии ---
        var plant = new Plant
        {
            Code = "Z1",
            Name = "Завод №1",
            Address = "г. Санкт-Петербург, пр. Заводской, 1",
            IsActive = true,
        };
        _db.Plants.Add(plant);
        await _db.SaveChangesAsync(ct);
        count++;

        var lineA = new ProductionLine
        {
            PlantId = plant.Id,
            Code = "L-A",
            Name = "Линия A (панели)",
            IsActive = true,
        };
        var lineB = new ProductionLine
        {
            PlantId = plant.Id,
            Code = "L-B",
            Name = "Линия B (плиты)",
            IsActive = true,
        };
        _db.ProductionLines.AddRange(lineA, lineB);
        await _db.SaveChangesAsync(ct);
        count += 2;

        _db.LineCapabilities.AddRange(
            new LineCapability
            {
                ProductionLineId = lineA.Id,
                ProductTypeId = ptPanel.Id,
                DefaultDailyCapacityUnits = 12,
                IsActive = true,
            },
            new LineCapability
            {
                ProductionLineId = lineB.Id,
                ProductTypeId = ptFloor.Id,
                DefaultDailyCapacityUnits = 8,
                IsActive = true,
            }
        );
        await _db.SaveChangesAsync(ct);
        count += 2;

        // --- Строительный объект ---
        var obj = new ConstructionObject
        {
            Code = "OBJ-DEMO",
            Name = "ЖК Демо",
            Address = "г. Санкт-Петербург, ул. Строителей, 10",
            IsActive = true,
        };
        _db.ConstructionObjects.Add(obj);
        await _db.SaveChangesAsync(ct);
        count++;

        var section = new BuildingSection
        {
            ConstructionObjectId = obj.Id,
            Code = "S1",
            Name = "Секция 1",
            SortOrder = 1,
        };
        _db.BuildingSections.Add(section);
        await _db.SaveChangesAsync(ct);
        count++;

        var floor1 = new Floor
        {
            BuildingSectionId = section.Id,
            Number = 1,
            Name = "1 этаж",
            SortOrder = 1,
        };
        var floor2 = new Floor
        {
            BuildingSectionId = section.Id,
            Number = 2,
            Name = "2 этаж",
            SortOrder = 2,
        };
        _db.Floors.AddRange(floor1, floor2);
        await _db.SaveChangesAsync(ct);
        count += 2;

        _db.UnloadingPoints.Add(
            new UnloadingPoint
            {
                ConstructionObjectId = obj.Id,
                Name = "Рампа 1",
                Description = "Основная точка разгрузки",
                IsActive = true,
            }
        );
        await _db.SaveChangesAsync(ct);
        count++;

        var takt = new ConstructionTakt
        {
            ConstructionObjectId = obj.Id,
            Code = "T1",
            Name = "Такт 1",
            Sequence = 1,
            PlannedProductionStartDate = DateOnly.FromDateTime(DateTime.UtcNow.Date),
            PlannedProductionEndDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(14)),
            Status = ConstructionTaktStatus.Planned,
            Comment = "Демо-такт",
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };
        _db.ConstructionTakts.Add(takt);
        await _db.SaveChangesAsync(ct);
        count++;

        // --- Склады ---
        _db.StorageAreas.AddRange(
            new StorageArea
            {
                PlantId = plant.Id,
                Name = "Склад завода З1",
                CapacityUnits = 200,
                IsActive = true,
            },
            new StorageArea
            {
                ConstructionObjectId = obj.Id,
                Name = "Площадка хранения на объекте",
                CapacityUnits = 50,
                IsActive = true,
            }
        );
        await _db.SaveChangesAsync(ct);
        count += 2;

        // --- Логистика ---
        var carrier = new Carrier
        {
            Name = "ООО ТрансДемо",
            ContactInfo = "+7 (812) 000-00-00",
            IsActive = true,
        };
        _db.Carriers.Add(carrier);
        await _db.SaveChangesAsync(ct);
        count++;

        var vType = new VehicleType
        {
            Name = "Панелевоз 3 оси",
            RollingStockType = "полуприцеп",
            MinPayloadKg = 10000,
            MaxPayloadKg = 25000,
            LoadingPlatformCount = 1,
            LoadingPlatformLengthMm = 13600,
            LoadingPlatformWidthMm = 2500,
            MaxCargoHeightMm = 4000,
            Notes = "Демо-тип ТС",
        };
        _db.VehicleTypes.Add(vType);
        await _db.SaveChangesAsync(ct);
        count++;

        _db.Vehicles.Add(
            new Vehicle
            {
                VehicleTypeId = vType.Id,
                CarrierId = carrier.Id,
                Make = "МАЗ",
                Model = "5440",
                RegistrationNumber = "А123ВС178",
                IsActive = true,
                Notes = "Демо-машина",
            }
        );
        await _db.SaveChangesAsync(ct);
        count++;

        _db.TransportRoutes.Add(
            new TransportRoute
            {
                PlantId = plant.Id,
                ConstructionObjectId = obj.Id,
                DistanceKm = 42.5m,
                EstimatedTravelMinutes = 75,
                TurnoverCoefficientPerDay = 1.5m,
                IsActive = true,
            }
        );
        await _db.SaveChangesAsync(ct);
        count++;

        // --- Изделия ---
        var p1 = new Product
        {
            ProductCode = "PRD-00000001",
            ProductTypeId = ptPanel.Id,
            Mark = "НС-1",
            WidthMm = 3000,
            HeightMm = 2800,
            ThicknessMm = 300,
            CorniceWidthIncreaseMm = 50,
            TotalWidthWithCorniceMm = 3050,
            ThicknessIncreaseMm = 0,
            RightBendMm = 0,
            LeftBendMm = 0,
            CladdingWidthWithBendsMm = 3000,
            WeightKg = 4500,
            Status = ProductStatus.Created,
            AdditionalInfo = "Демо-панель",
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };
        var p2 = new Product
        {
            ProductCode = "PRD-00000002",
            ProductTypeId = ptFloor.Id,
            Mark = "ПТ-1",
            WidthMm = 1500,
            HeightMm = 6000,
            ThicknessMm = 220,
            CorniceWidthIncreaseMm = 0,
            TotalWidthWithCorniceMm = 1500,
            ThicknessIncreaseMm = 0,
            RightBendMm = 0,
            LeftBendMm = 0,
            CladdingWidthWithBendsMm = 1500,
            WeightKg = 3200,
            Status = ProductStatus.Created,
            AdditionalInfo = "Демо-плита",
            CreatedAt = now,
            UpdatedAt = now,
            Version = 1,
        };
        _db.Products.AddRange(p1, p2);
        await _db.SaveChangesAsync(ct);
        count += 2;

        _db.ProjectPositions.AddRange(
            new ProjectPosition
            {
                ProductId = p1.Id,
                ConstructionObjectId = obj.Id,
                BuildingSectionId = section.Id,
                FloorId = floor1.Id,
                InstallationNumber = "1-1",
            },
            new ProjectPosition
            {
                ProductId = p2.Id,
                ConstructionObjectId = obj.Id,
                BuildingSectionId = section.Id,
                FloorId = floor2.Id,
                InstallationNumber = "2-1",
            }
        );
        await _db.SaveChangesAsync(ct);
        count += 2;

        _db.ProductIdentifiers.Add(
            new ProductIdentifier
            {
                ProductId = p1.Id,
                Type = ProductIdentifierType.QrCode,
                Value = "QR-PRD-00000001",
                IsActive = true,
                AssignedAt = now,
            }
        );
        await _db.SaveChangesAsync(ct);
        count++;

        return count;
    }

    private static async Task<int> DeleteAllAsync<T>(DbSet<T> set, CancellationToken ct)
        where T : class
    {
        // ExecuteDelete — эффективно и не грузит сущности в память
        return await set.ExecuteDeleteAsync(ct);
    }
}

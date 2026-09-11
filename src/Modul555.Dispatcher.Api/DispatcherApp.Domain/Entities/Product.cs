using DispatcherApp.Domain.Enums;

namespace DispatcherApp.Domain.Entities;

public class Product
{
    public long Id { get; set; }

    /// <summary>
    /// Внутренний неизменяемый код изделия.
    /// Например: PRD-00001258.
    /// </summary>
    public string ProductCode { get; set; } = null!;


    // Тип изделия

    public int ProductTypeId { get; set; }

    public ProductType ProductType { get; set; } = null!;


    // Основные сведения

    public string Mark { get; set; } = null!;


    // Геометрия, мм

    public decimal WidthMm { get; set; }

    public decimal HeightMm { get; set; }

    public decimal ThicknessMm { get; set; }

    public decimal CorniceWidthIncreaseMm { get; set; }

    public decimal TotalWidthWithCorniceMm { get; set; }

    public decimal ThicknessIncreaseMm { get; set; }

    public decimal RightBendMm { get; set; }

    public decimal LeftBendMm { get; set; }

    public decimal CladdingWidthWithBendsMm { get; set; }


    // Транспортные характеристики

    public decimal? WeightKg { get; set; }


    // Состояние

    public ProductStatus Status { get; set; }
        = ProductStatus.Created;


    // Дополнительные сведения

    public string? AdditionalInfo { get; set; }


    // Системные поля

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    /// <summary>
    /// Используется для защиты от одновременного
    /// изменения записи несколькими диспетчерами.
    /// </summary>
    public long Version { get; set; }


    // Navigation

    public ICollection<ProductIdentifier> Identifiers { get; set; }
        = new List<ProductIdentifier>();

    public ProjectPosition? ProjectPosition { get; set; }

    public ICollection<ProductAssignment> Assignments { get; set; }
        = new List<ProductAssignment>();
    
    public ICollection<TaktAssignment> TaktAssignments { get; set; }
    = new List<TaktAssignment>();

    public ICollection<Demand> Demands { get; set; }
        = new List<Demand>();

    public ICollection<StoragePlacement> StoragePlacements { get; set; }
        = new List<StoragePlacement>();

    public ICollection<TripItem> TripItems { get; set; }
        = new List<TripItem>();
}
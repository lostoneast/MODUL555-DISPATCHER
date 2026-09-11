namespace DispatcherApp.Domain.Entities;

public class Floor
{
    public int Id { get; set; }

    public int BuildingSectionId { get; set; }

    public BuildingSection BuildingSection { get; set; } = null!;


    /// <summary>
    /// Числовое значение этажа, если применимо.
    /// Например -1, 1, 2, 15.
    /// </summary>
    public int? Number { get; set; }

    /// <summary>
    /// Отображаемое название:
    /// "1 этаж", "Технический этаж", "Кровля".
    /// </summary>
    public string Name { get; set; } = null!;

    public int SortOrder { get; set; }
}
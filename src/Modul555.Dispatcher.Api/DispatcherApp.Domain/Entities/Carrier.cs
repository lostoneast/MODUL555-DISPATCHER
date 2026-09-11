namespace DispatcherApp.Domain.Entities;

public class Carrier
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactInfo { get; set; }

    public bool IsActive { get; set; } = true;


    public ICollection<Vehicle> Vehicles { get; set; }
        = new List<Vehicle>();
}
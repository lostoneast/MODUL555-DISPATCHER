namespace DispatcherApp.Domain.Entities;

public class Vehicle
{
    public long Id { get; set; }


    public int VehicleTypeId { get; set; }

    public VehicleType VehicleType { get; set; } = null!;


    public int CarrierId { get; set; }

    public Carrier Carrier { get; set; } = null!;


    public string Make { get; set; } = null!;

    public string Model { get; set; } = null!;

    public string RegistrationNumber { get; set; } = null!;


    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }


    public ICollection<Trip> Trips { get; set; }
        = new List<Trip>();
}
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/vehicle-types")]
public sealed class VehicleTypesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "vehicle-types")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<VehicleTypeDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListVehicleTypesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleTypeDto>> Create([FromBody] VehicleTypeWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateVehicleTypeAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<VehicleTypeDto>> Update(int id, [FromBody] VehicleTypeWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateVehicleTypeAsync(id, dto, ct));
}

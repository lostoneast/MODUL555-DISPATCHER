using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/vehicles")]
public sealed class VehiclesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "vehicles")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<VehicleDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListVehiclesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<VehicleDto>> Create([FromBody] VehicleWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateVehicleAsync(dto, ct));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<VehicleDto>> Update(long id, [FromBody] VehicleWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateVehicleAsync(id, dto, ct));

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken ct)
    {
        await Catalogs.DeactivateVehicleAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:long}/activate")]
    public async Task<IActionResult> Activate(long id, CancellationToken ct)
    {
        await Catalogs.ActivateVehicleAsync(id, ct);
        return NoContent();
    }
}

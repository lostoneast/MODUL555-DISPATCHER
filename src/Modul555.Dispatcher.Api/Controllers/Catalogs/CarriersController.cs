using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/carriers")]
public sealed class CarriersController(CatalogService catalogs) : CatalogControllerBase(catalogs, "carriers")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<CarrierDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListCarriersAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<CarrierDto>> Create([FromBody] CarrierWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateCarrierAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CarrierDto>> Update(int id, [FromBody] CarrierWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateCarrierAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateCarrierAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateCarrierAsync(id, ct);
        return NoContent();
    }
}

using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/transport-routes")]
public sealed class TransportRoutesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "transport-routes")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<TransportRouteDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListTransportRoutesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<TransportRouteDto>> Create([FromBody] TransportRouteWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateTransportRouteAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TransportRouteDto>> Update(int id, [FromBody] TransportRouteWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateTransportRouteAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateTransportRouteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateTransportRouteAsync(id, ct);
        return NoContent();
    }
}

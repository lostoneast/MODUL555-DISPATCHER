using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/line-capabilities")]
public sealed class LineCapabilitiesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "line-capabilities")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<LineCapabilityDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListLineCapabilitiesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<LineCapabilityDto>> Create([FromBody] LineCapabilityWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateLineCapabilityAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LineCapabilityDto>> Update(int id, [FromBody] LineCapabilityWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateLineCapabilityAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateLineCapabilityAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateLineCapabilityAsync(id, ct);
        return NoContent();
    }
}

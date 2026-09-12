using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/unloading-points")]
public sealed class UnloadingPointsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "unloading-points")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UnloadingPointDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListUnloadingPointsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<UnloadingPointDto>> Create([FromBody] UnloadingPointWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateUnloadingPointAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UnloadingPointDto>> Update(int id, [FromBody] UnloadingPointWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateUnloadingPointAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateUnloadingPointAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateUnloadingPointAsync(id, ct);
        return NoContent();
    }
}

using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/plants")]
public sealed class PlantsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "plants")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<PlantDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListPlantsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<PlantDto>> Create([FromBody] PlantWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreatePlantAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<PlantDto>> Update(int id, [FromBody] PlantWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdatePlantAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivatePlantAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivatePlantAsync(id, ct);
        return NoContent();
    }
}

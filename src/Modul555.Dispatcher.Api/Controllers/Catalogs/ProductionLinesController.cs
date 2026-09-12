using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/production-lines")]
public sealed class ProductionLinesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "production-lines")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductionLineDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListProductionLinesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<ProductionLineDto>> Create([FromBody] ProductionLineWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateProductionLineAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductionLineDto>> Update(int id, [FromBody] ProductionLineWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateProductionLineAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateProductionLineAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateProductionLineAsync(id, ct);
        return NoContent();
    }
}

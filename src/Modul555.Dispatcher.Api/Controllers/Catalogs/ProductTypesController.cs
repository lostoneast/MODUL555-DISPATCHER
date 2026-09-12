using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/product-types")]
public sealed class ProductTypesController(CatalogService catalogs) : CatalogControllerBase(catalogs, "product-types")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductTypeDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListProductTypesAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<ProductTypeDto>> Create([FromBody] ProductTypeWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateProductTypeAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductTypeDto>> Update(int id, [FromBody] ProductTypeWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateProductTypeAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateProductTypeAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateProductTypeAsync(id, ct);
        return NoContent();
    }
}

using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/products")]
public sealed class ProductsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "products")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListProductsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateProductAsync(dto, ct));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductDto>> Update(long id, [FromBody] ProductWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateProductAsync(id, dto, ct));
}

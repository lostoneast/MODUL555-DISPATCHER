using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/products")]
public sealed class ProductsController(CatalogService catalogs, ProductListService products, ProductDetailService details) : CatalogControllerBase(catalogs, "products")
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductDetailDto>> Detail(long id, CancellationToken ct)
        => Ok(await details.GetAsync(id, ct));

    [HttpGet("{id:long}/available-trips")]
    public async Task<ActionResult<IReadOnlyList<ProductTripOption>>> AvailableTrips(long id, [FromQuery] int? objectId, CancellationToken ct)
        => Ok(await details.TripsAsync(id, objectId, ct));

    [HttpPut("{id:long}/details")]
    public async Task<ActionResult<ProductDetailDto>> UpdateDetails(long id, [FromBody] ProductDetailWriteDto dto, CancellationToken ct)
        => Ok(await details.UpdateAsync(id, dto, User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value, ct));

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductListDto>>> List([FromQuery] ProductListQuery query, CancellationToken ct)
        => Ok(await products.ListAsync(query, ct));

    [HttpGet("fields")]
    public ActionResult<IReadOnlyList<ProductFieldInfo>> Fields() => Ok(ProductListFilters.Describe());

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] ProductWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateProductAsync(dto, ct));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductDto>> Update(long id, [FromBody] ProductWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateProductAsync(id, dto, ct));
}

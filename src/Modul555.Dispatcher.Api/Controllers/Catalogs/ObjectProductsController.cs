using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[ApiController]
[Authorize]
[Route("api/construction-objects/{objectId:int}/products")]
public sealed class ObjectProductsController(ObjectProductsService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ObjectProductDto>>> List(int objectId, [FromQuery] ObjectProductsQuery query, CancellationToken ct)
        => Ok(await service.ListAsync(objectId, query, ct));

    [HttpGet("structure")]
    public async Task<ActionResult<ObjectStructureDto>> Structure(int objectId, CancellationToken ct)
        => Ok(await service.StructureAsync(objectId, ct));

    [HttpPost]
    public async Task<ActionResult<ObjectProductDto>> Create(int objectId, [FromBody] ObjectProductWriteDto dto, CancellationToken ct)
        => Ok(await service.CreateAsync(objectId, dto, ct));

    [HttpPut("{productId:long}")]
    public async Task<ActionResult<ObjectProductDto>> Update(int objectId, long productId, [FromBody] ObjectProductWriteDto dto, CancellationToken ct)
        => Ok(await service.UpdateAsync(objectId, productId, dto, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export(int objectId, [FromQuery] ObjectProductsQuery query, CancellationToken ct)
        => File(await service.ExportAsync(objectId, query, ct),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"object-{objectId}-products.xlsx");
}

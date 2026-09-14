using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[ApiController]
[Authorize]
[Route("api/construction-objects/{objectId:int}/products")]
public sealed class ObjectProductsController(ObjectProductsService service, ObjectProductImportService importer) : ControllerBase
{
    [HttpGet("import-example")]
    public IActionResult ImportExample() => File(ObjectProductImportService.ExampleFile(),
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "products-import-example.xlsx");

    [HttpPost("import")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<ImportResult>> Import(int objectId, [FromForm] IFormFile file,
        [FromForm] string? suffix, [FromForm] int startNumber = 1, CancellationToken ct = default)
    {
        if (file.Length == 0 || !string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Выберите непустой файл .xlsx." });
        await using var stream = file.OpenReadStream();
        return Ok(await importer.ImportAsync(objectId, stream, suffix, startNumber, ct));
    }

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

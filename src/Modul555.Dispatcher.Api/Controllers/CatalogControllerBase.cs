using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

[ApiController]
[Authorize]
public abstract class CatalogControllerBase(CatalogService catalogs, string entity) : ControllerBase
{
    protected CatalogService Catalogs { get; } = catalogs;

    [HttpGet("lookups")]
    public async Task<ActionResult<List<LookupItem>>> Lookups(CancellationToken ct)
        => Ok(await Catalogs.LookupsAsync(entity, ct));

    [HttpGet("export")]
    public async Task<IActionResult> Export(CancellationToken ct)
    {
        var (bytes, fileName) = await Catalogs.ExportAsync(entity, ct);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpPost("import")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImportResult>> Import([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0)
            return BadRequest(new { message = "File is required." });
        await using var stream = file.OpenReadStream();
        return Ok(await Catalogs.ImportAsync(entity, stream, ct));
    }
}

using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

// Compatibility endpoint for existing API clients.
[ApiController]
[Authorize]
[Route("api/lookups")]
public sealed class LookupsController(CatalogService catalogs) : ControllerBase
{
    [HttpGet("{entity}")]
    public async Task<ActionResult<List<LookupItem>>> Get(string entity, CancellationToken ct)
    {
        try { return Ok(await catalogs.LookupsAsync(entity, ct)); }
        catch (ArgumentException) { return NotFound(); }
    }
}

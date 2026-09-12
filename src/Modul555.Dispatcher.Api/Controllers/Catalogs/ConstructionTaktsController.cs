using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/construction-takts")]
public sealed class ConstructionTaktsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "construction-takts")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ConstructionTaktDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListConstructionTaktsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<ConstructionTaktDto>> Create([FromBody] ConstructionTaktWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateConstructionTaktAsync(dto, ct));

    [HttpPut("{id:long}")]
    public async Task<ActionResult<ConstructionTaktDto>> Update(long id, [FromBody] ConstructionTaktWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateConstructionTaktAsync(id, dto, ct));
}

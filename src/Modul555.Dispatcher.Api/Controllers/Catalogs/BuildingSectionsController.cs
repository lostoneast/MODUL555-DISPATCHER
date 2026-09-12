using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/building-sections")]
public sealed class BuildingSectionsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "building-sections")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<BuildingSectionDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListBuildingSectionsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<BuildingSectionDto>> Create([FromBody] BuildingSectionWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateBuildingSectionAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<BuildingSectionDto>> Update(int id, [FromBody] BuildingSectionWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateBuildingSectionAsync(id, dto, ct));
}

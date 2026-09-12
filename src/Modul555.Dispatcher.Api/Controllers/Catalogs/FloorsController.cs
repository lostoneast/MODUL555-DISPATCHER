using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/floors")]
public sealed class FloorsController(CatalogService catalogs) : CatalogControllerBase(catalogs, "floors")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FloorDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListFloorsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<FloorDto>> Create([FromBody] FloorWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateFloorAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FloorDto>> Update(int id, [FromBody] FloorWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateFloorAsync(id, dto, ct));
}

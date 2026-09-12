using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/storage-areas")]
public sealed class StorageAreasController(CatalogService catalogs) : CatalogControllerBase(catalogs, "storage-areas")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<StorageAreaDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListStorageAreasAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<StorageAreaDto>> Create([FromBody] StorageAreaWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateStorageAreaAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<StorageAreaDto>> Update(int id, [FromBody] StorageAreaWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateStorageAreaAsync(id, dto, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await Catalogs.DeactivateStorageAreaAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateStorageAreaAsync(id, ct);
        return NoContent();
    }
}

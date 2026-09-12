using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[Route("api/construction-objects")]
public sealed class ConstructionObjectsController(CatalogService catalogs, ConstructionObjectDeletionService deletion) : CatalogControllerBase(catalogs, "construction-objects")
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<ConstructionObjectDto>>> List([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await Catalogs.ListConstructionObjectsAsync(query, ct));

    [HttpPost]
    public async Task<ActionResult<ConstructionObjectDto>> Create([FromBody] ConstructionObjectCreateDto dto, CancellationToken ct)
        => Ok(await Catalogs.CreateConstructionObjectAsync(dto, ct));

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ConstructionObjectDto>> Update(int id, [FromBody] ConstructionObjectWriteDto dto, CancellationToken ct)
        => Ok(await Catalogs.UpdateConstructionObjectAsync(id, dto, ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ConstructionObjectDto>> Get(int id, CancellationToken ct)
        => Ok(await Catalogs.GetConstructionObjectAsync(id, ct));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, [FromBody] ObjectDeletionRequest request, CancellationToken ct)
    {
        await deletion.DeleteAsync(id, request, ct);
        return NoContent();
    }

    [HttpGet("{id:int}/deletion-preview")]
    public async Task<ActionResult<ObjectDeletionPreview>> DeletionPreview(int id, CancellationToken ct)
        => Ok(await deletion.PreviewAsync(id, ct));

    [HttpPost("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken ct)
    {
        await Catalogs.ActivateConstructionObjectAsync(id, ct);
        return NoContent();
    }
}

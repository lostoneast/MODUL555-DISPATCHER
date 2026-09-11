using DispatcherApp.Application.Catalogs;
using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public sealed class CatalogsController : ControllerBase
{
    private readonly CatalogService _catalogs;

    public CatalogsController(CatalogService catalogs) => _catalogs = catalogs;

    [HttpGet("lookups/{entity}")]
    public async Task<ActionResult<List<LookupItem>>> Lookups(
        string entity,
        CancellationToken ct
    )
    {
        try
        {
            return Ok(await _catalogs.LookupsAsync(entity, ct));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("{entity}/export")]
    public async Task<IActionResult> Export(string entity, CancellationToken ct)
    {
        try
        {
            var (bytes, fileName) = await _catalogs.ExportAsync(entity, ct);
            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("{entity}/import")]
    [RequestSizeLimit(50_000_000)]
    public async Task<ActionResult<ImportResult>> Import(
        string entity,
        IFormFile file,
        CancellationToken ct
    )
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "File is required." });

        try
        {
            await using var stream = file.OpenReadStream();
            return Ok(await _catalogs.ImportAsync(entity, stream, ct));
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpGet("{entity}")]
    public async Task<IActionResult> List(
        string entity,
        [FromQuery] PagedQuery query,
        CancellationToken ct
    )
    {
        try
        {
            return entity.ToLowerInvariant() switch
            {
                "product-types" => Ok(await _catalogs.ListProductTypesAsync(query, ct)),
                "plants" => Ok(await _catalogs.ListPlantsAsync(query, ct)),
                "production-lines" => Ok(await _catalogs.ListProductionLinesAsync(query, ct)),
                "line-capabilities" => Ok(await _catalogs.ListLineCapabilitiesAsync(query, ct)),
                "construction-objects" => Ok(
                    await _catalogs.ListConstructionObjectsAsync(query, ct)
                ),
                "building-sections" => Ok(await _catalogs.ListBuildingSectionsAsync(query, ct)),
                "floors" => Ok(await _catalogs.ListFloorsAsync(query, ct)),
                "unloading-points" => Ok(await _catalogs.ListUnloadingPointsAsync(query, ct)),
                "storage-areas" => Ok(await _catalogs.ListStorageAreasAsync(query, ct)),
                "carriers" => Ok(await _catalogs.ListCarriersAsync(query, ct)),
                "vehicle-types" => Ok(await _catalogs.ListVehicleTypesAsync(query, ct)),
                "vehicles" => Ok(await _catalogs.ListVehiclesAsync(query, ct)),
                "transport-routes" => Ok(await _catalogs.ListTransportRoutesAsync(query, ct)),
                "construction-takts" => Ok(await _catalogs.ListConstructionTaktsAsync(query, ct)),
                "products" => Ok(await _catalogs.ListProductsAsync(query, ct)),
                _ => NotFound(),
            };
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
    }

    [HttpPost("{entity}")]
    public async Task<IActionResult> Create(string entity, CancellationToken ct)
    {
        try
        {
            return entity.ToLowerInvariant() switch
            {
                "product-types" => Ok(
                    await _catalogs.CreateProductTypeAsync(
                        await ReadBodyAsync<ProductTypeWriteDto>(),
                        ct
                    )
                ),
                "plants" => Ok(
                    await _catalogs.CreatePlantAsync(await ReadBodyAsync<PlantWriteDto>(), ct)
                ),
                "production-lines" => Ok(
                    await _catalogs.CreateProductionLineAsync(
                        await ReadBodyAsync<ProductionLineWriteDto>(),
                        ct
                    )
                ),
                "line-capabilities" => Ok(
                    await _catalogs.CreateLineCapabilityAsync(
                        await ReadBodyAsync<LineCapabilityWriteDto>(),
                        ct
                    )
                ),
                "construction-objects" => Ok(
                    await _catalogs.CreateConstructionObjectAsync(
                        await ReadBodyAsync<ConstructionObjectWriteDto>(),
                        ct
                    )
                ),
                "building-sections" => Ok(
                    await _catalogs.CreateBuildingSectionAsync(
                        await ReadBodyAsync<BuildingSectionWriteDto>(),
                        ct
                    )
                ),
                "floors" => Ok(
                    await _catalogs.CreateFloorAsync(await ReadBodyAsync<FloorWriteDto>(), ct)
                ),
                "unloading-points" => Ok(
                    await _catalogs.CreateUnloadingPointAsync(
                        await ReadBodyAsync<UnloadingPointWriteDto>(),
                        ct
                    )
                ),
                "storage-areas" => Ok(
                    await _catalogs.CreateStorageAreaAsync(
                        await ReadBodyAsync<StorageAreaWriteDto>(),
                        ct
                    )
                ),
                "carriers" => Ok(
                    await _catalogs.CreateCarrierAsync(await ReadBodyAsync<CarrierWriteDto>(), ct)
                ),
                "vehicle-types" => Ok(
                    await _catalogs.CreateVehicleTypeAsync(
                        await ReadBodyAsync<VehicleTypeWriteDto>(),
                        ct
                    )
                ),
                "vehicles" => Ok(
                    await _catalogs.CreateVehicleAsync(await ReadBodyAsync<VehicleWriteDto>(), ct)
                ),
                "transport-routes" => Ok(
                    await _catalogs.CreateTransportRouteAsync(
                        await ReadBodyAsync<TransportRouteWriteDto>(),
                        ct
                    )
                ),
                "construction-takts" => Ok(
                    await _catalogs.CreateConstructionTaktAsync(
                        await ReadBodyAsync<ConstructionTaktWriteDto>(),
                        ct
                    )
                ),
                "products" => Ok(
                    await _catalogs.CreateProductAsync(await ReadBodyAsync<ProductWriteDto>(), ct)
                ),
                _ => NotFound(),
            };
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{entity}/{id:long}")]
    public async Task<IActionResult> Update(string entity, long id, CancellationToken ct)
    {
        try
        {
            return entity.ToLowerInvariant() switch
            {
                "product-types" => Ok(
                    await _catalogs.UpdateProductTypeAsync(
                        (int)id,
                        await ReadBodyAsync<ProductTypeWriteDto>(),
                        ct
                    )
                ),
                "plants" => Ok(
                    await _catalogs.UpdatePlantAsync(
                        (int)id,
                        await ReadBodyAsync<PlantWriteDto>(),
                        ct
                    )
                ),
                "production-lines" => Ok(
                    await _catalogs.UpdateProductionLineAsync(
                        (int)id,
                        await ReadBodyAsync<ProductionLineWriteDto>(),
                        ct
                    )
                ),
                "line-capabilities" => Ok(
                    await _catalogs.UpdateLineCapabilityAsync(
                        (int)id,
                        await ReadBodyAsync<LineCapabilityWriteDto>(),
                        ct
                    )
                ),
                "construction-objects" => Ok(
                    await _catalogs.UpdateConstructionObjectAsync(
                        (int)id,
                        await ReadBodyAsync<ConstructionObjectWriteDto>(),
                        ct
                    )
                ),
                "building-sections" => Ok(
                    await _catalogs.UpdateBuildingSectionAsync(
                        (int)id,
                        await ReadBodyAsync<BuildingSectionWriteDto>(),
                        ct
                    )
                ),
                "floors" => Ok(
                    await _catalogs.UpdateFloorAsync(
                        (int)id,
                        await ReadBodyAsync<FloorWriteDto>(),
                        ct
                    )
                ),
                "unloading-points" => Ok(
                    await _catalogs.UpdateUnloadingPointAsync(
                        (int)id,
                        await ReadBodyAsync<UnloadingPointWriteDto>(),
                        ct
                    )
                ),
                "storage-areas" => Ok(
                    await _catalogs.UpdateStorageAreaAsync(
                        (int)id,
                        await ReadBodyAsync<StorageAreaWriteDto>(),
                        ct
                    )
                ),
                "carriers" => Ok(
                    await _catalogs.UpdateCarrierAsync(
                        (int)id,
                        await ReadBodyAsync<CarrierWriteDto>(),
                        ct
                    )
                ),
                "vehicle-types" => Ok(
                    await _catalogs.UpdateVehicleTypeAsync(
                        (int)id,
                        await ReadBodyAsync<VehicleTypeWriteDto>(),
                        ct
                    )
                ),
                "vehicles" => Ok(
                    await _catalogs.UpdateVehicleAsync(
                        id,
                        await ReadBodyAsync<VehicleWriteDto>(),
                        ct
                    )
                ),
                "transport-routes" => Ok(
                    await _catalogs.UpdateTransportRouteAsync(
                        (int)id,
                        await ReadBodyAsync<TransportRouteWriteDto>(),
                        ct
                    )
                ),
                "construction-takts" => Ok(
                    await _catalogs.UpdateConstructionTaktAsync(
                        id,
                        await ReadBodyAsync<ConstructionTaktWriteDto>(),
                        ct
                    )
                ),
                "products" => Ok(
                    await _catalogs.UpdateProductAsync(
                        id,
                        await ReadBodyAsync<ProductWriteDto>(),
                        ct
                    )
                ),
                _ => NotFound(),
            };
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{entity}/{id:long}")]
    public async Task<IActionResult> Deactivate(string entity, long id, CancellationToken ct)
    {
        var key = entity.ToLowerInvariant();
        if (key is "building-sections" or "floors")
            return NotFound();

        try
        {
            switch (key)
            {
                case "product-types":
                    await _catalogs.DeactivateProductTypeAsync((int)id, ct);
                    break;
                case "plants":
                    await _catalogs.DeactivatePlantAsync((int)id, ct);
                    break;
                case "production-lines":
                    await _catalogs.DeactivateProductionLineAsync((int)id, ct);
                    break;
                case "line-capabilities":
                    await _catalogs.DeactivateLineCapabilityAsync((int)id, ct);
                    break;
                case "construction-objects":
                    await _catalogs.DeactivateConstructionObjectAsync((int)id, ct);
                    break;
                case "unloading-points":
                    await _catalogs.DeactivateUnloadingPointAsync((int)id, ct);
                    break;
                case "storage-areas":
                    await _catalogs.DeactivateStorageAreaAsync((int)id, ct);
                    break;
                case "carriers":
                    await _catalogs.DeactivateCarrierAsync((int)id, ct);
                    break;
                case "vehicles":
                    await _catalogs.DeactivateVehicleAsync(id, ct);
                    break;
                case "transport-routes":
                    await _catalogs.DeactivateTransportRouteAsync((int)id, ct);
                    break;
                case "vehicle-types":
                case "construction-takts":
                case "products":
                    return NotFound();
                default:
                    return NotFound();
            }

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{entity}/{id:long}/activate")]
    public async Task<IActionResult> Activate(string entity, long id, CancellationToken ct)
    {
        var key = entity.ToLowerInvariant();
        if (key is "building-sections" or "floors")
            return NotFound();

        try
        {
            switch (key)
            {
                case "product-types":
                    await _catalogs.ActivateProductTypeAsync((int)id, ct);
                    break;
                case "plants":
                    await _catalogs.ActivatePlantAsync((int)id, ct);
                    break;
                case "production-lines":
                    await _catalogs.ActivateProductionLineAsync((int)id, ct);
                    break;
                case "line-capabilities":
                    await _catalogs.ActivateLineCapabilityAsync((int)id, ct);
                    break;
                case "construction-objects":
                    await _catalogs.ActivateConstructionObjectAsync((int)id, ct);
                    break;
                case "unloading-points":
                    await _catalogs.ActivateUnloadingPointAsync((int)id, ct);
                    break;
                case "storage-areas":
                    await _catalogs.ActivateStorageAreaAsync((int)id, ct);
                    break;
                case "carriers":
                    await _catalogs.ActivateCarrierAsync((int)id, ct);
                    break;
                case "vehicles":
                    await _catalogs.ActivateVehicleAsync(id, ct);
                    break;
                case "transport-routes":
                    await _catalogs.ActivateTransportRouteAsync((int)id, ct);
                    break;
                case "vehicle-types":
                case "construction-takts":
                case "products":
                    return NotFound();
                default:
                    return NotFound();
            }

            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<T> ReadBodyAsync<T>()
    {
        var body = await HttpContext.Request.ReadFromJsonAsync<T>();
        if (body is null)
            throw new ArgumentException("Request body is required.");
        return body;
    }
}

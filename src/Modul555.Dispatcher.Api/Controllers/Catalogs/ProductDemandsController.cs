using DispatcherApp.Application.Catalogs;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[ApiController, Authorize, Route("api/products/{productId:long}/demands")]
public sealed class ProductDemandsController(ProductOperationsService operations) : ControllerBase
{
    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDemandDto>>> List(long productId, CancellationToken ct)
        => Ok(await operations.DemandsAsync(productId, ct));

    [HttpGet("takts")]
    public async Task<ActionResult<IReadOnlyList<ProductDemandTaktOption>>> Takts(long productId, CancellationToken ct)
        => Ok(await operations.TaktsAsync(productId, ct));

    [HttpPost]
    public async Task<IActionResult> Create(long productId, ProductDemandWriteDto dto, CancellationToken ct)
    {
        await operations.SaveDemandAsync(productId, null, dto, UserId, ct);
        return NoContent();
    }

    [HttpPut("{demandId:long}")]
    public async Task<IActionResult> Revise(long productId, long demandId, ProductDemandWriteDto dto, CancellationToken ct)
    {
        await operations.SaveDemandAsync(productId, demandId, dto, UserId, ct);
        return NoContent();
    }

    [HttpPost("{demandId:long}/cancel")]
    public async Task<IActionResult> Cancel(long productId, long demandId, ProductDemandCancelDto dto, CancellationToken ct)
    {
        await operations.CancelDemandAsync(productId, demandId, dto, UserId, ct);
        return NoContent();
    }
}

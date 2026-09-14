using DispatcherApp.Application.Catalogs;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers.Catalogs;

[ApiController, Authorize, Route("api/products/{productId:long}/identifiers")]
public sealed class ProductIdentifiersController(ProductOperationsService operations) : ControllerBase
{
    private string? UserId => User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;

    [HttpGet("types")]
    public ActionResult<string[]> Types() => Ok(Enum.GetNames<ProductIdentifierType>());

    [HttpPost]
    public async Task<IActionResult> Create(long productId, ProductIdentifierWriteDto dto, CancellationToken ct)
    {
        await operations.SaveIdentifierAsync(productId, null, dto, UserId, ct);
        return NoContent();
    }

    [HttpPut("{identifierId:long}")]
    public async Task<IActionResult> Replace(long productId, long identifierId, ProductIdentifierWriteDto dto, CancellationToken ct)
    {
        await operations.SaveIdentifierAsync(productId, identifierId, dto, UserId, ct);
        return NoContent();
    }

    [HttpPost("{identifierId:long}/revoke")]
    public async Task<IActionResult> Revoke(long productId, long identifierId, ProductOperationDto dto, CancellationToken ct)
    {
        await operations.RevokeIdentifierAsync(productId, identifierId, dto, UserId, ct);
        return NoContent();
    }
}

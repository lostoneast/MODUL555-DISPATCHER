using DispatcherApp.Application.Common;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

[ApiController]
[Authorize(Roles = "admin,manager")]
[Route("api/admin")]
public sealed class AdminController(AdminService service) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<AdminOverviewDto>> Overview(CancellationToken ct)
        => Ok(await service.OverviewAsync(ct));

    [HttpGet("roles")]
    public ActionResult<IReadOnlyList<RoleOptionDto>> Roles() => Ok(service.Roles());

    [HttpGet("audit")]
    public async Task<ActionResult<PagedResult<AuditEventDto>>> Audit([FromQuery] PagedQuery query, CancellationToken ct)
        => Ok(await service.AuditAsync(query, ct));
}

using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

[ApiController]
[Authorize(Roles = "admin,manager")]
[Route("api/admin/demo")]
public sealed class DemoDataController(DemoDataService service) : ControllerBase
{
    [HttpPost("clear")]
    public async Task<ActionResult<DemoDataService.DemoDataResult>> Clear(CancellationToken ct)
        => Ok(await service.ClearAsync(ct));

    [HttpPost("seed")]
    public async Task<ActionResult<DemoDataService.DemoDataResult>> Seed(CancellationToken ct)
        => Ok(await service.SeedAsync(ct));
}

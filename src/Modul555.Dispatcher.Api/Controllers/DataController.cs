using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DispatcherApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult Public()
    {
        return Ok(new
        {
            message = "API работает",
            authorization = "not required",
            time = DateTime.UtcNow
        });
    }

    [HttpGet("private")]
    [Authorize]
    public IActionResult Private()
    {
        var data = new[]
        {
            new
            {
                id = 1,
                name = "Изделие №1",
                status = "На складе"
            },
            new
            {
                id = 2,
                name = "Изделие №2",
                status = "В производстве"
            },
            new
            {
                id = 3,
                name = "Изделие №3",
                status = "Доставлено"
            }
        };

        return Ok(new
        {
            user = User.Identity?.Name,
            authenticated = User.Identity?.IsAuthenticated,
            roles = User.Claims
                .Where(x => x.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(x => x.Value)
                .Distinct(),

            data
        });
    }
}
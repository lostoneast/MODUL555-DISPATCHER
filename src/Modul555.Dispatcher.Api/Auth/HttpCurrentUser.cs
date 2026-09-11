using System.Security.Claims;
using DispatcherApp.Application.Auth;

namespace DispatcherApp.Auth;

public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpCurrentUser(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? UserId =>
        _accessor.HttpContext?.User.FindFirstValue("sub")
        ?? _accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public string? UserName =>
        _accessor.HttpContext?.User.FindFirstValue("preferred_username")
        ?? _accessor.HttpContext?.User.FindFirstValue("username")
        ?? _accessor.HttpContext?.User.Identity?.Name;

    public IReadOnlyList<string> Roles =>
        _accessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray()
        ?? [];
}

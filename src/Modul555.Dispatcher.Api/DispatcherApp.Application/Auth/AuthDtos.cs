namespace DispatcherApp.Application.Auth;

public sealed class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginResponse
{
    public required string Token { get; init; }
    public string? RefreshToken { get; init; }
    public required UserProfileDto User { get; init; }
}

public sealed class RefreshRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public sealed class RefreshResponse
{
    public required string Token { get; init; }
    public string? RefreshToken { get; init; }
}

public sealed class UserProfileDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string FullName { get; init; }
    public required string Position { get; init; }
    public required IReadOnlyList<string> Roles { get; init; }
}

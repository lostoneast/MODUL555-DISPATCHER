namespace DispatcherApp.Application.Auth;

public interface ICurrentUser
{
    string? UserId { get; }

    string? UserName { get; }

    IReadOnlyList<string> Roles { get; }
}

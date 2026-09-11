namespace DispatcherApp.Domain.Common;

public static class Roles
{
    public const string Admin = "admin";
    public const string Manager = "manager";
    public const string Tester = "tester";
    public const string Developer = "developer";

    public static readonly string[] Keycloak =
    [
        Admin,
        Manager,
        Tester,
        Developer,
    ];

    public static readonly string[] All = [.. Keycloak];

    public static readonly string[] Administrators = [Admin, Manager];
}

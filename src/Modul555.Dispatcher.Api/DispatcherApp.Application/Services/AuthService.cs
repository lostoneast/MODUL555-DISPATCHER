using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DispatcherApp.Application.Auth;
using DispatcherApp.Domain.Common;

namespace DispatcherApp.Application.Services;

public sealed class AuthService
{
    private readonly ICurrentUser _user;
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;

    public AuthService(
        ICurrentUser user,
        IHttpClientFactory httpFactory,
        IConfiguration configuration
    )
    {
        _user = user;
        _httpFactory = httpFactory;
        _configuration = configuration;
    }

    public async Task<LoginResponse?> LoginViaKeycloakAsync(
        LoginRequest request,
        CancellationToken ct
    )
    {
        if (
            string.IsNullOrWhiteSpace(request.UserName)
            || string.IsNullOrWhiteSpace(request.Password)
        )
            return null;

        var tokens = await RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["username"] = request.UserName.Trim(),
                ["password"] = request.Password,
                ["scope"] = "openid profile email",
            },
            ct
        );
        if (tokens is null)
            return null;

        var identity = await ResolveIdentityAsync(tokens, ct);
        if (identity is null)
            return null;

        return new LoginResponse
        {
            Token = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken,
            User = BuildProfile(identity),
        };
    }

    public async Task<RefreshResponse?> RefreshAsync(string refreshToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var tokens = await RequestTokenAsync(
            new Dictionary<string, string>
            {
                ["grant_type"] = "refresh_token",
                ["refresh_token"] = refreshToken,
            },
            ct
        );
        if (tokens is null)
            return null;

        return new RefreshResponse
        {
            Token = tokens.AccessToken,
            RefreshToken = tokens.RefreshToken ?? refreshToken,
        };
    }

    public Task<UserProfileDto?> GetProfileAsync(CancellationToken ct)
    {
        _ = ct;
        var userName = _user.UserName?.Trim();
        if (string.IsNullOrWhiteSpace(userName))
            return Task.FromResult<UserProfileDto?>(null);

        var tokenRoles = _user.Roles.Count > 0 ? _user.Roles.ToArray() : new[] { userName };
        var identity = new ResolvedIdentity(
            _user.UserId ?? userName,
            userName,
            tokenRoles
        );
        return Task.FromResult<UserProfileDto?>(BuildProfile(identity));
    }

    private static UserProfileDto BuildProfile(ResolvedIdentity identity)
    {
        var meta = KeycloakUserMeta(identity.UserName);
        var roles = FilterRoles(identity.Roles);

        return new UserProfileDto
        {
            Id = identity.UserId,
            UserName = identity.UserName,
            FullName = meta.FullName,
            Position = meta.Position,
            Roles = roles,
        };
    }

    private async Task<KeycloakTokenResponse?> RequestTokenAsync(
        Dictionary<string, string> form,
        CancellationToken ct
    )
    {
        var authority =
            _configuration["Keycloak:Authority"]
            ?? "http://72.56.252.95:8080/realms/stroy-company";
        var clientId = _configuration["Keycloak:TokenClientId"] ?? "admin-cli";
        form["client_id"] = clientId;

        var http = _httpFactory.CreateClient("keycloak");
        using var content = new FormUrlEncodedContent(form);
        using var resp = await http.PostAsync(
            $"{authority.TrimEnd('/')}/protocol/openid-connect/token",
            content,
            ct
        );
        if (!resp.IsSuccessStatusCode)
            return null;

        return await resp.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
            cancellationToken: ct
        );
    }

    private async Task<ResolvedIdentity?> ResolveIdentityAsync(
        KeycloakTokenResponse tokens,
        CancellationToken ct
    )
    {
        var userId = TryClaim(tokens.IdToken, "sub") ?? TryClaim(tokens.AccessToken, "sub");
        var userName =
            TryPreferredUsername(tokens.IdToken) ?? TryPreferredUsername(tokens.AccessToken);
        var roles = ExtractRoles(tokens.AccessToken);

        if (string.IsNullOrWhiteSpace(userName))
        {
            var authority =
                _configuration["Keycloak:Authority"]
                ?? "http://72.56.252.95:8080/realms/stroy-company";
            var http = _httpFactory.CreateClient("keycloak");
            using var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"{authority.TrimEnd('/')}/protocol/openid-connect/userinfo"
            );
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
            using var resp = await http.SendAsync(req, ct);
            if (resp.IsSuccessStatusCode)
            {
                await using var stream = await resp.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                if (
                    doc.RootElement.TryGetProperty("preferred_username", out var un)
                    && un.GetString() is { Length: > 0 } name
                )
                    userName = name;

                if (
                    string.IsNullOrWhiteSpace(userId)
                    && doc.RootElement.TryGetProperty("sub", out var sub)
                    && sub.GetString() is { Length: > 0 } id
                )
                    userId = id;
            }
        }

        if (string.IsNullOrWhiteSpace(userName))
            return null;

        if (roles.Count == 0)
            roles = [userName];

        return new ResolvedIdentity(userId ?? userName, userName, roles);
    }

    private static string? TryPreferredUsername(string? jwt) => TryClaim(jwt, "preferred_username");

    private static string? TryClaim(string? jwt, string claim)
    {
        var payload = ReadJwtPayload(jwt);
        return payload?.TryGetProperty(claim, out var u) == true ? u.GetString() : null;
    }

    private static List<string> ExtractRoles(string? jwt)
    {
        var roles = new List<string>();
        var payload = ReadJwtPayload(jwt);
        if (payload is null)
            return roles;

        if (
            payload.Value.TryGetProperty("realm_access", out var realm)
            && realm.TryGetProperty("roles", out var arr)
        )
        {
            foreach (var r in arr.EnumerateArray())
                if (r.GetString() is { Length: > 0 } name)
                    roles.Add(name);
        }

        return roles;
    }

    private static JsonElement? ReadJwtPayload(string? jwt)
    {
        if (string.IsNullOrWhiteSpace(jwt))
            return null;
        var parts = jwt.Split('.');
        if (parts.Length < 2)
            return null;
        try
        {
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2:
                    payload += "==";
                    break;
                case 3:
                    payload += "=";
                    break;
            }
            var bytes = Convert.FromBase64String(payload);
            using var doc = JsonDocument.Parse(bytes);
            return doc.RootElement.Clone();
        }
        catch
        {
            return null;
        }
    }

    private static IReadOnlyList<string> FilterRoles(IEnumerable<string> fromToken)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in fromToken)
            if (!string.IsNullOrWhiteSpace(r) && Roles.All.Contains(r))
                set.Add(r);

        return set.ToArray();
    }

    private static (string FullName, string Position) KeycloakUserMeta(string userName) =>
        userName.ToLowerInvariant() switch
        {
            "admin" => ("Администратор системы", "Администратор"),
            "manager" => ("Менеджер", "Менеджер"),
            "tester" => ("Тестировщик", "Тестировщик"),
            "developer" => ("Разработчик", "Разработчик"),
            _ => (userName, "Пользователь Keycloak"),
        };

    private sealed record ResolvedIdentity(
        string UserId,
        string UserName,
        IReadOnlyList<string> Roles
    );

    private sealed class KeycloakTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("id_token")]
        public string? IdToken { get; set; }
    }
}

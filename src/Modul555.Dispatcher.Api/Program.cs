using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using DispatcherApp.Application.Auth;
using DispatcherApp.Application.Services;
using DispatcherApp.Auth;
using DispatcherApp.Infrastructure.Persistence;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connection =
    builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=modul555_dispatcher;Username=dispatcher;Password=dispatcher";

var keycloakAuthority =
    builder.Configuration["Keycloak:Authority"]
    ?? "http://72.56.252.95:8080/realms/stroy-company";
var keycloakAudience = builder.Configuration["Keycloak:Audience"];
var keycloakUserInfo = $"{keycloakAuthority.TrimEnd('/')}/protocol/openid-connect/userinfo";

builder.Services.AddDbContext<DispatcherDbContext>(o => o.UseNpgsql(connection));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("keycloak");
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<CatalogService>();

builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.Authority = keycloakAuthority;
        o.RequireHttpsMetadata = false;
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = !string.IsNullOrWhiteSpace(keycloakAudience),
            ValidAudience = keycloakAudience,
            ValidateLifetime = true,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role,
        };
        o.MapInboundClaims = false;
        o.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                if (context.Principal?.Identity is not ClaimsIdentity identity)
                    return;

                void AddRoles(IEnumerable<string?> roles)
                {
                    foreach (var name in roles)
                    {
                        if (
                            !string.IsNullOrWhiteSpace(name)
                            && !identity.HasClaim(ClaimTypes.Role, name)
                        )
                            identity.AddClaim(new Claim(ClaimTypes.Role, name!));
                    }
                }

                if (context.SecurityToken is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwt)
                {
                    if (jwt.Payload.TryGetValue("realm_access", out var realmObj))
                    {
                        if (
                            realmObj is JsonElement realmEl
                            && realmEl.TryGetProperty("roles", out var realmRoles)
                        )
                            AddRoles(realmRoles.EnumerateArray().Select(r => r.GetString()));
                        else if (
                            realmObj is Dictionary<string, object> realmDict
                            && realmDict.TryGetValue("roles", out var rolesObj)
                            && rolesObj is object[] roleArr
                        )
                            AddRoles(roleArr.Select(r => r?.ToString()));
                    }

                    if (
                        jwt.Payload.TryGetValue("resource_access", out var resObj)
                        && resObj is JsonElement resEl
                        && !string.IsNullOrWhiteSpace(keycloakAudience)
                        && resEl.TryGetProperty(keycloakAudience, out var client)
                        && client.TryGetProperty("roles", out var clientRoles)
                    )
                        AddRoles(clientRoles.EnumerateArray().Select(r => r.GetString()));
                }

                if (string.IsNullOrWhiteSpace(identity.FindFirst("preferred_username")?.Value))
                {
                    var accessToken = context.Request.Headers.Authorization.ToString();
                    if (
                        accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                    )
                    {
                        accessToken = accessToken["Bearer ".Length..].Trim();
                        try
                        {
                            var factory =
                                context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
                            var http = factory.CreateClient("keycloak");
                            using var req = new HttpRequestMessage(HttpMethod.Get, keycloakUserInfo);
                            req.Headers.Authorization = new AuthenticationHeaderValue(
                                "Bearer",
                                accessToken
                            );
                            using var resp = await http.SendAsync(req);
                            if (resp.IsSuccessStatusCode)
                            {
                                await using var stream = await resp.Content.ReadAsStreamAsync();
                                using var doc = await JsonDocument.ParseAsync(stream);
                                var root = doc.RootElement;
                                if (
                                    root.TryGetProperty("preferred_username", out var un)
                                    && un.GetString() is { Length: > 0 } userName
                                    && !identity.HasClaim("preferred_username", userName)
                                )
                                {
                                    identity.AddClaim(new Claim("preferred_username", userName));
                                    identity.AddClaim(new Claim(ClaimTypes.Name, userName));
                                }

                                if (
                                    root.TryGetProperty("realm_access", out var ra)
                                    && ra.TryGetProperty("roles", out var roles)
                                )
                                    AddRoles(roles.EnumerateArray().Select(r => r.GetString()));
                            }
                        }
                        catch
                        {
                            /* userinfo optional enrichment */
                        }
                    }
                }

                if (!identity.Claims.Any(c => c.Type == ClaimTypes.Role))
                {
                    var username =
                        identity.FindFirst("preferred_username")?.Value
                        ?? identity.Name;
                    if (!string.IsNullOrWhiteSpace(username))
                        identity.AddClaim(new Claim(ClaimTypes.Role, username));
                }
            },
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        )
    );
builder.Services.AddOpenApi();
builder.Services.AddCors(o =>
    o.AddDefaultPolicy(p => p.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin())
);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DispatcherDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

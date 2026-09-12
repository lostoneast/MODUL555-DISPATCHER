using System.ComponentModel.DataAnnotations;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Controllers;
using DispatcherApp.Infrastructure.Persistence;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

// Real MVC discovery, without running application startup, migrations or database writes.
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddApplicationPart(typeof(AuthController).Assembly);
await using var app = builder.Build();
var actions = app.Services.GetRequiredService<IActionDescriptorCollectionProvider>()
    .ActionDescriptors.Items.OfType<ControllerActionDescriptor>().ToArray();
var routes = actions.SelectMany(a => a.ActionConstraints!.OfType<HttpMethodActionConstraint>()
    .SelectMany(c => c.HttpMethods.Select(m => (Method: m, Route: a.AttributeRouteInfo!.Template!, Action: a))))
    .ToArray();
void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}
Check(!routes.GroupBy(r => (r.Method, r.Route)).Any(g => g.Count() > 1), "Duplicate routes");
Check(!routes.Any(r => r.Route.StartsWith("api/{entity}")), "Generic CRUD route still exists");
string[] resources = ["product-types", "plants", "production-lines", "line-capabilities",
    "construction-objects", "building-sections", "floors", "unloading-points", "storage-areas",
    "carriers", "vehicle-types", "vehicles", "transport-routes", "construction-takts", "products"];
string[] withoutActive = ["building-sections", "floors", "vehicle-types", "construction-takts", "products"];
using var db = new DispatcherDbContext(new DbContextOptionsBuilder<DispatcherDbContext>()
    .UseNpgsql("Host=localhost;Database=contract_checks;Username=unused").Options);
foreach (var resource in resources)
{
    foreach (var (method, suffix) in new[] { ("GET", ""), ("POST", ""), ("GET", "/lookups"), ("GET", "/export"), ("POST", "/import") })
        Check(routes.Count(r => r.Method == method && r.Route == $"api/{resource}{suffix}") == 1, $"Missing {method} {resource}{suffix}");
    var update = routes.Single(r => r.Method == "PUT" && r.Route.StartsWith($"api/{resource}/"));
    var dtoType = update.Action.MethodInfo.GetParameters().Single(p => p.Name == "dto").ParameterType;
    var entityName = dtoType.Name.Replace("WriteDto", "");
    var entity = db.Model.GetEntityTypes().Single(e => e.ClrType.Name == entityName);
    var keyType = entity.FindPrimaryKey()!.Properties.Single().ClrType;
    Check(update.Action.MethodInfo.GetParameters().Single(p => p.Name == "id").ParameterType == keyType, $"ORM key mismatch: {resource}");
    Check(update.Route.EndsWith(keyType == typeof(int) ? "{id:int}" : "{id:long}"), $"Invalid key constraint: {resource}");
    Check(routes.Any(r => r.Method == "DELETE" && r.Route.StartsWith($"api/{resource}/")) == !withoutActive.Contains(resource), $"Invalid activation surface: {resource}");
    Check(update.Action.MethodInfo.GetParameters().Single(p => p.Name == "dto").GetCustomAttributes(typeof(FromBodyAttribute), true).Length == 1, $"Missing body binding: {resource}");
}
Check(routes.Any(r => r.Method == "GET" && r.Route == "api/construction-objects/{id:int}"), "Missing object detail route");
Check(routes.Any(r => r.Route == "api/lookups/{entity}"), "Missing legacy lookup route");
foreach (var type in typeof(AuthController).Assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(ControllerBase)) && !t.IsAbstract))
    Check(!type.GetConstructors().SelectMany(c => c.GetParameters()).Any(p => p.ParameterType == typeof(DispatcherDbContext)), $"Controller accesses database: {type.Name}");
var emptyPlant = new PlantWriteDto();
Check(!Validator.TryValidateObject(emptyPlant, new ValidationContext(emptyPlant), [], true), "Empty DTO accepted");

var service = new CatalogService(db);
async Task Reject(ConstructionObjectCreateDto dto)
{
    try { await service.CreateConstructionObjectAsync(dto); }
    catch (ArgumentException) { return; }
    throw new Exception("Invalid construction request accepted");
}
await Reject(new() { StartDate = new(2026, 9, 2), EndDate = new(2026, 9, 1) });
await Reject(new() { AutoAddFloors = true, FloorsCount = 2, SectionsCount = 0 });
await Reject(new() { TaktsCount = 1 });
await Reject(new() { SectionsCount = 51 });
Console.WriteLine($"PASS: {routes.Length} MVC routes; 15 ORM key/capability contracts; DTO and construction validation. No database connections or writes.");

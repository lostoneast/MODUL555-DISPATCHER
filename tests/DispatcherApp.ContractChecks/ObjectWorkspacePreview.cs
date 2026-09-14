using System.Security.Claims;
using DispatcherApp.Application.Auth;
using DispatcherApp.Application.Catalogs;
using DispatcherApp.Controllers;
using DispatcherApp.Infrastructure.Persistence;
using DispatcherApp.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Optional manual UI fixture. Binds ONLY to loopback and uses the disposable test DB.
// Never included in the production application's startup.
internal static class ObjectWorkspacePreview
{
    private sealed class PreviewUser : ICurrentUser
    {
        public string UserId => "ui-preview";
        public string UserName => "ui-preview";
        public IReadOnlyList<string> Roles => ["manager"];
    }
    public static async Task RunAsync()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { WebRootPath = Path.GetFullPath("web/dist") });
        builder.Logging.ClearProviders();
        builder.Services.AddDbContext<DispatcherDbContext>(o => o.UseNpgsql("Host=127.0.0.1;Port=55439;Database=object_workspace_tests;Username=workspace_test;Password=workspace_test_only"));
        builder.Services.AddScoped<CatalogService>();
        builder.Services.AddScoped<ProductListService>();
        builder.Services.AddScoped<ProductDetailService>();
        builder.Services.AddScoped<ProductOperationsService>();
        builder.Services.AddScoped<ObjectProductsService>();
        builder.Services.AddScoped<ObjectProductImportService>();
        builder.Services.AddScoped<ConstructionObjectDeletionService>();
        builder.Services.AddScoped<ICurrentUser, PreviewUser>();
        builder.Services.AddAuthorization();
        builder.Services.AddControllers(o => o.Filters.Add<ApiExceptionFilter>()).AddApplicationPart(typeof(AuthController).Assembly)
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
        await using var app = builder.Build();
        int id;
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DispatcherDbContext>();
            var existing = await db.ConstructionObjects.FirstOrDefaultAsync(x => x.Code == "UI-PREVIEW");
            if (existing == null)
            {
                var catalogs = scope.ServiceProvider.GetRequiredService<CatalogService>();
                var obj = await catalogs.CreateConstructionObjectAsync(new ConstructionObjectCreateDto
                { Code = "UI-PREVIEW", Name = "ЖК Северный — тестовый объект", Address = "Тестовый адрес, 12", SectionsCount = 3, FloorsCount = 5, AutoAddFloors = true });
                id = obj.Id;
            }
            else id = existing.Id;
            if (!await db.ProjectPositions.AnyAsync(x => x.ConstructionObjectId == id))
            {
                var products = scope.ServiceProvider.GetRequiredService<ObjectProductsService>();
                var structure = await products.StructureAsync(id, default);
                var typeId = await db.ProductTypes.OrderBy(x => x.Id).Select(x => x.Id).FirstAsync();
                for (var i = 1; i <= 25; i++)
                    await products.CreateAsync(id, new ObjectProductWriteDto
                    {
                        ProductCode = $"UI-{i:D5}", ProductTypeId = typeId, Mark = $"НС-{i}", WidthMm = 3000, HeightMm = 2800, ThicknessMm = 300,
                        WeightKg = 4500, BuildingSectionId = structure.Sections[0].Id, FloorId = structure.Floors[0].Id,
                        InstallationNumber = $"М-{i}", AdditionalInfo = "Наружная стеновая панель. Проверить закладные детали.",
                    }, default);
            }
        }
        app.Use(async (context, next) =>
        {
            context.User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "ui-preview"), new Claim(ClaimTypes.Role, "manager")], "TestFixture"));
            await next(context);
        });
        app.UseAuthorization();
        app.UseStaticFiles();
        app.MapControllers();
        app.MapGet("/preview/start", () => Results.Content($$"""
            <script>localStorage.setItem('dispatcher.token','test-fixture');localStorage.setItem('dispatcher.user',JSON.stringify({id:'test',userName:'test',fullName:'Тестовый пользователь',roles:['manager']}));location.replace('/construction-objects/{{id}}');</script>
            """, "text/html; charset=utf-8"));
        app.MapFallbackToFile("index.html");
        await app.RunAsync("http://127.0.0.1:55123");
    }
}

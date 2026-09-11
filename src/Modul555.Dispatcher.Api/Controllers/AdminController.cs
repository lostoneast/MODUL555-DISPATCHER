using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Controllers;

[ApiController]
[Authorize(Roles = "admin,manager")]
[Route("api/admin")]
public sealed class AdminController : ControllerBase
{
    private readonly DispatcherDbContext _db;

    public AdminController(DispatcherDbContext db) => _db = db;

    [HttpGet("overview")]
    public async Task<ActionResult<AdminOverviewDto>> Overview(CancellationToken ct)
    {
        var dto = new AdminOverviewDto
        {
            Products = await _db.Products.CountAsync(ct),
            ProductTypes = await _db.ProductTypes.CountAsync(x => x.IsActive, ct),
            Plants = await _db.Plants.CountAsync(x => x.IsActive, ct),
            ProductionLines = await _db.ProductionLines.CountAsync(x => x.IsActive, ct),
            ConstructionObjects = await _db.ConstructionObjects.CountAsync(x => x.IsActive, ct),
            ConstructionTakts = await _db.ConstructionTakts.CountAsync(ct),
            Vehicles = await _db.Vehicles.CountAsync(x => x.IsActive, ct),
            StorageAreas = await _db.StorageAreas.CountAsync(x => x.IsActive, ct),
            Trips = await _db.Trips.CountAsync(ct),
            AuditEvents = await _db.AuditEvents.CountAsync(ct),
        };
        return Ok(dto);
    }

    [HttpGet("roles")]
    public ActionResult<IReadOnlyList<RoleOptionDto>> Roles()
    {
        var items = Domain.Common.Roles.All
            .Select(r => new RoleOptionDto
            {
                Value = r,
                Label = r switch
                {
                    Domain.Common.Roles.Admin => "Администратор",
                    Domain.Common.Roles.Manager => "Менеджер",
                    Domain.Common.Roles.Tester => "Тестировщик",
                    Domain.Common.Roles.Developer => "Разработчик",
                    _ => r,
                },
            })
            .ToList();
        return Ok(items);
    }

    [HttpGet("audit")]
    public async Task<ActionResult<PagedResult<AuditEventDto>>> Audit(
        [FromQuery] PagedQuery query,
        CancellationToken ct
    )
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var q = _db.AuditEvents.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(x =>
                (x.UserName != null && x.UserName.Contains(s))
                || x.EntityType.Contains(s)
                || x.EntityId.Contains(s)
                || x.Action.Contains(s)
                || (x.Comment != null && x.Comment.Contains(s))
            );
        }

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(x => x.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new AuditEventDto
            {
                Id = x.Id,
                Timestamp = x.Timestamp,
                UserId = x.UserId,
                UserName = x.UserName,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                Action = x.Action,
                Comment = x.Comment,
                Source = x.Source,
                CorrelationId = x.CorrelationId,
            })
            .ToListAsync(ct);

        return Ok(
            new PagedResult<AuditEventDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize,
            }
        );
    }
}

public sealed class AdminOverviewDto
{
    public int Products { get; set; }
    public int ProductTypes { get; set; }
    public int Plants { get; set; }
    public int ProductionLines { get; set; }
    public int ConstructionObjects { get; set; }
    public int ConstructionTakts { get; set; }
    public int Vehicles { get; set; }
    public int StorageAreas { get; set; }
    public int Trips { get; set; }
    public int AuditEvents { get; set; }
}

public sealed class RoleOptionDto
{
    public required string Value { get; init; }
    public required string Label { get; init; }
}

public sealed class AuditEventDto
{
    public long Id { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public string? UserId { get; set; }
    public string? UserName { get; set; }
    public string EntityType { get; set; } = "";
    public string EntityId { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Comment { get; set; }
    public AuditSource Source { get; set; }
    public string? CorrelationId { get; set; }
}

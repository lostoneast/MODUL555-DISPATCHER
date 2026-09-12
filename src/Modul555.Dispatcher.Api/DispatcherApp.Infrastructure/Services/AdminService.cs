using DispatcherApp.Application.Common;
using DispatcherApp.Domain.Enums;
using DispatcherApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DispatcherApp.Infrastructure.Services;

public sealed class AdminService
{
    private readonly DispatcherDbContext _db;

    public AdminService(DispatcherDbContext db) => _db = db;

    public async Task<AdminOverviewDto> OverviewAsync(CancellationToken ct)
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
        return dto;
    }

    public IReadOnlyList<RoleOptionDto> Roles()
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
        return items;
    }

    public async Task<PagedResult<AuditEventDto>> AuditAsync(
        PagedQuery query,
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

        return new PagedResult<AuditEventDto>
            {
                Items = items,
                Total = total,
                Page = page,
                PageSize = pageSize,
            };
    }
}


using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DispatcherApp.Infrastructure.Persistence;

public sealed class DispatcherDbContextFactory : IDesignTimeDbContextFactory<DispatcherDbContext>
{
    public DispatcherDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=modul555_dispatcher;Username=dispatcher;Password=dispatcher"
            )
            .Options;

        return new DispatcherDbContext(options);
    }
}

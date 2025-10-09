using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheNextLevel.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Use SQLite for design-time operations (migrations)
        optionsBuilder.UseSqlite("Data Source=thenextlevel.db");

        return new AppDbContext(optionsBuilder.Options);
    }
}

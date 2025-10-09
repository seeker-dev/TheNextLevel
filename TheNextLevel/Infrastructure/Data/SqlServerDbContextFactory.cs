using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TheNextLevel.Infrastructure.Data;

public class SqlServerDbContextFactory : IDesignTimeDbContextFactory<SqlServerDbContext>
{
    public SqlServerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SqlServerDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=TheNextLevel;Integrated Security=true;TrustServerCertificate=true;");
        return new SqlServerDbContext(optionsBuilder.Options);
    }
}

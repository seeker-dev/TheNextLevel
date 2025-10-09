using Microsoft.EntityFrameworkCore;

namespace TheNextLevel.Infrastructure.Data;

public class SqlServerDbContext : AppDbContext
{
    public SqlServerDbContext(DbContextOptions<SqlServerDbContext> options) : base(options)
    {
    }
}

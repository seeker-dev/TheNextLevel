using Microsoft.EntityFrameworkCore;

namespace TheNextLevel.Infrastructure.Data;

public class SqliteDbContext : AppDbContext
{
    public SqliteDbContext(DbContextOptions<SqliteDbContext> options) : base(options)
    {
    }
}

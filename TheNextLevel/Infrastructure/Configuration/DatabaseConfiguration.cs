using TheNextLevel.Core.Enums;

namespace TheNextLevel.Infrastructure.Configuration
{
    public class DatabaseConfiguration
    {
        public DatabaseProvider Provider { get; set; } = DatabaseProvider.SQLite;
        public string ConnectionString { get; set; } = string.Empty;
    }
}

using Microsoft.EntityFrameworkCore;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Application.Services
{
    public class SecureConfigurationService : ISecureConfigurationService
    {
        private const string ProviderKey = "DatabaseProvider";
        private const string SqliteConnectionKey = "ConnectionString_SQLite";
        private const string SqlServerConnectionKey = "ConnectionString_SqlServer";

        public async Task<string> GetDatabaseProviderAsync()
        {
            try
            {
                var provider = await SecureStorage.GetAsync(ProviderKey);
                return provider ?? "SQLite"; // Default to SQLite
            }
            catch (Exception)
            {
                return "SQLite";
            }
        }

        public async Task SetDatabaseProviderAsync(string provider)
        {
            await SecureStorage.SetAsync(ProviderKey, provider);
        }

        public async Task<string?> GetConnectionStringAsync(string provider)
        {
            try
            {
                var key = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
                    ? SqlServerConnectionKey
                    : SqliteConnectionKey;

                return await SecureStorage.GetAsync(key);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task SetConnectionStringAsync(string provider, string connectionString)
        {
            var key = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase)
                ? SqlServerConnectionKey
                : SqliteConnectionKey;

            await SecureStorage.SetAsync(key, connectionString);
        }

        public async Task<(bool isValid, string? errorMessage)> TestConnectionAsync(string provider, string connectionString)
        {
            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

                if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder.UseSqlServer(connectionString);
                }
                else
                {
                    optionsBuilder.UseSqlite(connectionString);
                }

                using var context = new AppDbContext(optionsBuilder.Options);

                // Try to open connection and execute a simple query
                await context.Database.CanConnectAsync();

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task ClearAllAsync()
        {
            SecureStorage.Remove(ProviderKey);
            SecureStorage.Remove(SqliteConnectionKey);
            SecureStorage.Remove(SqlServerConnectionKey);

            await Task.CompletedTask;
        }
    }
}

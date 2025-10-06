using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using TheNextLevel.Configuration;

namespace TheNextLevel.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Load configuration from embedded resource
        var configuration = LoadConfiguration();
        var databaseSettings = new DatabaseSettings();
        configuration.GetSection("DatabaseSettings").Bind(databaseSettings);

        // Configure database provider
        if (databaseSettings.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(databaseSettings.ConnectionStrings.SqlServer);
        }
        else
        {
            optionsBuilder.UseSqlite(databaseSettings.ConnectionStrings.SQLite);
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static IConfiguration LoadConfiguration()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "TheNextLevel.appsettings.json";

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");
        }

        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonStream(stream);
        return configBuilder.Build();
    }
}

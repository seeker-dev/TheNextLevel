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

        // Check for --provider argument to override configuration
        var providerOverride = GetProviderFromArgs(args);

        // Load configuration from embedded resource
        var configuration = LoadConfiguration();
        var databaseSettings = new DatabaseSettings();
        configuration.GetSection("DatabaseSettings").Bind(databaseSettings);

        var provider = providerOverride ?? databaseSettings.Provider;

        // Configure database provider with provider-specific migrations
        if (provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            optionsBuilder.UseSqlServer(
                databaseSettings.ConnectionStrings.SqlServer,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "dbo"));
        }
        else
        {
            optionsBuilder.UseSqlite(
                databaseSettings.ConnectionStrings.SQLite);
        }

        return new AppDbContext(optionsBuilder.Options);
    }

    private static string? GetProviderFromArgs(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--provider")
            {
                return args[i + 1];
            }
        }
        return null;
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

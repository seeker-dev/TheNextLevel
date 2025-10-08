using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Configuration;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;
using TheNextLevel.Infrastructure.Repositories.Sqlite;
using TheNextLevel.Infrastructure.Repositories.SqlServer;

namespace TheNextLevel
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Register secure configuration service
            builder.Services.AddSingleton<ISecureConfigurationService, SecureConfigurationService>();

            // Load database settings with migration from appsettings.json to secure storage
            var databaseSettings = LoadDatabaseSettingsAsync().GetAwaiter().GetResult();

            // Register database context based on provider
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                if (databaseSettings.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlServer(databaseSettings.ConnectionStrings.SqlServer);
                }
                else
                {
                    var connectionString = databaseSettings.ConnectionStrings.SQLite;
                    if (!Path.IsPathRooted(connectionString.Replace("Data Source=", "")))
                    {
                        connectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, connectionString.Replace("Data Source=", ""))}";
                    }
                    options.UseSqlite(connectionString);
                }
            });

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();

            // Register repositories based on provider
            if (databaseSettings.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            {
                builder.Services.AddScoped<ITaskRepository, SqlServerTaskRepository>();
                builder.Services.AddScoped<IProjectRepository, SqlServerProjectRepository>();
            }
            else
            {
                builder.Services.AddScoped<ITaskRepository, SqliteTaskRepository>();
                builder.Services.AddScoped<IProjectRepository, SqliteProjectRepository>();
            }

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Build the app and apply migrations
            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }

            return app;
        }

        private static async Task<DatabaseSettings> LoadDatabaseSettingsAsync()
        {
            var configService = new SecureConfigurationService();

            // Try to load from secure storage first
            var provider = await configService.GetDatabaseProviderAsync();
            var connectionString = await configService.GetConnectionStringAsync(provider);

            // If not found in secure storage, fall back to appsettings.json (migration scenario)
            if (string.IsNullOrEmpty(connectionString))
            {
                var configuration = LoadConfiguration();
                var settings = new DatabaseSettings();
                configuration.GetSection("DatabaseSettings").Bind(settings);

                // Migrate to secure storage
                await configService.SetDatabaseProviderAsync(settings.Provider);

                if (settings.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    await configService.SetConnectionStringAsync("SqlServer", settings.ConnectionStrings.SqlServer);
                    connectionString = settings.ConnectionStrings.SqlServer;
                }
                else
                {
                    await configService.SetConnectionStringAsync("SQLite", settings.ConnectionStrings.SQLite);
                    connectionString = settings.ConnectionStrings.SQLite;
                }

                provider = settings.Provider;
            }

            // Return settings loaded from secure storage
            return new DatabaseSettings
            {
                Provider = provider,
                ConnectionStrings = new ConnectionStrings
                {
                    SQLite = provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase) ? connectionString : "Data Source=thenextlevel.db",
                    SqlServer = provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) ? connectionString : ""
                }
            };
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
}
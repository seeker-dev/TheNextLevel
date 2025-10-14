using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Core.Enums;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Configuration;
using TheNextLevel.Infrastructure.Data;
using TheNextLevel.Infrastructure.Repositories;

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

            // Load configuration from appsettings.json
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var databaseMode = config.GetValue<string>("DatabaseMode") ?? "EFCore";

            if (databaseMode == "EFCore")
            {
                var dbConfig = config.GetSection("Database").Get<DatabaseConfiguration>()
                ?? new DatabaseConfiguration
                {
                    Provider = DatabaseProvider.SQLite,
                    ConnectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "thenextlevel.db")}"
                };

                // Register database context based on configuration
                switch (dbConfig.Provider)
                {
                    case DatabaseProvider.SqlServer:
                        builder.Services.AddDbContext<AppDbContext, SqlServerDbContext>(options =>
                            options.UseSqlServer(dbConfig.ConnectionString));
                        break;

                    case DatabaseProvider.SQLite:
                    default:
                        var connString = string.IsNullOrEmpty(dbConfig.ConnectionString)
                            ? $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "thenextlevel.db")}"
                            : dbConfig.ConnectionString;
                        builder.Services.AddDbContext<AppDbContext, SqliteDbContext>(options =>
                            options.UseSqlite(connString));
                        break;
                }
            }
            else if (databaseMode == "Turso")
            {
                // TODO: Implement Turso mode
                throw new NotImplementedException("Turso database mode is not yet implemented.");
            }
            else
            {
                throw new InvalidOperationException($"Unsupported DatabaseMode: {databaseMode}");
            }
            

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();

            // Register repositories
            builder.Services.AddScoped<ITaskRepository, EfCoreTaskRepository>();
            builder.Services.AddScoped<IProjectRepository, EfCoreProjectRepository>();

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
    }
}
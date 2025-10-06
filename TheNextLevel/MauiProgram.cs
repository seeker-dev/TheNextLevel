using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;
using TheNextLevel.Infrastructure.Repositories.Sqlite;

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

            // Register database context
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = $"Data Source={Path.Combine(FileSystem.AppDataDirectory, "thenextlevel.db")}";
                options.UseSqlite(connectionString);
            });

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();

            // Register repositories
            builder.Services.AddScoped<ITaskRepository, SqliteTaskRepository>();
            builder.Services.AddScoped<IProjectRepository, SqliteProjectRepository>();

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
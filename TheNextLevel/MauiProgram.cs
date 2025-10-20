using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Configuration;
using TheNextLevel.Infrastructure.Data;
using TheNextLevel.Infrastructure.Repositories;
using TheNextLevel.Infrastructure.Services;

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

            var tursoConfig = config.GetSection("Turso").Get<TursoConfiguration>()
                ?? throw new InvalidOperationException("Turso configuration is missing in appsettings.json");

            // Validate Turso configuration
            if (string.IsNullOrEmpty(tursoConfig.DatabaseUrl))
                throw new InvalidOperationException("Turso DatabaseUrl is required");

            if (string.IsNullOrEmpty(tursoConfig.AuthToken))
                throw new InvalidOperationException("Turso AuthToken is required");

            // Register Turso client as singleton
            builder.Services.AddSingleton(tursoConfig);
            builder.Services.AddSingleton<TursoClient>();

            // Register account context
            builder.Services.AddSingleton<IAccountContext, AccountContext>();

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();

            // Register repositories based on database mode
            builder.Services.AddScoped<ITaskRepository, TursoTaskRepository>();
            builder.Services.AddScoped<IProjectRepository, TursoProjectRepository>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Build the app
            var app = builder.Build();
            return app;
        }
    }
}
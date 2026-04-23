using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Configuration;
using TheNextLevel.Infrastructure.Data;
using TheNextLevel.Infrastructure.Repositories;
using TheNextLevel.Infrastructure.Services;
using TheNextLevel.Services;

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

            // Load configuration from appsettings.json (embedded resource)
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"{assembly.GetName().Name}.appsettings.json";

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException($"Could not find embedded resource: {resourceName}");

            var config = new ConfigurationBuilder()
                .AddJsonStream(stream)
                .Build();

            var apiBaseUrl = config["ApiBaseUrl"]
                ?? throw new InvalidOperationException("ApiBaseUrl is missing in appsettings.json");

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

            // Register HTTP client and auth service
            builder.Services.AddSingleton(new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
            builder.Services.AddSingleton<IAuthService, AuthService>();

            // Register account context
            builder.Services.AddSingleton<IAccountContext, AccountContext>();

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IProjectService, ProjectService>();
            builder.Services.AddScoped<IMissionService, MissionService>();

            // Register repositories based on database mode
            builder.Services.AddScoped<ITaskRepository, TursoTaskRepository>();
            builder.Services.AddScoped<IProjectRepository, TursoProjectRepository>();
            builder.Services.AddScoped<IMissionRepository, TursoMissionRepository>();

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
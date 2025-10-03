using Microsoft.Extensions.Logging;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Application.Services;
using TheNextLevel.Core.Interfaces;
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

            // Register application services
            builder.Services.AddScoped<ITaskService, TaskService>();

            // Register infrastructure services
            builder.Services.AddSingleton<ITaskRepository, InMemoryTaskRepository>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
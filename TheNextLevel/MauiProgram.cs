using Microsoft.Extensions.Logging;
using TheNextLevel.Application.Tasks.Services;
using TheNextLevel.Application.Common.Services;
using TheNextLevel.Infrastructure.Data;

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
            builder.Services.AddSingleton<IDialogService, DialogService>();
            
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

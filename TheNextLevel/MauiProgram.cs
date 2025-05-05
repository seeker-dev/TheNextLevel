using Microsoft.Extensions.Logging;
using TaskManagement.Data;
using TaskManagement.Data.Sqlite;
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
            builder.Services.AddSingleton<BackgroundService>();
            builder.Services.AddSingleton<FilePickerService>();

            builder.Services.AddSingleton<IDatabaseServiceFactory, SqliteServiceFactory>();
            builder.Services.AddSingleton<IDatabaseService, SqliteService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

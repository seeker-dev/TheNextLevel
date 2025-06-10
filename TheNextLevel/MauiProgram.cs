using Microsoft.Extensions.Logging;
using TaskManagement.Data;
using TaskManagement.Data.Sqlite;
using TaskManagement.Domain.Commands.Task;
using TheNextLevel.Services.v2;

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

            // Utility services
            builder.Services.AddSingleton<IAppBackgroundSelectService, AppBackgroundSelectService>();

            builder.Services.AddSingleton<IDatabaseServiceFactory, SqliteServiceFactory>();
            builder.Services.AddSingleton<IDatabaseService, SqliteService>();

            builder.Services.AddSingleton<IListCommand, ListCommand>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}

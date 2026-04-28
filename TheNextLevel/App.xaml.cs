namespace TheNextLevel
{
    public partial class App : Microsoft.Maui.Controls.Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage()) { Title = "TheNextLevel" };
        }

        protected override void OnStart()
        {
            base.OnStart();
            _ = CheckForCrashLogAsync();
        }

        private static async Task CheckForCrashLogAsync()
        {
            var logPath = Path.Combine(FileSystem.AppDataDirectory, "crash.log");
            if (!File.Exists(logPath))
                return;

            var page = Current?.Windows.FirstOrDefault()?.Page;
            if (page == null)
                return;

            var view = await page.DisplayAlert(
                "Previous Session Crash",
                "The app recorded errors from a previous session. View the log?",
                "View", "Dismiss");

            if (view)
            {
                var log = File.ReadAllText(logPath);
                var preview = log.Length > 1500
                    ? log[..1500] + $"\n\n...truncated. Full log at:\n{logPath}"
                    : log;

                await page.DisplayAlert("Crash Log", preview, "OK");
            }

            // Archive the log so it no longer triggers the alert on next launch.
            // Previous archives are kept in AppDataDirectory as crash-{timestamp}.log.
            var archivePath = Path.Combine(
                FileSystem.AppDataDirectory,
                $"crash-{DateTime.Now:yyyyMMdd-HHmmss}.log");
            File.Move(logPath, archivePath);
        }
    }
}

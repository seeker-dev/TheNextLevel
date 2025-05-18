namespace TheNextLevel.Services.v2
{
    internal class AppBackgroundSelectService : IAppBackgroundSelectService
    {
        private const string BackgroundPrefKey = "background_image";

        public event Action OnBackgroundChanged = delegate { };

        public string CurrentBackground { get; private set; } = Preferences.Get(BackgroundPrefKey, string.Empty);

        public void Load()
        {
            CurrentBackground = Preferences.Get(BackgroundPrefKey, string.Empty);
            OnBackgroundChanged?.Invoke();
        }

        public void Save(string background)
        {
            if (background == null)
            {
                Preferences.Remove(BackgroundPrefKey);
                CurrentBackground = string.Empty;
            }
            else
            {
                Preferences.Set(BackgroundPrefKey, background);
                CurrentBackground = background;
            }

            OnBackgroundChanged?.Invoke();
        }
    }
}

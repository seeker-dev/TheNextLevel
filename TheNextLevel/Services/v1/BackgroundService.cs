using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheNextLevel.Services.v1
{
    public class BackgroundService
    {
        private const string BackgroundPrefKey = "background_image";
        public string CurrentBackground { get; private set; }
        public event Action OnBackgroundChanged;
        public BackgroundService()
        {
            // Load the saved background when the service starts
            LoadSavedBackground();
        }
        private void LoadSavedBackground()
        {
            if (Preferences.ContainsKey(BackgroundPrefKey))
            {
                CurrentBackground = Preferences.Get(BackgroundPrefKey, null);
            }
        }
        public void SetBackground(string imagePath)
        {
            CurrentBackground = imagePath;

            // Save the preference
            if (imagePath == null)
            {
                Preferences.Remove(BackgroundPrefKey);
            }
            else
            {
                Preferences.Set(BackgroundPrefKey, imagePath);
            }

            OnBackgroundChanged?.Invoke();
        }
    }
}

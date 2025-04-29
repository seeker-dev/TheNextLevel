using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheNextLevel.Services
{
    public class BackgroundService
    {
        public string CurrentBackground { get; private set; }
        public event Action OnBackgroundChanged;
        public void SetBackground(string bgPath)
        {
            CurrentBackground = bgPath;
            OnBackgroundChanged?.Invoke();
        }
    }
}

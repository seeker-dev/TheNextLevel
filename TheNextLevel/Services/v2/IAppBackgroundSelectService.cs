namespace TheNextLevel.Services.v2
{
    internal interface IAppBackgroundSelectService
    {
        // Fix: Removed the setter and provided only the event declaration with add and remove accessors handled implicitly by the compiler.  
        event Action OnBackgroundChanged;

        string CurrentBackground { get; }

        void Save(string background);

        void Load();
    }
}

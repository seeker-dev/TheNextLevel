namespace TheNextLevel.ViewModels
{
    public class NLTask
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Normal"; // Default priority
        public bool IsCompleted { get; set; } = false;
    }
}

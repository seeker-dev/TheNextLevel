
namespace TheNextLevel.Components.Shared
{
    public class NLTask
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool Completed { get; set; }
        public string Priority { get; set; } = "normal"; // urgent, important, normal
        public string Category { get; set; } = "project"; // project, personal, learning, etc.
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? DueDate { get; set; }
    }
}


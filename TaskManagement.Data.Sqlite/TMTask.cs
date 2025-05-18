
namespace TaskManagement.Data.Sqlite
{
    public class TMTask : TaskManagement.Data.ITMTask
    {
        public TMTask() { }

        public int Id { get; set; }

        public string Title { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.MinValue;
        public DateTime UpdatedAt { get; set; } = DateTime.MinValue;
        public bool IsCompleted { get; set; }
    }
}

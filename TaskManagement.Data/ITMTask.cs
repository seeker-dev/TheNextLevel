namespace TaskManagement.Data
{
    public interface ITMTask
    {
        int Id { get; }
        string Title { get; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsCompleted { get; set; }
    }
}

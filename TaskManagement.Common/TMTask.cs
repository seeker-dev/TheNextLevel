namespace TaskManagement.Common
{
    public interface TMTask
    {
        string TaskName { get; set; }
        string TaskDescription { get; set; }
        //DateTime DueDate { get; set; }
        bool IsCompleted { get; set; }
    }
}

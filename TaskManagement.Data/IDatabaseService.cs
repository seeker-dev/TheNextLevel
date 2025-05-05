namespace TaskManagement.Data
{
    public interface IDatabaseService
    {
        Task<int> SaveTask(ITMTask task);
        Task<int> DeleteTask(int taskId);
        Task<IEnumerable<ITMTask>> GetAllTasksAsync();
        Task<ITMTask> GetTaskByIdAsync(int taskId);
    }
}

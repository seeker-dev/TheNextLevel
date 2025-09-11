using TheNextLevel.Application.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(Guid id);
    System.Threading.Tasks.Task<Guid> CreateTaskAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<bool> UpdateTaskAsync(Guid id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(Guid id);
    System.Threading.Tasks.Task<bool> CompleteTaskAsync(Guid id);
    System.Threading.Tasks.Task<bool> ReopenTaskAsync(Guid id);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(bool isCompleted);
}
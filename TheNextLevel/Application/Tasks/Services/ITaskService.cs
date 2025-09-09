using TheNextLevel.Application.Tasks.DTOs;
using TheNextLevel.Domain.Tasks.ValueObjects;

namespace TheNextLevel.Application.Tasks.Services;

public interface ITaskService
{
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(TaskId id);
    System.Threading.Tasks.Task<TaskId> CreateTaskAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<bool> UpdateTaskAsync(TaskId id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(TaskId id);
    System.Threading.Tasks.Task<bool> CompleteTaskAsync(TaskId id);
    System.Threading.Tasks.Task<bool> ReopenTaskAsync(TaskId id);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(string status);
}
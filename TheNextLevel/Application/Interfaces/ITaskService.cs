using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync();
    System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id);
    System.Threading.Tasks.Task<int> CreateTaskAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<bool> UpdateTaskAsync(int id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id);
    System.Threading.Tasks.Task<bool> CompleteTaskAsync(int id);
    System.Threading.Tasks.Task<bool> ReopenTaskAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(bool isCompleted);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByProjectAsync(int projectId);
    System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetUngroupedTasksAsync();
    System.Threading.Tasks.Task<bool> AssignTaskToProjectAsync(int taskId, int? projectId);
    System.Threading.Tasks.Task<PagedResult<TaskDto>> GetTasksPagedAsync(int skip, int take, bool isCompleted = false);
}
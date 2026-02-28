using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    System.Threading.Tasks.Task<TaskDto?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<PagedResult<TaskDto>> ListAsync(int skip, int take, bool isCompleted = false);
    System.Threading.Tasks.Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take, bool isCompleted = false);
    System.Threading.Tasks.Task<int> CreateAsync(CreateTaskRequest request);
    System.Threading.Tasks.Task<bool> UpdateAsync(int id, UpdateTaskRequest request);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
    System.Threading.Tasks.Task<bool> CompleteAsync(int id);
    System.Threading.Tasks.Task<bool> ReopenAsync(int id);
    System.Threading.Tasks.Task<bool> AssignAsync(int taskId, int projectId);
    System.Threading.Tasks.Task<int> CreateSubtaskAsync(CreateSubtaskRequest request);
    System.Threading.Tasks.Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take);
}
using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto?> GetByIdAsync(int id);
    Task<PagedResult<TaskDto>> ListAsync(int skip, int take);
    Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take);
    Task<TaskDto> CreateAsync(CreateTaskRequest request);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
    Task<bool> MoveAsync(int taskId, int newProjectId);

    Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take);
    Task<TaskDto> CreateSubtaskAsync(CreateSubtaskRequest request);
    Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description);
    Task<bool> DeleteSubtaskAsync(int id, int parentId);
    Task<int> BulkCompleteSubtasksAsync(int parentId);
}
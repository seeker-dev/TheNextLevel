using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.DTOs.Projections;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto?> GetByIdAsync(int id);
    Task<PagedResult<TaskDto>> ListAsync(int skip, int take);
    Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take);
    Task<PagedResult<TaskSummaryDto>> ListByStatus(TaskState status, int skip, int take);
    Task<TaskDto> CreateAsync(CreateTaskRequest request);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request);
    Task<bool> DeleteAsync(int id);
    Task<bool> SetStatusAsync(int id, TaskState status);
    Task<bool> MoveAsync(int taskId, int newProjectId);

    Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take);
    Task<TaskDto> CreateSubtaskAsync(CreateSubtaskRequest request);
    Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description);
    Task<bool> DeleteSubtaskAsync(int id, int parentId);
}
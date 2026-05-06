using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.DTOs.Projections;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface ITaskService
{
    Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<TaskDto>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take, CancellationToken ct = default);
    Task<PagedResult<TaskSummaryDto>> ListByStatus(TaskState status, int skip, int take, CancellationToken ct = default);
    Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken ct = default);
    Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> SetStatusAsync(int id, TaskState status, CancellationToken ct = default);
    Task<bool> MoveAsync(int taskId, int newProjectId, CancellationToken ct = default);

    Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take, CancellationToken ct = default);
    Task<TaskDto> CreateSubtaskAsync(CreateSubtaskRequest request, CancellationToken ct = default);
    Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description, CancellationToken ct = default);
    Task<bool> DeleteSubtaskAsync(int id, int parentId, CancellationToken ct = default);
}

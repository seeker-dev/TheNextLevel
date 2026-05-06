using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface ITaskRepository
{
    Task<Entities.Task?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Entities.Task>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<PagedResult<Entities.Task>> ListByProjectIdAsync(int projectId, int skip, int take, CancellationToken ct = default);
    Task<PagedResult<Entities.TaskSummaryProjection>> ListByStatus(int status, int skip, int take, CancellationToken ct = default);
    Task<Entities.Task> CreateAsync(int projectId, string name, string description, CancellationToken ct = default);
    Task<bool> UpdateAsync(int id, string name, string description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> SetStatusAsync(int id, int status, CancellationToken ct = default);
    Task<bool> MoveAsync(int taskId, int newProjectId, CancellationToken ct = default);

    Task<PagedResult<Entities.Task>> ListSubtasksByParentIdAsync(int parentId, int skip, int take, CancellationToken ct = default);
    Task<Entities.Task> CreateSubtaskAsync(int parentId, string name, string description, CancellationToken ct = default);
    Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description, CancellationToken ct = default);
    Task<bool> DeleteSubtaskAsync(int id, int parentId, CancellationToken ct = default);
    Task<int> CountSubtasksAsync(int parentId, CancellationToken ct = default);
    Task<int> BulkCompleteSubtasksAsync(int parentId, CancellationToken ct = default);
    Task<bool> MoveSubtaskAsync(int taskId, int newParentId, CancellationToken ct = default);
}

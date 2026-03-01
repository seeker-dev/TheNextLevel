using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface ITaskRepository
{
    Task<Entities.Task?> GetByIdAsync(int id);
    Task<PagedResult<Entities.Task>> ListAsync(int skip, int take);
    Task<PagedResult<Entities.Task>> ListByProjectIdAsync(int projectId, int skip, int take);
    Task<Entities.Task> CreateAsync(int projectId, string name, string description);
    Task<bool> UpdateAsync(int id, string name, string description);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
    Task<bool> MoveAsync(int taskId, int newProjectId);
    
    
    Task<PagedResult<Entities.Task>> ListSubtasksByParentIdAsync(int parentId, int skip, int take);
    Task<Entities.Task> CreateSubtaskAsync(int parentId, string name, string description);
    Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description);
    Task<bool> DeleteSubtaskAsync(int id, int parentId);
    Task<int> CountSubtasksAsync(int parentId);
    Task<int> BulkCompleteSubtasksAsync(int parentId);
    Task<bool> MoveSubtaskAsync(int taskId, int newParentId);
}
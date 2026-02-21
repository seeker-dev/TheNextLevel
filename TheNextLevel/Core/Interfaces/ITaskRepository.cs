using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetAllAsync();
    System.Threading.Tasks.Task<Entities.Task?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Entities.Task> AddAsync(string name, string description, int? projectId = null, int? parentTaskId = null);
    System.Threading.Tasks.Task<bool> UpdateAsync(int id, string name, string description);
    System.Threading.Tasks.Task<bool> CompleteAsync(int id);
    System.Threading.Tasks.Task<bool> ReopenAsync(int id);
    System.Threading.Tasks.Task<bool> AssignToProjectAsync(int id, int? projectId);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetByStatusAsync(bool isCompleted);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByProjectIdAsync(int projectId);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetUngroupedTasksAsync();
    System.Threading.Tasks.Task<PagedResult<Entities.Task>> GetPagedAsync(int skip, int take, bool isCompleted = false);
    System.Threading.Tasks.Task<PagedResult<Entities.Task>> GetPagedByProjectIdAsync(int projectId, int skip, int take, bool isCompleted = false);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByProjectIdsAsync(IEnumerable<int> projectIds);
    System.Threading.Tasks.Task<PagedResult<Entities.Task>> GetSubtasksByParentIdAsync(int parentTaskId, int skip, int take);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetRootTasksAsync();
    System.Threading.Tasks.Task<int> GetSubtaskCountAsync(int parentTaskId);
    System.Threading.Tasks.Task<int> BulkCompleteSubtasksAsync(int parentTaskId);
}
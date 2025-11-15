using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetAllAsync();
    System.Threading.Tasks.Task<Entities.Task?> GetByIdAsync(int id);
    System.Threading.Tasks.Task<Entities.Task> AddAsync(Entities.Task task);
    System.Threading.Tasks.Task<Entities.Task> UpdateAsync(Entities.Task task);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetByStatusAsync(bool isCompleted);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByProjectIdAsync(int projectId);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetUngroupedTasksAsync();
    System.Threading.Tasks.Task<PagedResult<Entities.Task>> GetPagedAsync(int skip, int take, bool isCompleted = false);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByProjectIdsAsync(IEnumerable<int> projectIds);
}
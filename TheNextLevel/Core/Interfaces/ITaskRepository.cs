using TheNextLevel.Core.Entities;

namespace TheNextLevel.Core.Interfaces;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetAllAsync();
    System.Threading.Tasks.Task<Entities.Task?> GetByIdAsync(Guid id);
    System.Threading.Tasks.Task<Entities.Task> AddAsync(Entities.Task task);
    System.Threading.Tasks.Task<Entities.Task> UpdateAsync(Entities.Task task);
    System.Threading.Tasks.Task<bool> DeleteAsync(Guid id);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetByStatusAsync(bool isCompleted);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetTasksByProjectIdAsync(Guid projectId);
    System.Threading.Tasks.Task<IEnumerable<Entities.Task>> GetUngroupedTasksAsync();
}
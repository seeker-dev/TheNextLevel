using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Infrastructure.Repositories;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly List<Core.Entities.Task> _tasks = new();
    
    public System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetAllAsync()
    {
        return System.Threading.Tasks.Task.FromResult(_tasks.AsEnumerable());
    }
    
    public System.Threading.Tasks.Task<Core.Entities.Task?> GetByIdAsync(Guid id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        return System.Threading.Tasks.Task.FromResult(task);
    }
    
    public System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
    {
        _tasks.Add(task);
        return System.Threading.Tasks.Task.FromResult(task);
    }
    
    public System.Threading.Tasks.Task<Core.Entities.Task> UpdateAsync(Core.Entities.Task task)
    {
        var index = _tasks.FindIndex(t => t.Id == task.Id);
        if (index != -1)
        {
            _tasks[index] = task;
        }
        return System.Threading.Tasks.Task.FromResult(task);
    }
    
    public System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task != null)
        {
            _tasks.Remove(task);
            return System.Threading.Tasks.Task.FromResult(true);
        }
        return System.Threading.Tasks.Task.FromResult(false);
    }
    
    public System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetByStatusAsync(bool isCompleted)
    {
        var filteredTasks = _tasks.Where(t => t.IsCompleted == isCompleted);
        return System.Threading.Tasks.Task.FromResult(filteredTasks);
    }

    public System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdAsync(Guid projectId)
    {
        var filteredTasks = _tasks.Where(t => t.ProjectId == projectId);
        return System.Threading.Tasks.Task.FromResult(filteredTasks);
    }

    public System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetUngroupedTasksAsync()
    {
        var ungroupedTasks = _tasks.Where(t => t.ProjectId == null);
        return System.Threading.Tasks.Task.FromResult(ungroupedTasks);
    }
}
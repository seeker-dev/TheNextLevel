using System.Collections.Concurrent;
using TheNextLevel.Domain.Tasks.Entities;
using TheNextLevel.Domain.Tasks.ValueObjects;
using DomainTask = TheNextLevel.Domain.Tasks.Entities.Task;

namespace TheNextLevel.Infrastructure.Data;

public class InMemoryTaskRepository : ITaskRepository
{
    private readonly ConcurrentDictionary<TaskId, DomainTask> _tasks = new();
    
    public System.Threading.Tasks.Task<IEnumerable<DomainTask>> GetAllAsync()
    {
        return System.Threading.Tasks.Task.FromResult(_tasks.Values.AsEnumerable());
    }
    
    public System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(TaskId id)
    {
        _tasks.TryGetValue(id, out var task);
        return System.Threading.Tasks.Task.FromResult(task);
    }
    
    public System.Threading.Tasks.Task<TaskId> AddAsync(DomainTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        
        _tasks.TryAdd(task.Id, task);
        return System.Threading.Tasks.Task.FromResult(task.Id);
    }
    
    public System.Threading.Tasks.Task UpdateAsync(DomainTask task)
    {
        if (task == null) throw new ArgumentNullException(nameof(task));
        
        _tasks.TryRemove(task.Id, out _);
        _tasks.TryAdd(task.Id, task);
        return System.Threading.Tasks.Task.CompletedTask;
    }
    
    public System.Threading.Tasks.Task DeleteAsync(TaskId id)
    {
        _tasks.TryRemove(id, out _);
        return System.Threading.Tasks.Task.CompletedTask;
    }
}
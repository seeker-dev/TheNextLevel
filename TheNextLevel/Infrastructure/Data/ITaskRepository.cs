using TheNextLevel.Domain.Tasks.Entities;
using TheNextLevel.Domain.Tasks.ValueObjects;
using DomainTask = TheNextLevel.Domain.Tasks.Entities.Task;

namespace TheNextLevel.Infrastructure.Data;

public interface ITaskRepository
{
    System.Threading.Tasks.Task<IEnumerable<DomainTask>> GetAllAsync();
    System.Threading.Tasks.Task<DomainTask?> GetByIdAsync(TaskId id);
    System.Threading.Tasks.Task<TaskId> AddAsync(DomainTask task);
    System.Threading.Tasks.Task UpdateAsync(DomainTask task);
    System.Threading.Tasks.Task DeleteAsync(TaskId id);
}
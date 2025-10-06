using Microsoft.EntityFrameworkCore;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories.SqlServer;

public class SqlServerTaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public SqlServerTaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetAllAsync()
    {
        return await _context.Tasks.ToListAsync();
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task?> GetByIdAsync(Guid id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> UpdateAsync(Core.Entities.Task task)
    {
        _context.Entry(task).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return task;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
        {
            return false;
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetByStatusAsync(bool isCompleted)
    {
        return await _context.Tasks
            .Where(t => t.IsCompleted == isCompleted)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdAsync(Guid projectId)
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetUngroupedTasksAsync()
    {
        return await _context.Tasks
            .Where(t => t.ProjectId == null)
            .ToListAsync();
    }
}

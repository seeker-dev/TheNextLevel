using Microsoft.EntityFrameworkCore;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories;

public class EfCoreProjectRepository : IProjectRepository
{
    private readonly AppDbContext _context;

    public EfCoreProjectRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _context.Projects
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Project> AddAsync(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        _context.Entry(project).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            return false;
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }
}

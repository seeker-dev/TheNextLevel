using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Infrastructure.Repositories;

public class InMemoryProjectRepository : IProjectRepository
{
    private readonly List<Core.Entities.Project> _projects = new();
    
    public System.Threading.Tasks.Task<IEnumerable<Core.Entities.Project>> GetAllAsync()
    {
        return System.Threading.Tasks.Task.FromResult(_projects.AsEnumerable());
    }
    
    public System.Threading.Tasks.Task<Core.Entities.Project?> GetByIdAsync(int id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        return System.Threading.Tasks.Task.FromResult(project);
    }

    public System.Threading.Tasks.Task<Core.Entities.Project> AddAsync(Core.Entities.Project project)
    {
        _projects.Add(project);
        return System.Threading.Tasks.Task.FromResult(project);
    }

    public System.Threading.Tasks.Task<Core.Entities.Project> UpdateAsync(Core.Entities.Project project)
    {
        var index = _projects.FindIndex(p => p.Id == project.Id);
        if (index != -1)
        {
            _projects[index] = project;
        }
        return System.Threading.Tasks.Task.FromResult(project);
    }

    public System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var project = _projects.FirstOrDefault(p => p.Id == id);
        if (project != null)
        {
            _projects.Remove(project);
            return System.Threading.Tasks.Task.FromResult(true);
        }
        return System.Threading.Tasks.Task.FromResult(false);
    }
}
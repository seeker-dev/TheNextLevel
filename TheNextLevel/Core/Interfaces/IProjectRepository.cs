using TheNextLevel.Core.Entities;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<IEnumerable<Project>> GetAllAsync(bool includeTasks = false);
    Task<Project?> GetByIdAsync(int id);
    Task<IEnumerable<Project>> GetAsync(int startIndex, int count, bool includeTasks = false);
    Task<int> GetTotalProjectsCountAsync(); 
    Task<Project> AddAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id);
}
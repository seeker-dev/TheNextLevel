using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.Entities;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<int> GetTotalProjectsCountAsync();
    Task<Project> AddAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<Project>> GetPagedAsync(int skip, int take);
}
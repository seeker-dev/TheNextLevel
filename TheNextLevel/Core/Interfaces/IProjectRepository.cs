using TheNextLevel.Core.Entities;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<int> GetTotalProjectsCountAsync(string? filterText = null);
    Task<Project> AddAsync(Project project);
    Task<Project> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<Project>> GetPagedAsync(int skip, int take, string? filterText = null);
}
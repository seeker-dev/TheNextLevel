using TheNextLevel.Core.Entities;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<int> GetTotalProjectsCountAsync(string? filterText = null);
    Task<Project> AddAsync(string title, string description, int missionId);
    Task<Project?> UpdateAsync(int id, string name, string description);
    Task<bool> DeleteAsync(int id);
    Task<PagedResult<Project>> GetPagedAsync(int skip, int take, string? filterText = null);
}
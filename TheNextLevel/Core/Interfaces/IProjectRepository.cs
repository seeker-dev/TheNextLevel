using TheNextLevel.Core.Entities;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id);
    Task<PagedResult<Project>> ListAsync(int skip, int take);
    Task<PagedResult<Mission>> ListByMissionIdAsync(int missionId, int skip, int take);
    Task<Project> CreateAsync(int missionId, string title, string description);
    Task<Project?> UpdateAsync(int id, string name, string description);
    Task<bool> DeleteAsync(int id);
    Task<int> CountAsync(string? filterText = null);
    System.Threading.Tasks.Task MoveAsync(int projectId, int newMissionId);
}
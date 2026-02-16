using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;
public interface IMissionService
{
    Task<MissionDto?> GetByIdAsync(int id);
    Task<PagedResult<MissionDto>> ListAsync(int skip, int take, string? filterText = null);
    Task<MissionDto> CreateAsync(string title, string description);
    Task<MissionDto> UpdateAsync(int id, string title, string description);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
    Task<PagedResult<ProjectDto>> ListProjectsAsync(int id, int skip, int take, string? filterText = null);
    Task<PagedResult<TaskDto>> ListTasksAsync(int id, int skip, int take, string? filterText = null);
    Task AddToMissionAsync(int id, int projectId);
}
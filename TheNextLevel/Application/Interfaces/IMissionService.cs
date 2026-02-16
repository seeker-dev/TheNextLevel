using TheNextLevel.Application.DTOs;

namespace TheNextLevel.Application.Interfaces;
public interface IMissionService
{
    Task<MissionDto> GetByIdAsync(int id);
    Task<IEnumerable<MissionDto>> ListAsync(int skip, int take, string? filterText = null);
    Task<MissionDto> CreateAsync(int id, string title, string description);
    Task<MissionDto> UpdateAsync(int id, string title, string description);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
    Task<IEnumerable<ProjectDto>> ListProjectsAsync(int id, int skip, int take, string? filterText = null);
    Task<IEnumerable<TaskDto>> ListTasksAsync(int id, int skip, int take, string? filterText = null);
    Task<bool> AssignAsync(int id, int projectId);
}
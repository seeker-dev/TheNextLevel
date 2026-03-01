using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<PagedResult<ProjectDto>> ListAsync(int skip, int take);
    Task<PagedResult<ProjectDto>> ListByMissionAsync(int missionId, int skip, int take);
    Task<ProjectDto> CreateAsync(CreateProjectDto request);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto request);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
}
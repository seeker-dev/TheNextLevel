using TheNextLevel.Application.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<ProjectDto> CreateProjectAsync(string name, string description);
    Task<ProjectDto?> UpdateProjectAsync(int id, string name, string description);
    Task<bool> DeleteProjectAsync(int id);
    Task<PagedResult<ProjectDto>> GetProjectsPagedAsync(int skip, int take);
}
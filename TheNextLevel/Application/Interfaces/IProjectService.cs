using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetProjectByIdAsync(int id);
    Task<PagedResult<ProjectDto>> GetProjectsPagedAsync(int skip, int take, string? filterText = null);
    Task<ProjectDto> CreateProjectAsync(string name, string description);
    Task<ProjectDto?> UpdateProjectAsync(int id, string name, string description);
    Task<bool> DeleteProjectAsync(int id);
}
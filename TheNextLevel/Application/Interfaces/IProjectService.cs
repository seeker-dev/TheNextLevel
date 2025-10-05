using TheNextLevel.Application.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<IEnumerable<ProjectDto>> GetAllProjectsAsync();
    Task<ProjectDto?> GetProjectByIdAsync(Guid id);
    Task<ProjectDto> CreateProjectAsync(string name, string description);
    Task<ProjectDto?> UpdateProjectAsync(Guid id, string name, string description);
    Task<bool> DeleteProjectAsync(Guid id);
}
using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Shared.DTOs;

namespace TheNextLevel.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description ?? string.Empty,
            project.Tasks
        );
    }

    public async Task<ProjectDto> CreateProjectAsync(string name, string description)
    {
        var project = new Project(name, description);
        var createdProject = await _projectRepository.AddAsync(project);

        return new ProjectDto(
            createdProject.Id,
            createdProject.Name,
            createdProject.Description ?? string.Empty,
            []
        );
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, string name, string description)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        project.UpdateName(name);
        project.UpdateDescription(description);

        var updatedProject = await _projectRepository.UpdateAsync(project);

        return new ProjectDto(
            updatedProject.Id,
            updatedProject.Name,
            updatedProject.Description ?? string.Empty,
            updatedProject.Tasks
        );
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        return await _projectRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsPagedAsync(int skip, int take)
    {
        var pagedResult = await _projectRepository.GetPagedAsync(skip, take);

        var projectDtos = new List<ProjectDto>();
        foreach (var project in pagedResult.Items)
        {
            projectDtos.Add(new ProjectDto(
                project.Id,
                project.Name,
                project.Description ?? string.Empty,
                project.Tasks
            ));
        }

        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = pagedResult.TotalCount
        };
    }
}
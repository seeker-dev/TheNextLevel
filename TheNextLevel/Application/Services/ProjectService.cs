using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Extensions;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;

    public ProjectService(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        return project.ToDto();
    }

    public async Task<PagedResult<ProjectDto>> ListByMissionAsync(int missionId, int skip, int take)
    {
        var pagedProjects = await _projectRepository.ListByMissionIdAsync(missionId, skip, take);
        var projectDtos = pagedProjects.Items.Select(p => p.ToDto()).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto project)
    {
        var createdProject = await _projectRepository.CreateAsync(project.MissionId, project.Name, project.Description);

        ArgumentNullException.ThrowIfNull(createdProject, nameof(createdProject));

        return createdProject.ToDto();
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto project)
    {
        var updatedProject = await _projectRepository.UpdateAsync(id, project.Name, project.Description);

        ArgumentNullException.ThrowIfNull(updatedProject, nameof(updatedProject));

        return updatedProject.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _projectRepository.DeleteAsync(id);
    }

    public async Task<bool> CompleteAsync(int id)
    {
        return await _projectRepository.CompleteAsync(id);
    }

    public async Task<bool> ResetAsync(int id)
    {
        return await _projectRepository.ResetAsync(id);
    }

    public async Task<PagedResult<ProjectDto>> ListAsync(int skip, int take)
    {
        var pagedResult = await _projectRepository.ListAsync(skip, take);

        return new PagedResult<ProjectDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }
}
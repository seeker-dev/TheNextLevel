using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public ProjectService(IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }

    public async Task<ProjectDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project == null)
            return null;

        return ProjectDto.From(project);
    }

    public async Task<PagedResult<ProjectDto>> ListByMissionAsync(int missionId, int skip, int take, CancellationToken ct = default)
    {
        var pagedProjects = await _projectRepository.ListByMissionIdAsync(missionId, skip, take, ct);
        var projectDtos = pagedProjects.Items.Select(ProjectDto.From).ToList();

        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = pagedProjects.TotalCount
        };
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto project, CancellationToken ct = default)
    {
        var createdProject = await _projectRepository.CreateAsync(project.MissionId, project.Name, project.Description, ct);

        ArgumentNullException.ThrowIfNull(createdProject, nameof(createdProject));

        return ProjectDto.From(createdProject);
    }

    public async Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto project, CancellationToken ct = default)
    {
        var updatedProject = await _projectRepository.UpdateAsync(id, project.Name, project.Description, ct);

        ArgumentNullException.ThrowIfNull(updatedProject, nameof(updatedProject));

        return ProjectDto.From(updatedProject);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(id, ct);
        if (project == null)
            return false;

        var tasks = await _taskRepository.ListByProjectIdAsync(id, 0, 1, ct);
        if (tasks.TotalCount > 0)
            throw new InvalidOperationException("Cannot delete a project that has tasks.");

        return await _projectRepository.DeleteAsync(id, ct);
    }

    public async Task<bool> CompleteAsync(int id, CancellationToken ct = default)
    {
        return await _projectRepository.CompleteAsync(id, ct);
    }

    public async Task<bool> ResetAsync(int id, CancellationToken ct = default)
    {
        return await _projectRepository.ResetAsync(id, ct);
    }

    public async Task<PagedResult<ProjectDto>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        var pagedResult = await _projectRepository.ListAsync(skip, take, ct);

        return new PagedResult<ProjectDto>
        {
            Items = pagedResult.Items.Select(ProjectDto.From),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<bool> MoveAsync(int projectId, int newParentId, CancellationToken ct = default)
    {
        return await _projectRepository.MoveAsync(projectId, newParentId, ct);
    }
}

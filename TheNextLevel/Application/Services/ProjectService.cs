using Microsoft.AspNetCore.Components.Web.Virtualization;
using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ITaskRepository _taskRepository;

    public ProjectService(IProjectRepository projectRepository, ITaskRepository taskRepository)
    {
        _projectRepository = projectRepository;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync(bool includeTasks = false)
    {
        var projects = await _projectRepository.GetAllAsync(includeTasks);
        var projectDtos = new List<ProjectDto>();

        foreach (var project in projects)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            projectDtos.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Tasks = [.. tasks.Select(t => new TaskDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.ProjectId
                ))],
            });
        }

        return projectDtos;
    }

    private async ValueTask<ItemsProviderResult<ProjectDto>> LoadProjects(ItemsProviderRequest request)
    {
        var projects = await _projectRepository.GetAsync(request.StartIndex, request.Count);
        var totalProjects = await _projectRepository.GetTotalProjectsCountAsync();

        var projectDtos = new List<ProjectDto>();
        foreach (var project in projects)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            projectDtos.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Tasks = [.. tasks.Select(t => new TaskDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.ProjectId
                ))],
            });
        }

        return new ItemsProviderResult<ProjectDto>(projectDtos, totalProjects);
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
        return new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Tasks = [.. tasks.Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.ProjectId
            ))],
        };
    }

    public async Task<ProjectDto> CreateProjectAsync(string name, string description)
    {
        var project = new Project(name, description);
        var createdProject = await _projectRepository.AddAsync(project);

        return new ProjectDto
        {
            Id = createdProject.Id,
            Name = createdProject.Name,
            Description = createdProject.Description,
            Tasks = []
        };
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, string name, string description)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        project.UpdateName(name);
        project.UpdateDescription(description);

        var updatedProject = await _projectRepository.UpdateAsync(project);
        var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);

        return new ProjectDto
        {
            Id = updatedProject.Id,
            Name = updatedProject.Name,
            Description = updatedProject.Description,
            Tasks = [.. tasks.Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                t.IsCompleted,
                t.ProjectId
            ))],
        };
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
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            projectDtos.Add(new ProjectDto
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Tasks = [.. tasks.Select(t => new TaskDto(
                    t.Id,
                    t.Title,
                    t.Description,
                    t.IsCompleted,
                    t.ProjectId
                ))],
            });
        }

        return new PagedResult<ProjectDto>
        {
            Items = projectDtos,
            TotalCount = pagedResult.TotalCount
        };
    }
}
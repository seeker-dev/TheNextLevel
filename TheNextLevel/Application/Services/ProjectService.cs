using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Extensions;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IAccountContext _accountContext;

    public ProjectService(IProjectRepository projectRepository, IAccountContext accountContext)
    {
        _projectRepository = projectRepository;
        _accountContext = accountContext ?? throw new ArgumentNullException(nameof(accountContext));
    }

    public async Task<ProjectDto?> GetProjectByIdAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        return project.ToDto();
    }

    public async Task<ProjectDto> CreateProjectAsync(string name, string description)
    {
        var project = new Project(name, description);
        project.AccountId = _accountContext.GetCurrentAccountId();

        var createdProject = await _projectRepository.AddAsync(project);

        return createdProject.ToDto();
    }

    public async Task<ProjectDto?> UpdateProjectAsync(int id, string name, string description)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
            return null;

        project.UpdateName(name);
        project.UpdateDescription(description);

        var updatedProject = await _projectRepository.UpdateAsync(project);

        return updatedProject.ToDto();
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        return await _projectRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<ProjectDto>> GetProjectsPagedAsync(int skip, int take, string? filterText = null)
    {
        var pagedResult = await _projectRepository.GetPagedAsync(skip, take, filterText);

        return new PagedResult<ProjectDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }
}
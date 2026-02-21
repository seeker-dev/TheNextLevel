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

    public async Task<ProjectDto> CreateAsync(string name, string description, int missionId)
    {
        var createdProject = await _projectRepository.AddAsync(name, description, missionId);
        return createdProject.ToDto();
    }

    public async Task<ProjectDto?> UpdateAsync(int id, string name, string description)
    {
        var updatedProject = await _projectRepository.UpdateAsync(id, name, description);
        return updatedProject?.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _projectRepository.DeleteAsync(id);
    }

    public async Task<PagedResult<ProjectDto>> ListAsync(int skip, int take, string? filterText = null)
    {
        var pagedResult = await _projectRepository.GetPagedAsync(skip, take, filterText);

        return new PagedResult<ProjectDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }
}
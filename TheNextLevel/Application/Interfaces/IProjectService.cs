using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(int id);
    Task<PagedResult<ProjectDto>> ListAsync(int skip, int take, string? filterText = null);
    Task<ProjectDto> CreateAsync(string name, string description, int missionId);
    Task<ProjectDto?> UpdateAsync(int id, string name, string description);
    Task<bool> DeleteAsync(int id);
}
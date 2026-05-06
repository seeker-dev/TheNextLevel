using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IProjectService
{
    Task<ProjectDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<ProjectDto>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<PagedResult<ProjectDto>> ListByMissionAsync(int missionId, int skip, int take, CancellationToken ct = default);
    Task<ProjectDto> CreateAsync(CreateProjectDto request, CancellationToken ct = default);
    Task<ProjectDto> UpdateAsync(int id, UpdateProjectDto request, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, CancellationToken ct = default);
    Task<bool> ResetAsync(int id, CancellationToken ct = default);
    Task<bool> MoveAsync(int projectId, int newParentId, CancellationToken ct = default);
}

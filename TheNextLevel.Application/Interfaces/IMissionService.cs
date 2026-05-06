using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IMissionService
{
    Task<MissionDto?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<MissionDto>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<MissionDto> CreateAsync(CreateMissionDto mission, CancellationToken ct = default);
    Task<MissionDto> UpdateAsync(int id, UpdateMissionDto mission, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, CancellationToken ct = default);
    Task<bool> ResetAsync(int id, CancellationToken ct = default);
}

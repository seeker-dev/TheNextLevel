using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Application.Interfaces;

public interface IMissionService
{
    Task<MissionDto?> GetByIdAsync(int id);
    Task<PagedResult<MissionDto>> ListAsync(int skip, int take);
    Task<MissionDto> CreateAsync(CreateMissionDto mission);
    Task<MissionDto> UpdateAsync(int id, UpdateMissionDto mission);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
}
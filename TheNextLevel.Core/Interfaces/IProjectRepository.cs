using TheNextLevel.Core.Entities;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Core.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Project>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<PagedResult<Project>> ListByMissionIdAsync(int missionId, int skip, int take, CancellationToken ct = default);
    Task<Project> CreateAsync(int missionId, string title, string description, CancellationToken ct = default);
    Task<Project?> UpdateAsync(int id, string name, string description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, CancellationToken ct = default);
    Task<bool> ResetAsync(int id, CancellationToken ct = default);
    Task<int> CountAsync(string? filterText = null, CancellationToken ct = default);
    Task<bool> MoveAsync(int id, int missionId, CancellationToken ct = default);
}

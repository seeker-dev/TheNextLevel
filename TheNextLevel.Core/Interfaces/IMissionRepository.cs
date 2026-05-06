using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Entities;

namespace TheNextLevel.Core.Interfaces;
public interface IMissionRepository
{
    Task<Mission?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<PagedResult<Mission>> ListAsync(int skip, int take, CancellationToken ct = default);
    Task<Mission> CreateAsync(string title, string description, CancellationToken ct = default);
    Task<Mission> UpdateAsync(int id, string title, string description, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
    Task<bool> CompleteAsync(int id, CancellationToken ct = default);
    Task<bool> ResetAsync(int id, CancellationToken ct = default);
}

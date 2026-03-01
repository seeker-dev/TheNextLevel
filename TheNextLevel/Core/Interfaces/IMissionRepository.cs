using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Entities;

namespace TheNextLevel.Core.Interfaces;
public interface IMissionRepository
{
    Task<Mission?> GetByIdAsync(int id);
    Task<PagedResult<Mission>> ListAsync(int skip, int take);
    Task<Mission> CreateAsync(string title, string description);
    Task<Mission> UpdateAsync(int id, string title, string description);
    Task<bool> DeleteAsync(int id);
    Task<bool> CompleteAsync(int id);
    Task<bool> ResetAsync(int id);
}
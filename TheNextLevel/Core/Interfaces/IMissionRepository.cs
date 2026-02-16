using TheNextLevel.Core.Entities;
using Task = TheNextLevel.Core.Entities.Task;
namespace TheNextLevel.Core.Interfaces;
public interface IMissionRepository
{
    System.Threading.Tasks.Task<Mission> GetByIdAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Mission>> ListAsync(int skip, int take, string? filterText = null);
    System.Threading.Tasks.Task<Mission> AddAsync(Mission mission);
    System.Threading.Tasks.Task<Mission> UpdateAsync(Mission mission);
    System.Threading.Tasks.Task<bool> DeleteAsync(int id);
    System.Threading.Tasks.Task CompleteAsync(int id);
    System.Threading.Tasks.Task ResetAsync(int id);
    System.Threading.Tasks.Task<IEnumerable<Project>> ListProjectsAsync(int id, int skip, int take, string? filterText = null);
    System.Threading.Tasks.Task<IEnumerable<Task>> ListTasksAsync(int id, int skip, int take, string? filterText = null);
    System.Threading.Tasks.Task AssignAsync(int id, int projectId);
}
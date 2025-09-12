using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    
    public TaskService(ITaskRepository taskRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
    }
    
    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Select(MapToDto);
    }
    
    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task != null ? MapToDto(task) : null;
    }
    
    public async System.Threading.Tasks.Task<Guid> CreateTaskAsync(CreateTaskRequest request)
    {
        var task = new Core.Entities.Task(request.Title, request.Description);
        
        await _taskRepository.AddAsync(task);
        return task.Id;
    }
    
    public async System.Threading.Tasks.Task<bool> UpdateTaskAsync(Guid id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.UpdateTitle(request.Title);
        task.UpdateDescription(request.Description);
        
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(Guid id)
    {
        return await _taskRepository.DeleteAsync(id);
    }
    
    public async System.Threading.Tasks.Task<bool> CompleteTaskAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.MarkComplete();
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> ReopenTaskAsync(Guid id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.Reopen();
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(bool isCompleted)
    {
        var tasks = await _taskRepository.GetByStatusAsync(isCompleted);
        return tasks.Select(MapToDto);
    }
    
    private static TaskDto MapToDto(Core.Entities.Task task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            task.IsCompleted,
            task.CreatedAt
        );
    }
}
using TheNextLevel.Application.Tasks.DTOs;
using TheNextLevel.Domain.Tasks.Entities;
using TheNextLevel.Domain.Tasks.ValueObjects;
using TheNextLevel.Infrastructure.Data;
using DomainTask = TheNextLevel.Domain.Tasks.Entities.Task;
using TaskStatus = TheNextLevel.Domain.Tasks.ValueObjects.TaskStatus;

namespace TheNextLevel.Application.Tasks.Services;

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
    
    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(TaskId id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task != null ? MapToDto(task) : null;
    }
    
    public async System.Threading.Tasks.Task<TaskId> CreateTaskAsync(CreateTaskRequest request)
    {
        var priority = Priority.FromString(request.Priority);
        var task = DomainTask.Create(request.Title, request.Description, priority, request.DueDate);
        
        await _taskRepository.AddAsync(task);
        return task.Id;
    }
    
    public async System.Threading.Tasks.Task<bool> UpdateTaskAsync(TaskId id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.UpdateTitle(request.Title);
        task.UpdateDescription(request.Description);
        task.UpdatePriority(Priority.FromString(request.Priority));
        task.UpdateDueDate(request.DueDate);
        
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(TaskId id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        await _taskRepository.DeleteAsync(id);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> CompleteTaskAsync(TaskId id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.MarkComplete();
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> StartTaskAsync(TaskId id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.Start();
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<bool> ReopenTaskAsync(TaskId id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;
        
        task.Reopen();
        await _taskRepository.UpdateAsync(task);
        return true;
    }
    
    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetOverdueTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        return tasks.Where(t => t.IsOverdue).Select(MapToDto);
    }
    
    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(string status)
    {
        var tasks = await _taskRepository.GetAllAsync();
        var taskStatus = TaskStatus.FromString(status);
        return tasks.Where(t => t.Status == taskStatus).Select(MapToDto);
    }
    
    private static TaskDto MapToDto(DomainTask task)
    {
        return new TaskDto(
            task.Id.Value,
            task.Title,
            task.Description,
            task.Status.Name,
            task.Priority.Name,
            task.Priority.Color,
            task.CreatedAt,
            task.DueDate,
            task.CompletedAt,
            task.IsOverdue,
            task.DaysUntilDue
        );
    }
}
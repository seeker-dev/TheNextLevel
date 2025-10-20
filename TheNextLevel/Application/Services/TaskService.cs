using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Shared.DTOs;

namespace TheNextLevel.Application.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IAccountContext _accountContext;

    public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository, IAccountContext accountContext)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
        _accountContext = accountContext ?? throw new ArgumentNullException(nameof(accountContext));
    }
    
    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetAllTasksAsync()
    {
        var tasks = await _taskRepository.GetAllAsync();
        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }
    
    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task != null ? await MapToDtoAsync(task) : null;
    }

    public async System.Threading.Tasks.Task<int> CreateTaskAsync(CreateTaskRequest request)
    {
        var task = new Core.Entities.Task(request.Name, request.Description);
        task.AccountId = _accountContext.GetCurrentAccountId();

        await _taskRepository.AddAsync(task);
        return task.Id;
    }

    public async System.Threading.Tasks.Task<bool> UpdateTaskAsync(int id, UpdateTaskRequest request)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        task.UpdateName(request.Name);
        task.UpdateDescription(request.Description);

        await _taskRepository.UpdateAsync(task);
        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteTaskAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    public async System.Threading.Tasks.Task<bool> CompleteTaskAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        task.MarkComplete();
        await _taskRepository.UpdateAsync(task);
        return true;
    }

    public async System.Threading.Tasks.Task<bool> ReopenTaskAsync(int id)
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
        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByProjectAsync(int projectId)
    {
        var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetUngroupedTasksAsync()
    {
        var tasks = await _taskRepository.GetUngroupedTasksAsync();
        var taskDtos = new List<TaskDto>();
        foreach (var task in tasks)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }
        return taskDtos;
    }

    public async System.Threading.Tasks.Task<bool> AssignTaskToProjectAsync(int taskId, int? projectId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        if (task == null) return false;

        // If assigning to a project, verify the project exists
        if (projectId.HasValue)
        {
            var project = await _projectRepository.GetByIdAsync(projectId.Value);
            if (project == null) return false;
        }

        // Update the task's project assignment
        task.ProjectId = projectId;
        await _taskRepository.UpdateAsync(task);
        return true;
    }

    public async System.Threading.Tasks.Task<PagedResult<TaskDto>> GetTasksPagedAsync(int skip, int take, bool isCompleted = false)
    {
        var pagedResult = await _taskRepository.GetPagedAsync(skip, take, isCompleted);

        var taskDtos = new List<TaskDto>();
        foreach (var task in pagedResult.Items)
        {
            taskDtos.Add(await MapToDtoAsync(task));
        }

        return new PagedResult<TaskDto>
        {
            Items = taskDtos,
            TotalCount = pagedResult.TotalCount
        };
    }

    private async System.Threading.Tasks.Task<TaskDto> MapToDtoAsync(Core.Entities.Task task)
    {
        return new TaskDto(
            task.Id,
            task.AccountId,
            task.Name,
            task.Description,
            task.IsCompleted,
            task.ProjectId
        );
    }
}
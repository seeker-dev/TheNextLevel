using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Extensions;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Core.DTOs;

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
        return tasks.ToDto();
    }
    
    public async System.Threading.Tasks.Task<TaskDto?> GetTaskByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task?.ToDto();
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

        // Auto-complete all subtasks if this is a parent task
        if (task.ParentTaskId == null)
        {
            await _taskRepository.BulkCompleteSubtasksAsync(id);
        }

        return true;
    }

    public async System.Threading.Tasks.Task<bool> ReopenTaskAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        task.Reopen();
        await _taskRepository.UpdateAsync(task);

        // If reopening a subtask, reopen parent if completed
        if (task.ParentTaskId.HasValue)
        {
            var parentTask = await _taskRepository.GetByIdAsync(task.ParentTaskId.Value);
            if (parentTask != null && parentTask.IsCompleted)
            {
                parentTask.Reopen();
                await _taskRepository.UpdateAsync(parentTask);
            }
        }

        return true;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(bool isCompleted)
    {
        var tasks = await _taskRepository.GetByStatusAsync(isCompleted);
        return tasks.ToDto();
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetTasksByProjectAsync(int projectId)
    {
        var tasks = await _taskRepository.GetTasksByProjectIdAsync(projectId);
        return tasks.ToDto();
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetUngroupedTasksAsync()
    {
        var tasks = await _taskRepository.GetUngroupedTasksAsync();
        return tasks.ToDto();
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

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async System.Threading.Tasks.Task<int> CreateSubtaskAsync(CreateSubtaskRequest request)
    {
        // Validate parent task exists
        var parentTask = await _taskRepository.GetByIdAsync(request.ParentTaskId);
        if (parentTask == null)
            throw new InvalidOperationException("Parent task not found");

        // Enforce single-level nesting
        if (parentTask.ParentTaskId.HasValue)
            throw new InvalidOperationException("Cannot create subtask under another subtask. Only single-level nesting is supported.");

        // Create subtask without project association - subtasks belong to their parent task, not directly to projects
        var subtask = new Core.Entities.Task(request.Name, request.Description);
        subtask.AccountId = _accountContext.GetCurrentAccountId();
        subtask.ParentTaskId = request.ParentTaskId;

        await _taskRepository.AddAsync(subtask);
        return subtask.Id;
    }

    public async System.Threading.Tasks.Task<IEnumerable<TaskDto>> GetSubtasksByParentIdAsync(int parentTaskId)
    {
        var subtasks = await _taskRepository.GetSubtasksByParentIdAsync(parentTaskId);
        return subtasks.ToDto();
    }

    public async System.Threading.Tasks.Task<bool> CanTaskHaveSubtasksAsync(int taskId)
    {
        var task = await _taskRepository.GetByIdAsync(taskId);
        return task?.ParentTaskId == null;
    }
}
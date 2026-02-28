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

    public TaskService(ITaskRepository taskRepository, IProjectRepository projectRepository)
    {
        _taskRepository = taskRepository ?? throw new ArgumentNullException(nameof(taskRepository));
        _projectRepository = projectRepository ?? throw new ArgumentNullException(nameof(projectRepository));
    }

    public async System.Threading.Tasks.Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task?.ToDto();
    }

    public async System.Threading.Tasks.Task<int> CreateAsync(CreateTaskRequest request)
    {
        var task = await _taskRepository.AddAsync(request.Name, request.Description, projectId: request.ProjectId);
        return task.Id;
    }

    public async System.Threading.Tasks.Task<bool> UpdateAsync(int id, UpdateTaskRequest request)
    {
        return await _taskRepository.UpdateAsync(id, request.Name, request.Description);
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    public async System.Threading.Tasks.Task<bool> CompleteAsync(int id)
    {
        var completed = await _taskRepository.CompleteAsync(id);
        if (!completed) return false;

        await _taskRepository.BulkCompleteSubtasksAsync(id);
        return true;
    }

    public async System.Threading.Tasks.Task<bool> ReopenAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        await _taskRepository.ReopenAsync(id);

        // If reopening a subtask, reopen parent if completed
        if (task.ParentTaskId.HasValue)
        {
            var parentTask = await _taskRepository.GetByIdAsync(task.ParentTaskId.Value);
            if (parentTask?.IsCompleted == true)
                await _taskRepository.ReopenAsync(parentTask.Id);
        }

        return true;
    }

    public async System.Threading.Tasks.Task<bool> AssignAsync(int taskId, int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) return false;

        return await _taskRepository.AssignToProjectAsync(taskId, projectId);
    }

    public async System.Threading.Tasks.Task<PagedResult<TaskDto>> ListAsync(int skip, int take, bool isCompleted = false)
    {
        var pagedResult = await _taskRepository.GetPagedAsync(skip, take, isCompleted);

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async System.Threading.Tasks.Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take, bool isCompleted = false)
    {
        var pagedResult = await _taskRepository.GetPagedByProjectIdAsync(projectId, skip, take, isCompleted);

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
        if (parentTask is null)
            throw new InvalidOperationException("Parent task not found");

        // Enforce single-level nesting
        if (parentTask.ParentTaskId.HasValue)
            throw new InvalidOperationException("Cannot create subtask under another subtask. Only single-level nesting is supported.");

        // Create subtask without project association - subtasks belong to their parent task, not directly to projects
        var subtask = await _taskRepository.AddAsync(request.Name, request.Description, parentTaskId: request.ParentTaskId);
        return subtask.Id;
    }

    public async System.Threading.Tasks.Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take)
    {
        var subtasks = await _taskRepository.GetSubtasksByParentIdAsync(parentTaskId, skip, take);

        return new PagedResult<TaskDto>
        {
            Items = subtasks.Items.ToDto(),
            TotalCount = subtasks.TotalCount
        };
    }
}

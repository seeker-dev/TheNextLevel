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

    public async Task<TaskDto?> GetByIdAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        return task?.ToDto();
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request)
    {
        var task = await _taskRepository.CreateAsync(request.ProjectId, request.Name, request.Description);
        return task.ToDto();
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request)
    {
        var updated = await _taskRepository.UpdateAsync(id, request.Name, request.Description);
        if (!updated) throw new InvalidOperationException("Task not found");

        var task = await _taskRepository.GetByIdAsync(id);
        return task!.ToDto();
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _taskRepository.DeleteAsync(id);
    }

    public async Task<bool> CompleteAsync(int id)
    {
        var completed = await _taskRepository.CompleteAsync(id);
        if (!completed) return false;

        await _taskRepository.BulkCompleteSubtasksAsync(id);
        return true;
    }

    public async Task<bool> ResetAsync(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null) return false;

        await _taskRepository.ResetAsync(id);

        // If resetting a subtask, reset parent if completed
        if (task.ParentTaskId.HasValue)
        {
            var parentTask = await _taskRepository.GetByIdAsync(task.ParentTaskId.Value);
            if (parentTask?.IsCompleted == true)
                await _taskRepository.ResetAsync(parentTask.Id);
        }

        return true;
    }

    public async Task<bool> MoveAsync(int taskId, int projectId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null) return false;

        return await _taskRepository.MoveAsync(taskId, projectId);
    }

    public async Task<PagedResult<TaskDto>> ListAsync(int skip, int take)
    {
        var pagedResult = await _taskRepository.ListAsync(skip, take);

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take)
    {
        var pagedResult = await _taskRepository.ListByProjectIdAsync(projectId, skip, take);

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.ToDto(),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take)
    {
        var subtasks = await _taskRepository.ListSubtasksByParentIdAsync(parentTaskId, skip, take);

        return new PagedResult<TaskDto>
        {
            Items = subtasks.Items.ToDto(),
            TotalCount = subtasks.TotalCount
        };
    }

    public async Task<TaskDto> CreateSubtaskAsync(CreateSubtaskRequest request)
    {
        // Validate parent task exists
        var parentTask = await _taskRepository.GetByIdAsync(request.ParentTaskId);
        if (parentTask is null)
            throw new InvalidOperationException("Parent task not found");

        // Enforce single-level nesting
        if (parentTask.ParentTaskId.HasValue)
            throw new InvalidOperationException("Cannot create subtask under another subtask. Only single-level nesting is supported.");

        // Create subtask without project association - subtasks belong to their parent task, not directly to projects
        var subtask = await _taskRepository.CreateSubtaskAsync(parentTask.Id, request.Name, request.Description);
        return subtask.ToDto();
    }

    public async Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description)
    {
        return await _taskRepository.UpdateSubtaskAsync(id, parentId, name, description);
    }

     public async Task<bool> DeleteSubtaskAsync(int id, int parentId)
     {
         return await _taskRepository.DeleteSubtaskAsync(id, parentId);
     }

    public async Task<int> BulkCompleteSubtasksAsync(int parentId)
    {
        return await _taskRepository.BulkCompleteSubtasksAsync(parentId);
    }
}

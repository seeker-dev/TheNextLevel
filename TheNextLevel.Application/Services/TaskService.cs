using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.Interfaces;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Core.DTOs;
using TheNextLevel.Application.DTOs.Projections;

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

    public async Task<TaskDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        return task is null ? null : TaskDto.From(task);
    }

    public async Task<TaskDto> CreateAsync(CreateTaskRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepository.CreateAsync(request.ProjectId, request.Name, request.Description, ct);
        return TaskDto.From(task);
    }

    public async Task<TaskDto> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken ct = default)
    {
        var updated = await _taskRepository.UpdateAsync(id, request.Name, request.Description, ct);
        if (!updated) throw new InvalidOperationException("Task not found");

        var task = await _taskRepository.GetByIdAsync(id, ct);
        return TaskDto.From(task!);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        return await _taskRepository.DeleteAsync(id, ct);
    }

    public async Task<bool> SetStatusAsync(int id, TaskState status, CancellationToken ct = default)
    {
        var task = await _taskRepository.GetByIdAsync(id, ct);
        if (task == null) return false;

        await _taskRepository.SetStatusAsync(id, (int)status, ct);

        // When completing a parent task, bulk-complete its subtasks
        if (status == TaskState.Completed && !task.ParentTaskId.HasValue)
            await _taskRepository.BulkCompleteSubtasksAsync(id, ct);

        // When moving a subtask away from Completed, reset parent if it was completed
        if (status != TaskState.Completed && task.ParentTaskId.HasValue)
        {
            var parentTask = await _taskRepository.GetByIdAsync(task.ParentTaskId.Value, ct);
            if (parentTask?.Status == (int)TaskState.Completed)
                await _taskRepository.SetStatusAsync(parentTask.Id, (int)TaskState.Inactive, ct);
        }

        return true;
    }

    public async Task<bool> MoveAsync(int taskId, int projectId, CancellationToken ct = default)
    {
        var project = await _projectRepository.GetByIdAsync(projectId, ct);
        if (project == null) return false;

        return await _taskRepository.MoveAsync(taskId, projectId, ct);
    }

    public async Task<PagedResult<TaskDto>> ListAsync(int skip, int take, CancellationToken ct = default)
    {
        var pagedResult = await _taskRepository.ListAsync(skip, take, ct);

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.Select(TaskDto.From),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<TaskDto>> ListByProjectAsync(int projectId, int skip, int take, CancellationToken ct = default)
    {
        var pagedResult = await _taskRepository.ListByProjectIdAsync(projectId, skip, take, ct);

        return new PagedResult<TaskDto>
        {
            Items = pagedResult.Items.Select(TaskDto.From),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<TaskSummaryDto>> ListByStatus(TaskState status, int skip, int take, CancellationToken ct = default)
    {
        var pagedResult = await _taskRepository.ListByStatus((int)status, skip, take, ct);

        return new PagedResult<TaskSummaryDto>
        {
            Items = pagedResult.Items.Select(TaskSummaryDto.From),
            TotalCount = pagedResult.TotalCount
        };
    }

    public async Task<PagedResult<TaskDto>> ListSubtasksByParentIdAsync(int parentTaskId, int skip, int take, CancellationToken ct = default)
    {
        var subtasks = await _taskRepository.ListSubtasksByParentIdAsync(parentTaskId, skip, take, ct);

        return new PagedResult<TaskDto>
        {
            Items = subtasks.Items.Select(TaskDto.From),
            TotalCount = subtasks.TotalCount
        };
    }

    public async Task<TaskDto> CreateSubtaskAsync(CreateSubtaskRequest request, CancellationToken ct = default)
    {
        // Validate parent task exists
        var parentTask = await _taskRepository.GetByIdAsync(request.ParentTaskId, ct);
        if (parentTask is null)
            throw new InvalidOperationException("Parent task not found");

        // Enforce single-level nesting
        if (parentTask.ParentTaskId.HasValue)
            throw new InvalidOperationException("Cannot create subtask under another subtask. Only single-level nesting is supported.");

        // Create subtask without project association - subtasks belong to their parent task, not directly to projects
        var subtask = await _taskRepository.CreateSubtaskAsync(parentTask.Id, request.Name, request.Description, ct);
        return TaskDto.From(subtask);
    }

    public async Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description, CancellationToken ct = default)
    {
        return await _taskRepository.UpdateSubtaskAsync(id, parentId, name, description, ct);
    }

    public async Task<bool> DeleteSubtaskAsync(int id, int parentId, CancellationToken ct = default)
    {
        return await _taskRepository.DeleteSubtaskAsync(id, parentId, ct);
    }
}

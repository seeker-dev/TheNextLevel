using TaskEntity = TheNextLevel.Core.Entities.Task;

namespace TheNextLevel.Application.DTOs;

public record TaskDto(int Id, string Name, string Description, TaskState Status, int? ProjectId = null, int? ParentTaskId = null) : IItemDto
{
    public static TaskDto From(TaskEntity task) => new(
        task.Id,
        task.Name,
        task.Description ?? string.Empty,
        (TaskState)task.Status,
        task.ProjectId,
        task.ParentTaskId
    );
}

public record CreateTaskRequest(int ProjectId, string Name, string Description);
public record UpdateTaskRequest(string Name, string Description, int ProjectId);

public record CreateSubtaskRequest(int ParentTaskId, string Name, string Description);
public record UpdateSubtaskRequest(string Name, string Description, int ParentTaskId);

public enum TaskState
{
    Inactive,
    Active,
    Completed,
    OnHold
}

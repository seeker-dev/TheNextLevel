using TheNextLevel.Application.DTOs;
using TaskEntity = TheNextLevel.Core.Entities.Task;

namespace TheNextLevel.Application.Extensions;

public static class TaskMappingExtensions
{
    public static TaskDto ToDto(this TaskEntity task)
    {
        return new TaskDto(
            task.Id,
            task.AccountId,
            task.Name,
            task.Description ?? string.Empty,
            task.IsCompleted,
            task.ProjectId,
            task.ParentTaskId
        );
    }

    public static IEnumerable<TaskDto> ToDto(this IEnumerable<TaskEntity> tasks)
    {
        return tasks.Select(task => task.ToDto());
    }
}


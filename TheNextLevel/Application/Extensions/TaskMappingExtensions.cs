using TheNextLevel.Application.DTOs;
using TheNextLevel.Application.DTOs.Projections;
using TheNextLevel.Core.Entities;
using TaskEntity = TheNextLevel.Core.Entities.Task;

namespace TheNextLevel.Application.Extensions;

public static class TaskMappingExtensions
{
    public static TaskDto ToDto(this TaskEntity task)
    {
        return new TaskDto(
            task.Id,
            task.Name,
            task.Description ?? string.Empty,
            (TaskState)task.Status,
            task.ProjectId,
            task.ParentTaskId
        );
    }

    public static TaskFullHierarchyDto ToDto(this TaskFullHierarchyProjection task)
    {
        return new TaskFullHierarchyDto(
            task.Id,
            task.Name,
            task.Description,
            (TaskState)task.Status,
            task.ProjectId.Value,
            task.ProjectTitle,
            task.MissionId.Value,
            task.MissionTitle
        );
    }

    public static IEnumerable<TaskDto> ToDto(this IEnumerable<TaskEntity> tasks)
    {
        return tasks.Select(task => task.ToDto());
    }

    public static IEnumerable<TaskFullHierarchyDto> ToDto(this IEnumerable<TaskFullHierarchyProjection> tasks)
    {
        return tasks.Select(task => task.ToDto());
    }
}


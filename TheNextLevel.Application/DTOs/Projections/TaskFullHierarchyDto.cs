using TheNextLevel.Core.Entities;

namespace TheNextLevel.Application.DTOs.Projections;

public record TaskFullHierarchyDto(int Id, string Name, string Description, TaskState Status, int ProjectId, string ProjectTitle, int MissionId, string MissionTitle) : IItemDto
{
    public static TaskFullHierarchyDto From(TaskFullHierarchyProjection task) => new(
        task.Id,
        task.Name,
        task.Description ?? string.Empty,
        (TaskState)task.Status,
        task.ProjectId!.Value,
        task.ProjectTitle,
        task.MissionId!.Value,
        task.MissionTitle
    );
}

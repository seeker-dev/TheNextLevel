using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.Entities;

namespace TheNextLevel.Application.Extensions;

public static class ProjectMappingExtensions
{
    public static ProjectDto ToDto(this Project project)
    {
        return new ProjectDto(
            project.Id,
            project.Name,
            project.Description ?? string.Empty,
            project.MissionId,
            project.IsCompleted
        );
    }

    public static IEnumerable<ProjectDto> ToDto(this IEnumerable<Project> projects)
    {
        return projects.Select(project => project.ToDto());
    }
}

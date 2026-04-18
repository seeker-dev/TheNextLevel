using TheNextLevel.Core.Entities;

namespace TheNextLevel.Application.DTOs;

public record ProjectDto(int Id, string Name, string Description, int MissionId, bool IsCompleted) : IItemDto
{
    public static ProjectDto From(Project project) => new(
        project.Id,
        project.Name,
        project.Description ?? string.Empty,
        project.MissionId,
        project.IsCompleted
    );
}

public record CreateProjectDto(int MissionId, string Name, string Description);
public record UpdateProjectDto(string Name, string Description, int MissionId, bool IsCompleted);

// Projections
public record EligibleProjectDto(int Id, string Name, string Description, string MissionTitle);

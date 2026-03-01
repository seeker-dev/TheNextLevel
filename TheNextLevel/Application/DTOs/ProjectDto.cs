namespace TheNextLevel.Application.DTOs;
public record ProjectDto(int Id, string Name, string Description, int MissionId, bool IsCompleted) : IItemDto;

public record CreateProjectDto(int MissionId, string Name, string Description);
public record UpdateProjectDto(string Name, string Description, int MissionId, bool IsCompleted);

// Projections
public record EligibleProjectDto(int Id, string Name, string Description, string MissionTitle);
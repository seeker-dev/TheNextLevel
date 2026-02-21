namespace TheNextLevel.Application.DTOs;
public record ProjectDto(int Id, string Name, string Description, string? MissionTitle = null);

public record CreateProjectDto(string Name, string Description, int? MissionId = null);
public record UpdateProjectDto(int Id, string Name, string Description, int? MissionId = null);
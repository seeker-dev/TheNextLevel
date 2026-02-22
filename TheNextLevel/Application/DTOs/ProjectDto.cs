namespace TheNextLevel.Application.DTOs;
public record ProjectDto(int Id, string Name, string Description) : IItemDto;
public record EligibleProjectDto(int Id, string Name, string Description, string MissionTitle);

public record CreateProjectDto(string Name, string Description, int? MissionId = null);
public record UpdateProjectDto(int Id, string Name, string Description, int? MissionId = null);
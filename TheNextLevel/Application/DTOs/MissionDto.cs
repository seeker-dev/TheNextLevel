namespace TheNextLevel.Application.DTOs;

public record MissionDto(int Id, string Title, string Description, bool IsCompleted);

public record CreateMissionDto(string Title, string Description);
public record UpdateMissionDto(int Id, string Title, string Description, bool IsCompleted);
namespace TheNextLevel.Application.DTOs;

public record MissionDto(int Id, string Name, string Description, bool IsCompleted) : IItemDto;

public record CreateMissionDto(string Name, string Description);
public record UpdateMissionDto(int Id, string Name, string Description, bool IsCompleted);
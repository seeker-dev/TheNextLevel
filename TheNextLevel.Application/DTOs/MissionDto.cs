using TheNextLevel.Core.Entities;

namespace TheNextLevel.Application.DTOs;

public record MissionDto(int Id, string Name, string Description, bool IsCompleted) : IItemDto
{
    public static MissionDto From(Mission mission) => new(
        mission.Id,
        mission.Title,
        mission.Description,
        mission.IsCompleted
    );
}

public record CreateMissionDto(string Name, string Description);
public record UpdateMissionDto(string Name, string Description, bool IsCompleted);

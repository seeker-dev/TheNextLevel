namespace TheNextLevel.Application.DTOs;

public record TaskDto(
    int Id,
    int AccountId,
    string Name,
    string Description,
    bool IsCompleted,
    int? ProjectId = null
);

public record CreateTaskRequest(string Name, string Description);
public record UpdateTaskRequest(string Name, string Description);
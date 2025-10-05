namespace TheNextLevel.Application.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    bool IsCompleted,
    DateTime CreatedAt,
    Guid? ProjectId = null
);

public record CreateTaskRequest(string Title, string Description);
public record UpdateTaskRequest(string Title, string Description);
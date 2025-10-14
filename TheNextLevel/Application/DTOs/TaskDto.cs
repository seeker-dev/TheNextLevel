namespace TheNextLevel.Application.DTOs;

public record TaskDto(
    int Id,
    string Title,
    string Description,
    bool IsCompleted,
    int? ProjectId = null
);

public record CreateTaskRequest(string Title, string Description);
public record UpdateTaskRequest(string Title, string Description);
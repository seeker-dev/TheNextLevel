namespace TheNextLevel.Application.Tasks.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    DateTime CreatedAt
);

public record CreateTaskRequest(
    string Title,
    string Description,
    DateTime? DueDate = null
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    DateTime? DueDate = null
);
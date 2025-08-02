namespace TheNextLevel.Application.Tasks.DTOs;

public record TaskDto(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string PriorityColor,
    DateTime CreatedAt,
    DateTime? DueDate,
    DateTime? CompletedAt,
    bool IsOverdue,
    int DaysUntilDue
);

public record CreateTaskRequest(
    string Title,
    string Description,
    string Priority,
    DateTime? DueDate = null
);

public record UpdateTaskRequest(
    string Title,
    string Description,
    string Priority,
    DateTime? DueDate = null
);
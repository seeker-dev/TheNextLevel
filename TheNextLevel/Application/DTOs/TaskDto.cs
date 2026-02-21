namespace TheNextLevel.Application.DTOs;

public record TaskDto(int Id, string Name, string Description, bool IsCompleted, int? ProjectId = null, int? ParentTaskId = null);

public record CreateTaskRequest(string Name, string Description);
public record CreateSubtaskRequest(string Name, string Description, int ParentTaskId);
public record UpdateTaskRequest(string Name, string Description);
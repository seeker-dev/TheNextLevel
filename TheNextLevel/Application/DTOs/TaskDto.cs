namespace TheNextLevel.Application.DTOs;

public record TaskDto(int Id, string Name, string Description, bool IsCompleted, int? ProjectId = null, int? ParentTaskId = null) : IItemDto;

public record CreateTaskRequest(int ProjectId, string Name, string Description);
public record UpdateTaskRequest(int Id, string Name, string Description, bool IsCompleted, int ProjectId);

public record CreateSubtaskRequest(int ParentTaskId, string Name, string Description);
public record UpdateSubtaskRequest(int Id, string Name, string Description, bool IsCompleted, int ParentTaskId);
namespace TheNextLevel.Application.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
    public int? ProjectId { get; set; }
    public int? ParentTaskId { get; set; }

    public TaskDto(
        int id,
        int accountId,
        string name,
        string description,
        bool isCompleted,
        int? projectId = null,
        int? parentTaskId = null)
    {
        Id = id;
        AccountId = accountId;
        Name = name;
        Description = description;
        IsCompleted = isCompleted;
        ProjectId = projectId;
        ParentTaskId = parentTaskId;
    }
}

public record CreateTaskRequest(string Name, string Description);
public record CreateSubtaskRequest(string Name, string Description, int ParentTaskId);
public record UpdateTaskRequest(string Name, string Description);
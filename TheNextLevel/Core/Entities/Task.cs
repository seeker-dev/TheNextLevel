namespace TheNextLevel.Core.Entities;

public class Task
{
    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }
    public int? ProjectId { get; private set; }
    public int? ParentTaskId { get; private set; }

    public Task(int id, int accountId, string name, string description, bool isCompleted, int? projectId, int? parentTaskId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Id = id;
        AccountId = accountId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        IsCompleted = isCompleted;
        ProjectId = projectId;
        ParentTaskId = parentTaskId;
    }
}

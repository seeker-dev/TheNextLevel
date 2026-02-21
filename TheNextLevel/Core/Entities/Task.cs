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

    public void UpdateName(string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName, nameof(newName));
        Name = newName.Trim();
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription?.Trim() ?? string.Empty;
    }

    public void AssignToProject(int? projectId)
    {
        ProjectId = projectId;
    }

    public void MarkComplete()
    {
        if (!IsCompleted)
            IsCompleted = true;
    }

    public void Reopen()
    {
        if (IsCompleted)
            IsCompleted = false;
    }
}

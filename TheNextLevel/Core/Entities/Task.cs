namespace TheNextLevel.Core.Entities;

public class Task
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    // Project relationship
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Subtask relationship
    public int? ParentTaskId { get; set; }
    public Task? ParentTask { get; set; }
    public ICollection<Task> Subtasks { get; set; } = new List<Task>();

    // Parameterless constructor for EF Core
    public Task()
    {
    }

    public Task(string name, string description) : this()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Task name cannot be empty", nameof(name));

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public Task(string name, string description, int? projectId) : this(name, description)
    {
        ProjectId = projectId;
    }

    public Task(string name, string description, int? projectId, int? parentTaskId) : this(name, description, projectId)
    {
        ParentTaskId = parentTaskId;
    }

    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Task name cannot be empty", nameof(newName));

        Name = newName.Trim();
    }
    
    public void UpdateDescription(string newDescription)
    {
        Description = newDescription?.Trim() ?? string.Empty;
    }
    
    public void MarkComplete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
        }
    }
    
    public void Reopen()
    {
        if (IsCompleted)
        {
            IsCompleted = false;
        }
    }

    public bool CanHaveSubtasks()
    {
        return ParentTaskId == null;
    }

    public void SetParentTask(int parentTaskId)
    {
        if (Subtasks.Any())
            throw new InvalidOperationException("Cannot set parent on a task that already has subtasks");
        ParentTaskId = parentTaskId;
    }
}
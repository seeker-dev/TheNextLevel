namespace TheNextLevel.Core.Entities;

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }

    // Project relationship
    public int? ProjectId { get; set; }
    public Project? Project { get; set; }

    // Parameterless constructor for EF Core
    public Task()
    {
    }

    public Task(string title, string description) : this()
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty", nameof(title));

        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public Task(string title, string description, int? projectId) : this(title, description)
    {
        ProjectId = projectId;
    }
    
    public void UpdateTitle(string newTitle)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
            throw new ArgumentException("Task title cannot be empty", nameof(newTitle));
            
        Title = newTitle.Trim();
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
}
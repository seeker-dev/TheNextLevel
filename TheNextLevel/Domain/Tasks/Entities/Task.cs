using TheNextLevel.Domain.Common;
using TheNextLevel.Domain.Tasks.ValueObjects;
using TaskStatus = TheNextLevel.Domain.Tasks.ValueObjects.TaskStatus;

namespace TheNextLevel.Domain.Tasks.Entities;

public class Task : Entity<TaskId>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Private constructor for EF Core
    private Task() : base() 
    {
        Status = TaskStatus.NotCompleted;
    }

    public Task(TaskId id, string title, string description) 
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty", nameof(title));
            
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Status = TaskStatus.NotCompleted;
        CreatedAt = DateTime.UtcNow;
    }

    public static Task Create(string title, string description)
    {
        return new Task(TaskId.New(), title, description);
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
        if (Status == TaskStatus.Completed)
            return; // Already completed
            
        Status = TaskStatus.Completed;
    }
    
    public void Reopen()
    {
        if (Status != TaskStatus.Completed)
            throw new InvalidOperationException("Can only reopen completed tasks");
            
        Status = TaskStatus.NotCompleted;
    }
}
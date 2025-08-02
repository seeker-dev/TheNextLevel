using TheNextLevel.Domain.Common;
using TheNextLevel.Domain.Tasks.ValueObjects;
using TaskStatus = TheNextLevel.Domain.Tasks.ValueObjects.TaskStatus;

namespace TheNextLevel.Domain.Tasks.Entities;

public class Task : Entity<TaskId>
{
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public TaskStatus Status { get; private set; }
    public Priority Priority { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    
    // Private constructor for EF Core
    private Task() : base() 
    {
        Status = TaskStatus.NotStarted;
        Priority = Priority.Medium;
    }
    
    public Task(TaskId id, string title, string description, Priority priority, DateTime? dueDate = null) 
        : base(id)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty", nameof(title));
            
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        Priority = priority ?? Priority.Medium;
        Status = TaskStatus.NotStarted;
        CreatedAt = DateTime.UtcNow;
        DueDate = dueDate;
    }
    
    public static Task Create(string title, string description, Priority priority, DateTime? dueDate = null)
    {
        return new Task(TaskId.New(), title, description, priority, dueDate);
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
    
    public void UpdatePriority(Priority newPriority)
    {
        Priority = newPriority ?? throw new ArgumentNullException(nameof(newPriority));
    }
    
    public void UpdateDueDate(DateTime? newDueDate)
    {
        DueDate = newDueDate;
    }
    
    public void Start()
    {
        if (Status == TaskStatus.Completed)
            throw new InvalidOperationException("Cannot start a completed task");
            
        Status = TaskStatus.InProgress;
    }
    
    public void MarkComplete()
    {
        if (Status == TaskStatus.Completed)
            return; // Already completed
            
        Status = TaskStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }
    
    public void Reopen()
    {
        if (Status != TaskStatus.Completed)
            throw new InvalidOperationException("Can only reopen completed tasks");
            
        Status = TaskStatus.InProgress;
        CompletedAt = null;
    }
    
    public bool IsOverdue => DueDate.HasValue && DueDate.Value < DateTime.UtcNow && Status != TaskStatus.Completed;
    
    public TimeSpan? TimeToCompletion => CompletedAt.HasValue ? CompletedAt.Value - CreatedAt : null;
    
    public int DaysUntilDue
    {
        get
        {
            if (!DueDate.HasValue) return int.MaxValue;
            return (int)(DueDate.Value - DateTime.UtcNow).TotalDays;
        }
    }
}
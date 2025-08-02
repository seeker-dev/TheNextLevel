namespace TheNextLevel.Domain.Tasks.ValueObjects;

public record TaskStatus(string Name, int Order)
{
    public static readonly TaskStatus NotStarted = new("Not Started", 1);
    public static readonly TaskStatus InProgress = new("In Progress", 2);
    public static readonly TaskStatus Completed = new("Completed", 3);
    
    public static TaskStatus FromString(string status) => status.ToLower() switch
    {
        "not started" or "notstarted" => NotStarted,
        "in progress" or "inprogress" => InProgress,
        "completed" => Completed,
        _ => NotStarted
    };
    
    public override string ToString() => Name;
}
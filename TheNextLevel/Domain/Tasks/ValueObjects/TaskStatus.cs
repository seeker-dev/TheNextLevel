namespace TheNextLevel.Domain.Tasks.ValueObjects;

public record TaskStatus(string Name, int Order)
{
    public static readonly TaskStatus NotCompleted = new("Not Completed", 1);
    public static readonly TaskStatus Completed = new("Completed", 2);
    
    public static TaskStatus FromString(string status) => status.ToLower() switch
    {
        "completed" => Completed,
        "not completed" or "notcompleted" => NotCompleted,
        _ => NotCompleted
    };
    
    public override string ToString() => Name;
}
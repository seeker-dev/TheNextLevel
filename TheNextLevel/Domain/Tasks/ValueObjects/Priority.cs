namespace TheNextLevel.Domain.Tasks.ValueObjects;

public record Priority(string Name, int Level, string Color)
{
    public static readonly Priority Low = new("Low", 1, "#28a745");
    public static readonly Priority Medium = new("Medium", 2, "#ffc107");
    public static readonly Priority High = new("High", 3, "#fd7e14");
    public static readonly Priority Critical = new("Critical", 4, "#dc3545");
    
    public static Priority FromString(string priority) => priority.ToLower() switch
    {
        "low" => Low,
        "medium" => Medium,
        "high" => High,
        "critical" => Critical,
        _ => Medium
    };
    
    public override string ToString() => Name;
}
namespace TheNextLevel.Domain.Tasks.ValueObjects;

public record TaskId(Guid Value)
{
    public static TaskId New() => new(Guid.NewGuid());
    
    public static implicit operator Guid(TaskId taskId) => taskId.Value;
    public static implicit operator TaskId(Guid guid) => new(guid);
    
    public override string ToString() => Value.ToString();
}
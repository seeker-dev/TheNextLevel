namespace TheNextLevel.Domain.Common;

public abstract class Entity<TId>
{
    public TId Id { get; protected set; } = default!;
    
    protected Entity() { }
    
    protected Entity(TId id)
    {
        Id = id;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
            return false;
            
        if (ReferenceEquals(this, other))
            return true;
            
        return Id?.Equals(other.Id) == true;
    }
    
    public override int GetHashCode()
    {
        return Id?.GetHashCode() ?? 0;
    }
    
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        return Equals(left, right);
    }
    
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)
    {
        return !Equals(left, right);
    }
}
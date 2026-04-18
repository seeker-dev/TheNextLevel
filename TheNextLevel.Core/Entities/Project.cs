namespace TheNextLevel.Core.Entities;

public class Project
{
    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int MissionId { get; private set; }
    public bool IsCompleted { get; private set; }

    public Project(int id, int accountId, string name, string? description, int missionId, bool isCompleted = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Id = id;
        AccountId = accountId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        MissionId = missionId;
        IsCompleted = isCompleted;
    }
}
namespace TheNextLevel.Core.Entities;

public class Project
{
    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public int MissionId { get; private set; }

    public Project(int id, int accountId, string name, string? description, int missionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));

        Id = id;
        AccountId = accountId;
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        MissionId = missionId;
    }
    
    public void UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Project name cannot be empty", nameof(newName));
            
        Name = newName.Trim();
    }
    
    public void UpdateDescription(string newDescription)
    {
        Description = newDescription?.Trim() ?? string.Empty;
    }
}
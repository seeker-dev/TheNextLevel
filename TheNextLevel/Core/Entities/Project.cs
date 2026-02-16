namespace TheNextLevel.Core.Entities;

public class Project
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation property for related tasks
    public ICollection<Task> Tasks { get; set; } = new List<Task>();

    public Project()
    {
    }

    public Project(string name, string description) : this()
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Project name cannot be empty", nameof(name));
            
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
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
namespace NextLevel5.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public int CompletionPercentage { get; set; }
    public List<Task> Tasks { get; set; }
}
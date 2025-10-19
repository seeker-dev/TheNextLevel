namespace TheNextLevel.Application.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<TaskDto> Tasks { get; set; } = [];

    public ProjectDto(int id, string name, string description, IEnumerable<Core.Entities.Task> tasks)
    {
        Id = id;
        Name = name;
        Description = description;
        Tasks = tasks.Select(t => new TaskDto(
            t.Id,
            t.Name,
            t.Description ?? string.Empty,
            t.IsCompleted,
            t.ProjectId
        )).ToList();
    }
}
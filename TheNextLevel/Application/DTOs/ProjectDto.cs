namespace TheNextLevel.Application.DTOs;

public class ProjectDto
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<TaskDto> Tasks { get; set; } = [];

    public ProjectDto(int id, int accountId, string name, string description, IEnumerable<Core.Entities.Task> tasks)
    {
        Id = id;
        AccountId = accountId;
        Name = name;
        Description = description;
        Tasks = tasks.Select(t => new TaskDto(
            t.Id,
            t.AccountId,
            t.Name,
            t.Description ?? string.Empty,
            t.IsCompleted,
            t.ProjectId
        )).ToList();
    }
}
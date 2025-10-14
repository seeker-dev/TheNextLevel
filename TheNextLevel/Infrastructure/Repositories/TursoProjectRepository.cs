using System.Text.Json;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories;

public class TursoProjectRepository : IProjectRepository
{
    private readonly TursoClient _client;
    private readonly ITaskRepository _taskRepository;

    public TursoProjectRepository(TursoClient client, ITaskRepository taskRepository)
    {
        _client = client;
        _taskRepository = taskRepository;
    }

    public async Task<IEnumerable<Project>> GetAllAsync()
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description, CreatedAt FROM Projects");

        var projects = MapToProjects(response).ToList();

        // Load tasks for each project
        foreach (var project in projects)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            project.Tasks = tasks.ToList();
        }

        return projects;
    }

    public async Task<Project?> GetByIdAsync(Guid id)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description, CreatedAt FROM Projects WHERE Id = ?",
            id.ToString());

        var project = MapToProjects(response).FirstOrDefault();

        if (project != null)
        {
            // Load tasks for this project
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            project.Tasks = tasks.ToList();
        }

        return project;
    }

    public async Task<Project> AddAsync(Project project)
    {
        await _client.ExecuteAsync(
            "INSERT INTO Projects (Id, Name, Description, CreatedAt) VALUES (?, ?, ?, ?)",
            project.Id.ToString(),
            project.Name,
            project.Description ?? string.Empty,
            project.CreatedAt.ToString("o"));

        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        await _client.ExecuteAsync(
            "UPDATE Projects SET Name = ?, Description = ? WHERE Id = ?",
            project.Name,
            project.Description ?? string.Empty,
            project.Id.ToString());

        return project;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Projects WHERE Id = ?",
            id.ToString());

        return response.Results?.AffectedRowCount > 0;
    }

    private IEnumerable<Project> MapToProjects(TursoResponse response)
    {
        if (response.Results?.Rows == null)
            return Enumerable.Empty<Project>();

        var projects = new List<Project>();
        var columns = response.Results.Columns;

        foreach (var row in response.Results.Rows)
        {
            var project = new Project
            {
                Id = Guid.Parse(GetColumnValue(row, columns, "Id")),
                Name = GetColumnValue(row, columns, "Name"),
                Description = GetColumnValue(row, columns, "Description"),
                CreatedAt = DateTime.Parse(GetColumnValue(row, columns, "CreatedAt"))
            };
            projects.Add(project);
        }

        return projects;
    }

    private string GetColumnValue(JsonElement[] row, string[] columns, string columnName)
    {
        var index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= row.Length)
            return string.Empty;

        var element = row[index];

        if (element.ValueKind == JsonValueKind.Null)
            return string.Empty;

        if (element.ValueKind == JsonValueKind.String)
            return element.GetString() ?? string.Empty;

        if (element.ValueKind == JsonValueKind.Number)
            return element.GetInt32().ToString();

        return element.ToString();
    }
}

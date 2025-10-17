using System.Text.Json;
using TheNextLevel.Application.DTOs;
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

    public async Task<IEnumerable<Project>> GetAllAsync(bool includeTasks = false)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description FROM Projects");

        var projects = MapToProjects(response).ToList();

        // Load tasks for each project
        if (includeTasks)
        {
            foreach (var project in projects)
            {
                var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
                project.Tasks = tasks.ToList();
            }
        }

        return projects;
    }
    
    public async Task<int> GetTotalProjectsCountAsync()
    {
        var response = await _client.QueryAsync(
            "SELECT COUNT(*) as TotalCount FROM Projects");
        if (response.Results?.Rows == null || response.Results.Rows.Length == 0)
            return 0;

        return int.Parse(GetColumnValue(response.Results.Rows[0], response.Results.Columns, "TotalCount"));
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description FROM Projects WHERE Id = ?",
            id);

        var project = MapToProjects(response).FirstOrDefault();

        if (project != null)
        {
            // Load tasks for this project
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            project.Tasks = tasks.ToList();
        }

        return project;
    }

    public async Task<IEnumerable<Project>> GetAsync(int startIndex, int count, bool includeTasks = false)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description FROM Projects LIMIT ? OFFSET ?",
            count,
            startIndex);

        var projects = MapToProjects(response).ToList();
        if (!includeTasks) return projects;

        // Load tasks for each project
        foreach (var project in projects)
        {
            var tasks = await _taskRepository.GetTasksByProjectIdAsync(project.Id);
            project.Tasks = tasks.ToList();
        }

        return projects;
    }

    public async Task<Project> AddAsync(Project project)
    {
        await _client.ExecuteAsync(
            "INSERT INTO Projects (Name, Description) VALUES (?, ?)",
            project.Name,
            project.Description ?? string.Empty);

        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        await _client.ExecuteAsync(
            "UPDATE Projects SET Name = ?, Description = ? WHERE Id = ?",
            project.Name,
            project.Description ?? string.Empty,
            project.Id);

        return project;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Projects WHERE Id = ?",
            id);

        return response.Results?.AffectedRowCount > 0;
    }

    public async Task<PagedResult<Project>> GetPagedAsync(int skip, int take)
    {
        // Get total count
        var totalCount = await GetTotalProjectsCountAsync();

        // Get paged data
        var response = await _client.QueryAsync(
            "SELECT Id, Name, Description FROM Projects LIMIT ? OFFSET ?",
            take,
            skip);

        var items = MapToProjects(response);

        return new PagedResult<Project>
        {
            Items = items,
            TotalCount = totalCount
        };
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
                Id = int.Parse(GetColumnValue(row, columns, "Id")),
                Name = GetColumnValue(row, columns, "Name"),
                Description = GetColumnValue(row, columns, "Description")
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

using System.Text.Json;
using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories;

public class TursoTaskRepository : ITaskRepository
{
    private readonly TursoClient _client;

    public TursoTaskRepository(TursoClient client)
    {
        _client = client;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetAllAsync()
    {
        var response = await _client.QueryAsync("SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks");
        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task?> GetByIdAsync(int id)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE Id = ?",
            id);

        var tasks = MapToTasks(response);
        return tasks.FirstOrDefault();
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
    {
        await _client.ExecuteAsync(
            "INSERT INTO Tasks (Title, Description, IsCompleted, ProjectId) VALUES (?, ?, ?, ?)",
            task.Title,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.ProjectId.HasValue ? (object)task.ProjectId.Value : DBNull.Value);

        return task;
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> UpdateAsync(Core.Entities.Task task)
    {
        await _client.ExecuteAsync(
            "UPDATE Tasks SET Title = ?, Description = ?, IsCompleted = ?, ProjectId = ? WHERE Id = ?",
            task.Title,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.ProjectId.HasValue ? (object)task.ProjectId.Value : DBNull.Value,
            task.Id);

        return task;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Tasks WHERE Id = ?",
            id);

        return response.Results?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetByStatusAsync(bool isCompleted)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE IsCompleted = ?",
            isCompleted ? 1 : 0);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdAsync(int projectId)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE ProjectId = ?",
            projectId);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetUngroupedTasksAsync()
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE ProjectId IS NULL");

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<PagedResult<Core.Entities.Task>> GetPagedAsync(int skip, int take, bool isCompleted = false)
    {
        // Get total count
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE IsCompleted = ?",
            isCompleted ? 1 : 0);
        var totalCount = 0;

        if (countResponse.Results?.Rows != null && countResponse.Results.Rows.Length > 0)
        {
            var countValue = countResponse.Results.Rows[0][0];
            totalCount = countValue.ValueKind == JsonValueKind.Number
                ? countValue.GetInt32()
                : 0;
        }

        // Get paged data
        var dataResponse = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE IsCompleted = ? ORDER BY Title desc LIMIT ? OFFSET ?",
            isCompleted ? 1 : 0,
            take,
            skip);

        var items = MapToTasks(dataResponse);

        return new PagedResult<Core.Entities.Task>
        {
            Items = items,
            TotalCount = totalCount
        };
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdsAsync(IEnumerable<int> projectIds)
    {
        var projectIdList = projectIds.ToList();

        if (!projectIdList.Any())
            return Enumerable.Empty<Core.Entities.Task>();

        // Build dynamic query with placeholders for IN clause
        var placeholders = string.Join(",", projectIdList.Select(_ => "?"));
        var query = $"SELECT Id, Title, Description, IsCompleted, ProjectId FROM Tasks WHERE ProjectId IN ({placeholders})";

        // Execute query with project IDs as parameters
        var response = await _client.QueryAsync(query, projectIdList.Cast<object>().ToArray());

        return MapToTasks(response);
    }

    private IEnumerable<Core.Entities.Task> MapToTasks(TursoResponse response)
    {
        if (response.Results?.Rows == null)
            return Enumerable.Empty<Core.Entities.Task>();

        var tasks = new List<Core.Entities.Task>();
        var columns = response.Results.Columns;

        foreach (var row in response.Results.Rows)
        {
            var task = new Core.Entities.Task
            {
                Id = int.Parse(GetColumnValue(row, columns, "Id")),
                Title = GetColumnValue(row, columns, "Title"),
                Description = GetColumnValue(row, columns, "Description"),
                IsCompleted = GetColumnValue(row, columns, "IsCompleted") == "1",
                ProjectId = ParseNullableInt(GetColumnValue(row, columns, "ProjectId"))
            };
            tasks.Add(task);
        }

        return tasks;
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

    private int? ParseNullableInt(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }
}

using System.Text.Json;
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
        var response = await _client.QueryAsync("SELECT Id, Title, Description, IsCompleted, CreatedAt, ProjectId FROM Tasks");
        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task?> GetByIdAsync(Guid id)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, CreatedAt, ProjectId FROM Tasks WHERE Id = ?",
            id.ToString());

        var tasks = MapToTasks(response);
        return tasks.FirstOrDefault();
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
    {
        await _client.ExecuteAsync(
            "INSERT INTO Tasks (Id, Title, Description, IsCompleted, CreatedAt, ProjectId) VALUES (?, ?, ?, ?, ?, ?)",
            task.Id.ToString(),
            task.Title,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.CreatedAt.ToString("o"),
            task.ProjectId?.ToString() ?? (object)DBNull.Value);

        return task;
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> UpdateAsync(Core.Entities.Task task)
    {
        await _client.ExecuteAsync(
            "UPDATE Tasks SET Title = ?, Description = ?, IsCompleted = ?, ProjectId = ? WHERE Id = ?",
            task.Title,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.ProjectId?.ToString() ?? (object)DBNull.Value,
            task.Id.ToString());

        return task;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(Guid id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Tasks WHERE Id = ?",
            id.ToString());

        return response.Results?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetByStatusAsync(bool isCompleted)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, CreatedAt, ProjectId FROM Tasks WHERE IsCompleted = ?",
            isCompleted ? 1 : 0);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdAsync(Guid projectId)
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, CreatedAt, ProjectId FROM Tasks WHERE ProjectId = ?",
            projectId.ToString());

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetUngroupedTasksAsync()
    {
        var response = await _client.QueryAsync(
            "SELECT Id, Title, Description, IsCompleted, CreatedAt, ProjectId FROM Tasks WHERE ProjectId IS NULL");

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
                Id = Guid.Parse(GetColumnValue(row, columns, "Id")),
                Title = GetColumnValue(row, columns, "Title"),
                Description = GetColumnValue(row, columns, "Description"),
                IsCompleted = GetColumnValue(row, columns, "IsCompleted") == "1",
                CreatedAt = DateTime.Parse(GetColumnValue(row, columns, "CreatedAt")),
                ProjectId = ParseNullableGuid(GetColumnValue(row, columns, "ProjectId"))
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

    private Guid? ParseNullableGuid(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return Guid.TryParse(value, out var guid) ? guid : null;
    }
}

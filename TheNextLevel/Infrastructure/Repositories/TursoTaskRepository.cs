using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories;

public class TursoTaskRepository : ITaskRepository
{
    private readonly TursoClient _client;
    private readonly IAccountContext _accountContext;

    public TursoTaskRepository(TursoClient client, IAccountContext accountContext)
    {
        _client = client;
        _accountContext = accountContext;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetAllAsync()
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE AccountId = ?",
            accountId);
        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE Id = ? AND AccountId = ?",
            id, accountId);

        var tasks = MapToTasks(response);
        return tasks.FirstOrDefault();
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(Core.Entities.Task task)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "INSERT INTO Tasks (AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId) VALUES (?, ?, ?, ?, ?, ?)",
            accountId,
            task.Name,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.ProjectId.HasValue ? (object)task.ProjectId.Value : null,
            task.ParentTaskId.HasValue ? (object)task.ParentTaskId.Value : null);

        // Set the database-generated ID
        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var id))
        {
            task.Id = id;
        }

        return task;
    }

    public async System.Threading.Tasks.Task<Core.Entities.Task> UpdateAsync(Core.Entities.Task task)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Tasks SET Name = ?, Description = ?, IsCompleted = ?, ProjectId = ?, ParentTaskId = ? WHERE Id = ? AND AccountId = ?",
            task.Name,
            task.Description,
            task.IsCompleted ? 1 : 0,
            task.ProjectId.HasValue ? (object)task.ProjectId.Value : null,
            task.ParentTaskId.HasValue ? (object)task.ParentTaskId.Value : null,
            task.Id,
            accountId);

        return task;
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Tasks WHERE Id = ?",
            id);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetByStatusAsync(bool isCompleted)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE IsCompleted = ? AND AccountId = ? AND ParentTaskId IS NULL",
            isCompleted ? 1 : 0, accountId);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetTasksByProjectIdAsync(int projectId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId = ? AND AccountId = ?",
            projectId, accountId);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetUngroupedTasksAsync()
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId IS NULL AND AccountId = ?",
            accountId);

        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<PagedResult<Core.Entities.Task>> GetPagedAsync(int skip, int take, bool isCompleted = false)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE IsCompleted = ? AND AccountId = ? AND ParentTaskId IS NULL",
            isCompleted ? 1 : 0, accountId);
        var totalCount = 0;

        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countValue = countResponse.Result.Rows[0][0];
            totalCount = countValue.GetInt32Value();
        }

        // Get paged data
        var dataResponse = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE IsCompleted = ? AND AccountId = ? AND ParentTaskId IS NULL ORDER BY Name desc LIMIT ? OFFSET ?",
            isCompleted ? 1 : 0,
            accountId,
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
        var accountId = _accountContext.GetCurrentAccountId();
        var projectIdList = projectIds.ToList();

        if (!projectIdList.Any())
            return Enumerable.Empty<Core.Entities.Task>();

        // Build dynamic query with placeholders for IN clause
        var placeholders = string.Join(",", projectIdList.Select(_ => "?"));
        var query = $"SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId IN ({placeholders}) AND AccountId = ?";

        // Execute query with project IDs as parameters
        var parameters = projectIdList.Cast<object>().ToList();
        parameters.Add(accountId);
        var response = await _client.QueryAsync(query, parameters.ToArray());

        return MapToTasks(response);
    }

    private IEnumerable<Core.Entities.Task> MapToTasks(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<Core.Entities.Task>();

        var tasks = new List<Core.Entities.Task>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            var task = new Core.Entities.Task
            {
                Id = int.Parse(GetColumnValue(row, columns, "Id")),
                AccountId = int.Parse(GetColumnValue(row, columns, "AccountId")),
                Name = GetColumnValue(row, columns, "Name"),
                Description = GetColumnValue(row, columns, "Description"),
                IsCompleted = GetColumnValue(row, columns, "IsCompleted") == "1",
                ProjectId = ParseNullableInt(GetColumnValue(row, columns, "ProjectId")),
                ParentTaskId = ParseNullableInt(GetColumnValue(row, columns, "ParentTaskId"))
            };
            tasks.Add(task);
        }

        return tasks;
    }

    private string GetColumnValue(TursoValue[] row, string[] columns, string columnName)
    {
        var index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= row.Length)
            return string.Empty;

        return row[index].GetStringValue();
    }

    private int? ParseNullableInt(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetSubtasksByParentIdAsync(int parentTaskId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ParentTaskId = ? AND AccountId = ?",
            parentTaskId, accountId);
        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<IEnumerable<Core.Entities.Task>> GetRootTasksAsync()
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ParentTaskId IS NULL AND AccountId = ?",
            accountId);
        return MapToTasks(response);
    }

    public async System.Threading.Tasks.Task<int> GetSubtaskCountAsync(int parentTaskId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE ParentTaskId = ? AND AccountId = ?",
            parentTaskId, accountId);

        if (response.Result?.Rows != null && response.Result.Rows.Length > 0)
        {
            var countValue = response.Result.Rows[0][0];
            return countValue.GetInt32Value();
        }

        return 0;
    }
}

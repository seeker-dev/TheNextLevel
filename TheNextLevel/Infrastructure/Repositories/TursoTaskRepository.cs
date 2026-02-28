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

    public async System.Threading.Tasks.Task<Core.Entities.Task> AddAsync(string name, string description, int? projectId = null, int? parentTaskId = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim() ?? string.Empty;

        var response = await _client.ExecuteAsync(
            "INSERT INTO Tasks (AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId) VALUES (?, ?, ?, ?, ?, ?)",
            accountId,
            trimmedName,
            trimmedDescription,
            0,
            projectId.HasValue ? (object)projectId.Value : null,
            parentTaskId.HasValue ? (object)parentTaskId.Value : null);

        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var id))
            return new Core.Entities.Task(id, accountId, trimmedName, trimmedDescription, false, projectId, parentTaskId);

        throw new InvalidOperationException("Failed to create task.");
    }

    public async System.Threading.Tasks.Task<bool> UpdateAsync(int id, string name, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET Name = ?, Description = ? WHERE Id = ? AND AccountId = ?",
            name.Trim(),
            description?.Trim() ?? string.Empty,
            id,
            accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<bool> CompleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 1 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<bool> ReopenAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 0 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<bool> AssignToProjectAsync(int id, int projectId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET ProjectId = ? WHERE Id = ? AND AccountId = ?",
            projectId,
            id,
            accountId);

        return response.Result?.AffectedRowCount > 0;
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
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId = ? AND AccountId = ? AND ParentTaskId IS NULL",
            projectId, accountId);

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

    public async System.Threading.Tasks.Task<PagedResult<Core.Entities.Task>> GetPagedByProjectIdAsync(int projectId, int skip, int take, bool isCompleted = false)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE ProjectId = ? AND IsCompleted = ? AND AccountId = ? AND ParentTaskId IS NULL",
            projectId, isCompleted ? 1 : 0, accountId);
        var totalCount = 0;

        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countValue = countResponse.Result.Rows[0][0];
            totalCount = countValue.GetInt32Value();
        }

        // Get paged data
        var dataResponse = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId = ? AND IsCompleted = ? AND AccountId = ? AND ParentTaskId IS NULL ORDER BY Name desc LIMIT ? OFFSET ?",
            projectId,
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

    private IEnumerable<Core.Entities.Task> MapToTasks(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<Core.Entities.Task>();

        var tasks = new List<Core.Entities.Task>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            tasks.Add(new Core.Entities.Task(
                id: int.Parse(GetColumnValue(row, columns, "Id")),
                accountId: int.Parse(GetColumnValue(row, columns, "AccountId")),
                name: GetColumnValue(row, columns, "Name"),
                description: GetColumnValue(row, columns, "Description"),
                isCompleted: GetColumnValue(row, columns, "IsCompleted") == "1",
                projectId: ParseNullableInt(GetColumnValue(row, columns, "ProjectId")),
                parentTaskId: ParseNullableInt(GetColumnValue(row, columns, "ParentTaskId"))
            ));
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

    public async System.Threading.Tasks.Task<PagedResult<Core.Entities.Task>> GetSubtasksByParentIdAsync(int parentTaskId, int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE ParentTaskId = ? AND AccountId = ?",
            parentTaskId, accountId);
        var totalCount = 0;

        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countValue = countResponse.Result.Rows[0][0];
            totalCount = countValue.GetInt32Value();
        }

        var dataResponse = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ParentTaskId FROM Tasks WHERE ParentTaskId = ? AND AccountId = ? ORDER BY Name desc LIMIT ? OFFSET ?",
            parentTaskId, accountId, take, skip);

        var items = MapToTasks(dataResponse);

        return new PagedResult<Core.Entities.Task>
        {
            Items = items,
            TotalCount = totalCount
        };
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

    public async System.Threading.Tasks.Task<int> BulkCompleteSubtasksAsync(int parentTaskId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 1 WHERE ParentTaskId = ? AND AccountId = ? AND IsCompleted = 0",
            parentTaskId, accountId);

        return response.Result?.AffectedRowCount ?? 0;
    }
}

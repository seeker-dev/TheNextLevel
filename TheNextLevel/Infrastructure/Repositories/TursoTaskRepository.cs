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

    public async Task<Core.Entities.Task?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE Id = ? AND AccountId = ?",
            id, accountId);

        var tasks = MapToTasks(response);
        return tasks.FirstOrDefault();
    }

    public async Task<Core.Entities.Task> CreateAsync(int projectId, string name, string description)
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
            projectId,
            DBNull.Value);

        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var id))
            return new Core.Entities.Task(id, accountId, trimmedName, trimmedDescription, false, projectId, null);

        throw new InvalidOperationException("Failed to create task.");
    }

    public async Task<bool> UpdateAsync(int id, string name, string description)
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

    public async Task<bool> CompleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 1 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<bool> ResetAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 0 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task MoveAsync(int taskId, int newProjectId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET ProjectId = ? WHERE Id = ? AND AccountId = ?",
            newProjectId, taskId, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Tasks WHERE Id = ? AND AccountId = ?",
            id,
            _accountContext.GetCurrentAccountId());

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<PagedResult<Core.Entities.Task>> ListAsync(int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE AccountId = ? AND ParentTaskId IS NULL",
            accountId);
        var totalCount = 0;

        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countValue = countResponse.Result.Rows[0][0];
            totalCount = countValue.GetInt32Value();
        }

        // Get paged data
        var dataResponse = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE AccountId = ? AND ParentTaskId IS NULL ORDER BY Name desc LIMIT ? OFFSET ?",
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

    public async Task<PagedResult<Core.Entities.Task>> ListByProjectIdAsync(int projectId, int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count
        var countResponse = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE ProjectId = ? AND AccountId = ? AND ParentTaskId IS NULL",
            projectId, accountId);
        var totalCount = 0;

        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countValue = countResponse.Result.Rows[0][0];
            totalCount = countValue.GetInt32Value();
        }

        // Get paged data
        var dataResponse = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, IsCompleted, ProjectId, ParentTaskId FROM Tasks WHERE ProjectId = ? AND AccountId = ? AND ParentTaskId IS NULL ORDER BY Name desc LIMIT ? OFFSET ?",
            projectId,
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

    public async Task<PagedResult<Core.Entities.Task>> ListSubtasksByParentIdAsync(int parentId, int skip, int take)
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

    public async Task<Core.Entities.Task> CreateSubtaskAsync(int parentId, string name, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var trimmedName = name.Trim();
        var trimmedDescription = description?.Trim() ?? string.Empty;

        var response = await _client.ExecuteAsync(
            "INSERT INTO Tasks (AccountId, Name, Description, IsCompleted, ParentTaskId) VALUES (?, ?, ?, ?, ?)",
            accountId,
            trimmedName,
            trimmedDescription,
            0,
            parentId);

        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var id))
            return new Core.Entities.Task(id, accountId, trimmedName, trimmedDescription, false, null, parentId);

        throw new InvalidOperationException("Failed to create subtask.");
    }

    public async Task<bool> UpdateSubtaskAsync(int id, int parentId, string name, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET Name = ?, Description = ? WHERE Id = ? AND ParentTaskId = ? AND AccountId = ?",
            name.Trim(),
            description?.Trim() ?? string.Empty,
            id,
            parentId,
            accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<bool> DeleteSubtaskAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Tasks WHERE Id = ? AND AccountId = ?",
            id,
            _accountContext.GetCurrentAccountId());

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<int> CountSubtasksAsync(int parentId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT COUNT(*) as Count FROM Tasks WHERE ParentTaskId = ? AND AccountId = ?",
            parentId, accountId);

        if (response.Result?.Rows != null && response.Result.Rows.Length > 0)
        {
            var countValue = response.Result.Rows[0][0];
            return countValue.GetInt32Value();
        }

        return 0;
    }

    public async Task<int> BulkCompleteSubtasksAsync(int parentId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Tasks SET IsCompleted = 1 WHERE ParentTaskId = ? AND AccountId = ? AND IsCompleted = 0",
            parentId, accountId);

        return response.Result?.AffectedRowCount ?? 0;
    }

    public async Task MoveSubtaskAsync(int taskId, int newParentId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Tasks SET ParentTaskId = ? WHERE Id = ? AND AccountId = ? AND ParentTaskId IS NOT NULL",
            newParentId, taskId, accountId);

    }
}

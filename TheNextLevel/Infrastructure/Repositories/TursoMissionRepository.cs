using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;
using EntityTask = TheNextLevel.Core.Entities.Task;

namespace TheNextLevel.Infrastructure.Repositories;

public class TursoMissionRepository : IMissionRepository
{
    private readonly TursoClient _client;
    private readonly IAccountContext _accountContext;

    public TursoMissionRepository(TursoClient client, IAccountContext accountContext)
    {
        _client = client;
        _accountContext = accountContext;
    }

    public async System.Threading.Tasks.Task<Mission?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return MapToMissions(response).FirstOrDefault();
    }

    public async System.Threading.Tasks.Task<PagedResult<Mission>> ListAsync(int skip, int take, string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Missions WHERE AccountId = ? AND Title LIKE ?";
            dataQuery = "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE AccountId = ? AND Title LIKE ? ORDER BY Title LIMIT ? OFFSET ?";
            countParams = new object[] { accountId, $"%{filterText}%" };
            dataParams = new object[] { accountId, $"%{filterText}%", take, skip };
        }
        else
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Missions WHERE AccountId = ?";
            dataQuery = "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE AccountId = ? ORDER BY Title LIMIT ? OFFSET ?";
            countParams = new object[] { accountId };
            dataParams = new object[] { accountId, take, skip };
        }

        var countResponse = await _client.QueryAsync(countQuery, countParams);
        var totalCount = 0;
        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countColumns = countResponse.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
            totalCount = int.Parse(GetColumnValue(countResponse.Result.Rows[0], countColumns, "TotalCount"));
        }

        var dataResponse = await _client.QueryAsync(dataQuery, dataParams);

        return new PagedResult<Mission>
        {
            Items = MapToMissions(dataResponse).ToList(),
            TotalCount = totalCount
        };
    }

    public async System.Threading.Tasks.Task<Mission> AddAsync(string title, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "INSERT INTO Missions (AccountId, Title, Description, IsCompleted) VALUES (?, ?, ?, 0)",
            accountId,
            title.Trim(),
            description?.Trim() ?? string.Empty);

        var id = 0;
        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var insertedId))
            id = insertedId;

        return new Mission(id, accountId, title, description);
    }

    public async System.Threading.Tasks.Task<Mission> UpdateAsync(int id, string title, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Missions SET Title = ?, Description = ? WHERE Id = ? AND AccountId = ?",
            title.Trim(),
            description?.Trim() ?? string.Empty,
            id,
            accountId);

        return await GetByIdAsync(id) ?? throw new InvalidOperationException($"Mission {id} not found after update.");
    }

    public async System.Threading.Tasks.Task<bool> DeleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "DELETE FROM Missions WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<bool> CompleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Missions SET IsCompleted = 1 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<bool> ResetAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Missions SET IsCompleted = 0 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async System.Threading.Tasks.Task<PagedResult<Project>> ListProjectsAsync(int id, int skip, int take, string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId = ? AND AccountId = ? AND Name LIKE ?";
            dataQuery = "SELECT Id, AccountId, Name, Description FROM Projects WHERE MissionId = ? AND AccountId = ? AND Name LIKE ? ORDER BY Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId, $"%{filterText}%" };
            dataParams = new object[] { id, accountId, $"%{filterText}%", take, skip };
        }
        else
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId = ? AND AccountId = ?";
            dataQuery = "SELECT Id, AccountId, Name, Description FROM Projects WHERE MissionId = ? AND AccountId = ? ORDER BY Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId };
            dataParams = new object[] { id, accountId, take, skip };
        }

        var countResponse = await _client.QueryAsync(countQuery, countParams);
        var totalCount = 0;
        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countColumns = countResponse.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
            totalCount = int.Parse(GetColumnValue(countResponse.Result.Rows[0], countColumns, "TotalCount"));
        }

        var dataResponse = await _client.QueryAsync(dataQuery, dataParams);

        return new PagedResult<Project>
        {
            Items = MapToProjects(dataResponse).ToList(),
            TotalCount = totalCount
        };
    }
    
    public async System.Threading.Tasks.Task<PagedResult<EligibleProjectProjection>> ListEligibleProjectsAsync(int id, int skip, int take, string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId != ? AND AccountId = ? AND Name LIKE ?";
            dataQuery = "SELECT p.Id, p.AccountId, p.Name, p.Description, m.Title FROM Projects p INNER JOIN Missions m ON p.MissionId = m.Id WHERE p.MissionId != ? AND p.AccountId = ? AND p.Name LIKE ? ORDER BY p.Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId, $"%{filterText}%" };
            dataParams = new object[] { id, accountId, $"%{filterText}%", take, skip };
        }
        else
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId != ? AND AccountId = ?";
            dataQuery = "SELECT p.Id, p.AccountId, p.Name, p.Description, m.Title FROM Projects p INNER JOIN Missions m ON p.MissionId = m.Id WHERE p.MissionId != ? AND p.AccountId = ? ORDER BY p.Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId };
            dataParams = new object[] { id, accountId, take, skip };
        }

        var countResponse = await _client.QueryAsync(countQuery, countParams);
        var totalCount = 0;
        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countColumns = countResponse.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
            totalCount = int.Parse(GetColumnValue(countResponse.Result.Rows[0], countColumns, "TotalCount"));
        }

        var dataResponse = await _client.QueryAsync(dataQuery, dataParams);

        return new PagedResult<EligibleProjectProjection>
        {
            Items = MapToEligibleProjectProjections(dataResponse).ToList(),
            TotalCount = totalCount
        };
    }

    public async System.Threading.Tasks.Task<PagedResult<EntityTask>> ListTasksAsync(int id, int skip, int take, string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Tasks t INNER JOIN Projects p ON t.ProjectId = p.Id WHERE p.MissionId = ? AND t.AccountId = ? AND t.Name LIKE ?";
            dataQuery = "SELECT t.Id, t.AccountId, t.Name, t.Description, t.IsCompleted, t.ProjectId, t.ParentTaskId FROM Tasks t INNER JOIN Projects p ON t.ProjectId = p.Id WHERE p.MissionId = ? AND t.AccountId = ? AND t.Name LIKE ? ORDER BY t.Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId, $"%{filterText}%" };
            dataParams = new object[] { id, accountId, $"%{filterText}%", take, skip };
        }
        else
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Tasks t INNER JOIN Projects p ON t.ProjectId = p.Id WHERE p.MissionId = ? AND t.AccountId = ?";
            dataQuery = "SELECT t.Id, t.AccountId, t.Name, t.Description, t.IsCompleted, t.ProjectId, t.ParentTaskId FROM Tasks t INNER JOIN Projects p ON t.ProjectId = p.Id WHERE p.MissionId = ? AND t.AccountId = ? ORDER BY t.Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId };
            dataParams = new object[] { id, accountId, take, skip };
        }

        var countResponse = await _client.QueryAsync(countQuery, countParams);
        var totalCount = 0;
        if (countResponse.Result?.Rows != null && countResponse.Result.Rows.Length > 0)
        {
            var countColumns = countResponse.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
            totalCount = int.Parse(GetColumnValue(countResponse.Result.Rows[0], countColumns, "TotalCount"));
        }

        var dataResponse = await _client.QueryAsync(dataQuery, dataParams);

        return new PagedResult<EntityTask>
        {
            Items = MapToTasks(dataResponse).ToList(),
            TotalCount = totalCount
        };
    }

    public async System.Threading.Tasks.Task AddToMissionAsync(int id, int projectId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Projects SET MissionId = ? WHERE Id = ? AND AccountId = ?",
            id, projectId, accountId);
    }

    private IEnumerable<Project> MapToProjects(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<Project>();

        var projects = new List<Project>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            projects.Add(new Project(
                int.Parse(GetColumnValue(row, columns, "Id")),
                int.Parse(GetColumnValue(row, columns, "AccountId")),
                GetColumnValue(row, columns, "Name"),
                GetColumnValue(row, columns, "Description"),
                int.Parse(GetColumnValue(row, columns, "MissionId"))
            ));
        }

        return projects;
    }

    private IEnumerable<EntityTask> MapToTasks(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<EntityTask>();

        var tasks = new List<EntityTask>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            tasks.Add(new EntityTask(
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

    private int? ParseNullableInt(string value)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }

    private IEnumerable<Mission> MapToMissions(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<Mission>();

        var missions = new List<Mission>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            var mission = new Mission(
                id: int.Parse(GetColumnValue(row, columns, "Id")),
                accountId: int.Parse(GetColumnValue(row, columns, "AccountId")),
                title: GetColumnValue(row, columns, "Title"),
                description: GetColumnValue(row, columns, "Description"));

            if (GetColumnValue(row, columns, "IsCompleted") == "1")
                mission.Complete();

            missions.Add(mission);
        }

        return missions;
    }

    private IEnumerable<EligibleProjectProjection> MapToEligibleProjectProjections(TursoResponse response)
    {
        if (response.Result?.Rows == null)
            return Enumerable.Empty<EligibleProjectProjection>();

        var projections = new List<EligibleProjectProjection>();
        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();

        foreach (var row in response.Result.Rows)
        {
            projections.Add(new EligibleProjectProjection(
                Id: int.Parse(GetColumnValue(row, columns, "Id")),
                AccountId: int.Parse(GetColumnValue(row, columns, "AccountId")),
                Name: GetColumnValue(row, columns, "Name"),
                Description: GetColumnValue(row, columns, "Description"),
                MissionTitle: GetColumnValue(row, columns, "Title")
            ));
        }

        return projections;
    }

    private string GetColumnValue(TursoValue[] row, string[] columns, string columnName)
    {
        var index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= row.Length)
            return string.Empty;

        return row[index].GetStringValue();
    }
}

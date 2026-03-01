using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

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

    public async Task<Mission?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return MapToMissions(response).FirstOrDefault();
    }

    public async Task<PagedResult<Mission>> ListAsync(int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        /*if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Missions WHERE AccountId = ? AND Title LIKE ?";
            dataQuery = "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE AccountId = ? AND Title LIKE ? ORDER BY Title LIMIT ? OFFSET ?";
            countParams = new object[] { accountId, $"%{filterText}%" };
            dataParams = new object[] { accountId, $"%{filterText}%", take, skip };
        }
        else
        {*/
            countQuery = "SELECT COUNT(*) as TotalCount FROM Missions WHERE AccountId = ?";
            dataQuery = "SELECT Id, AccountId, Title, Description, IsCompleted FROM Missions WHERE AccountId = ? ORDER BY Title LIMIT ? OFFSET ?";
            countParams = new object[] { accountId };
            dataParams = new object[] { accountId, take, skip };
        //}

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

    public async Task<Mission> CreateAsync(string title, string description)
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

        return new Mission(id, accountId, title, description, isCompleted: false);
    }

    public async Task<Mission> UpdateAsync(int id, string title, string description)
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

    public async Task<bool> DeleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "DELETE FROM Missions WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<bool> CompleteAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Missions SET IsCompleted = 1 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<bool> ResetAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "UPDATE Missions SET IsCompleted = 0 WHERE Id = ? AND AccountId = ?",
            id, accountId);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<PagedResult<Project>> ListProjectsAsync(int id, int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string countQuery;
        string dataQuery;
        object[] countParams;
        object[] dataParams;

        /*if (!string.IsNullOrWhiteSpace(filterText))
        {
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId = ? AND AccountId = ? AND Name LIKE ?";
            dataQuery = "SELECT Id, AccountId, Name, Description, MissionId FROM Projects WHERE MissionId = ? AND AccountId = ? AND Name LIKE ? ORDER BY Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId, $"%{filterText}%" };
            dataParams = new object[] { id, accountId, $"%{filterText}%", take, skip };
        }
        else
        {*/
            countQuery = "SELECT COUNT(*) as TotalCount FROM Projects WHERE MissionId = ? AND AccountId = ?";
            dataQuery = "SELECT Id, AccountId, Name, Description, MissionId FROM Projects WHERE MissionId = ? AND AccountId = ? ORDER BY Name LIMIT ? OFFSET ?";
            countParams = new object[] { id, accountId };
            dataParams = new object[] { id, accountId, take, skip };
        //}

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
                description: GetColumnValue(row, columns, "Description"),
                isCompleted: GetColumnValue(row, columns, "IsCompleted") == "1"
                );

            missions.Add(mission);
        }

        return missions;
    }

    private string GetColumnValue(TursoValue[] row, string[] columns, string columnName)
    {
        var index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= row.Length)
            return string.Empty;

        return row[index].GetStringValue();
    }
}

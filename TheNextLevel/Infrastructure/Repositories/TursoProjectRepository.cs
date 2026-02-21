using TheNextLevel.Core.DTOs;
using TheNextLevel.Core.Entities;
using TheNextLevel.Core.Interfaces;
using TheNextLevel.Infrastructure.Data;

namespace TheNextLevel.Infrastructure.Repositories;

public class TursoProjectRepository : IProjectRepository
{
    private readonly TursoClient _client;
    private readonly ITaskRepository _taskRepository;
    private readonly IAccountContext _accountContext;

    public TursoProjectRepository(TursoClient client, ITaskRepository taskRepository, IAccountContext accountContext)
    {
        _client = client;
        _taskRepository = taskRepository;
        _accountContext = accountContext;
    }

    public async Task<int> GetTotalProjectsCountAsync(string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        string query;
        object[] parameters;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            query = "SELECT COUNT(*) as TotalCount FROM Projects WHERE AccountId = ? AND Name LIKE ?";
            parameters = new object[] { accountId, $"%{filterText}%" };
        }
        else
        {
            query = "SELECT COUNT(*) as TotalCount FROM Projects WHERE AccountId = ?";
            parameters = new object[] { accountId };
        }

        var response = await _client.QueryAsync(query, parameters);
        if (response.Result?.Rows == null || response.Result.Rows.Length == 0)
            return 0;

        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
        return int.Parse(GetColumnValue(response.Result.Rows[0], columns, "TotalCount"));
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description, MissionId FROM Projects WHERE Id = ? AND AccountId = ?",
            id, accountId);

        var project = MapToProjects(response).FirstOrDefault();

        if (project != null)
        {
            // Load tasks for this project
            //var tasks = await _taskRepository.GetTasksByProjectIdsAsync(new[] { project.Id });
            //project.Tasks = tasks.ToList();
        }

        return project;
    }

    public async Task<Project> AddAsync(string name, string description, int missionId)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.ExecuteAsync(
            "INSERT INTO Projects (AccountId, Name, Description, MissionId) VALUES (?, ?, ?, ?)",
            accountId,
            name,
            description ?? string.Empty,
            missionId);

        // Set the database-generated ID
        if (response.Result?.LastInsertRowId != null && int.TryParse(response.Result.LastInsertRowId, out var id))
        {
            var project = new Project(id, accountId, name, description, missionId);
            return project;
        }

        throw new InvalidOperationException("Failed to create project.");
    }

    public async Task<Project?> UpdateAsync(int id, string name, string description)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Projects SET Name = ?, Description = ? WHERE Id = ? AND AccountId = ?",
            name.Trim(),
            description?.Trim() ?? string.Empty,
            id,
            accountId);

        return await GetByIdAsync(id);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Projects WHERE Id = ? AND AccountId = ?",
            id,
            _accountContext.GetCurrentAccountId());

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<PagedResult<Project>> GetPagedAsync(int skip, int take, string? filterText = null)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count with filter
        var totalCount = await GetTotalProjectsCountAsync(filterText);

        // Get paged data with filter
        string query;
        object[] parameters;

        if (!string.IsNullOrWhiteSpace(filterText))
        {
            query = "SELECT Id, AccountId, Name, Description, MissionId FROM Projects WHERE AccountId = ? AND Name LIKE ? ORDER BY Name desc LIMIT ? OFFSET ?";
            parameters = new object[] { accountId, $"%{filterText}%", take, skip };
        }
        else
        {
            query = "SELECT Id, AccountId, Name, Description, MissionId FROM Projects WHERE AccountId = ? ORDER BY Name desc LIMIT ? OFFSET ?";
            parameters = new object[] { accountId, take, skip };
        }

        var response = await _client.QueryAsync(query, parameters);

        var items = MapToProjects(response).ToList();

        if (items.Any())
        {
            /*
            // Batch load tasks for all projects
            var projectIds = items.Select(p => p.Id).ToList();
            var tasks = await _taskRepository.GetTasksByProjectIdsAsync(projectIds);

            // Group tasks by project (filter out tasks with null ProjectId)
            var tasksByProject = tasks
                .Where(t => t.ProjectId.HasValue)
                .GroupBy(t => t.ProjectId!.Value)
                .ToDictionary(g => g.Key, g => g.ToList());

            // Assign tasks to projects
            foreach (var project in items)
            {
                if (tasksByProject.TryGetValue(project.Id, out var projectTasks))
                {
                    project.Tasks = projectTasks;
                }
            }
            */
        }

        return new PagedResult<Project>
        {
            Items = items,
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
            var id = int.Parse(GetColumnValue(row, columns, "Id"));
            var accountId = int.Parse(GetColumnValue(row, columns, "AccountId"));
            var name = GetColumnValue(row, columns, "Name");
            var description = GetColumnValue(row, columns, "Description");
            var missionId = int.Parse(GetColumnValue(row, columns, "MissionId"));
            
            var project = new Project(id, accountId, name, description, missionId);
            projects.Add(project);
        }

        return projects;
    }

    private string GetColumnValue(TursoValue[] row, string[] columns, string columnName)
    {
        var index = Array.IndexOf(columns, columnName);
        if (index < 0 || index >= row.Length)
            return string.Empty;

        return row[index].GetStringValue();
    }
}

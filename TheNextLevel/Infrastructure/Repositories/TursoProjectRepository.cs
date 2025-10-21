using System.Text.Json;
using TheNextLevel.Shared.DTOs;
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

    public async Task<int> GetTotalProjectsCountAsync()
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT COUNT(*) as TotalCount FROM Projects WHERE AccountId = ?",
            accountId);
        if (response.Result?.Rows == null || response.Result.Rows.Length == 0)
            return 0;

        var columns = response.Result.Cols.Select(c => c.Name ?? string.Empty).ToArray();
        return int.Parse(GetColumnValue(response.Result.Rows[0], columns, "TotalCount"));
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description FROM Projects WHERE Id = ? AND AccountId = ?",
            id, accountId);

        var project = MapToProjects(response).FirstOrDefault();

        if (project != null)
        {
            // Load tasks for this project
            var tasks = await _taskRepository.GetTasksByProjectIdsAsync(new[] { project.Id });
            project.Tasks = tasks.ToList();
        }

        return project;
    }

    public async Task<Project> AddAsync(Project project)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "INSERT INTO Projects (AccountId, Name, Description) VALUES (?, ?, ?)",
            accountId,
            project.Name,
            project.Description ?? string.Empty);

        return project;
    }

    public async Task<Project> UpdateAsync(Project project)
    {
        var accountId = _accountContext.GetCurrentAccountId();
        await _client.ExecuteAsync(
            "UPDATE Projects SET Name = ?, Description = ? WHERE Id = ? AND AccountId = ?",
            project.Name,
            project.Description ?? string.Empty,
            project.Id,
            accountId);

        return project;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var response = await _client.ExecuteAsync(
            "DELETE FROM Projects WHERE Id = ?",
            id);

        return response.Result?.AffectedRowCount > 0;
    }

    public async Task<PagedResult<Project>> GetPagedAsync(int skip, int take)
    {
        var accountId = _accountContext.GetCurrentAccountId();

        // Get total count
        var totalCount = await GetTotalProjectsCountAsync();

        // Get paged data
        var response = await _client.QueryAsync(
            "SELECT Id, AccountId, Name, Description FROM Projects WHERE AccountId = ? ORDER BY Name desc LIMIT ? OFFSET ?",
            accountId,
            take,
            skip);

        var items = MapToProjects(response).ToList();

        if (items.Any())
        {
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
            var project = new Project
            {
                Id = int.Parse(GetColumnValue(row, columns, "Id")),
                AccountId = int.Parse(GetColumnValue(row, columns, "AccountId")),
                Name = GetColumnValue(row, columns, "Name"),
                Description = GetColumnValue(row, columns, "Description")
            };
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

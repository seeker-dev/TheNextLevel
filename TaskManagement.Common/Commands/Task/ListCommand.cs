using TaskManagement.Data;

namespace TaskManagement.Domain.Commands.Task
{
    public class ListCommand(IDatabaseServiceFactory databaseServiceFactory) : IListCommand
    {
        public async Task<IEnumerable<ListTaskResponse>> ExecuteAsync()
        {
            // Create a database service instance
            var databaseService = await databaseServiceFactory.CreateAsync();
            // Fetch tasks from the database
            var tasks = await databaseService.GetAllTasksAsync();
            // Map the tasks to ListTaskResponse
            var taskResponses = tasks.Select(task => new ListTaskResponse
            {
                Id = task.Id,
                Name = task.Title,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                IsCompleted = task.IsCompleted
            });
            // Return the list of tasks
            return taskResponses;
        }
    }
}

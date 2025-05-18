using TaskManagement.Domain.Commands.Task;

namespace TheNextLevel.Controllers.v1
{
    public class TaskController(IServiceProvider provider)
    {
        //private readonly ITaskService _taskService;
        public async Task<IEnumerable<ListTaskResponse>> GetAllTasks()
        {
            var listCommand = provider.GetService<IListCommand>();
            if (listCommand == null)
            {
                throw new InvalidOperationException("IListCommand service is not registered.");
            }

            var tasks = await listCommand.ExecuteAsync();
            return tasks;
        }

        //public async Task<IActionResult> GetTaskById(int id)
        //{
        //    var task = await _taskService.GetTaskByIdAsync(id);
        //    if (task == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(task);
        //}

        //public async Task<IActionResult> CreateTask(TaskDto taskDto)
        //{
        //    var createdTask = await _taskService.CreateTaskAsync(taskDto);
        //    return CreatedAtAction(nameof(GetTaskById), new { id = createdTask.Id }, createdTask);
        //}

        //public async Task<IActionResult> UpdateTask(int id, TaskDto taskDto)
        //{
        //    var updatedTask = await _taskService.UpdateTaskAsync(id, taskDto);
        //    if (updatedTask == null)
        //    {
        //        return NotFound();
        //    }
        //    return Ok(updatedTask);
        //}

        //public async Task<IActionResult> DeleteTask(int id)
        //{
        //    var result = await _taskService.DeleteTaskAsync(id);
        //    if (!result)
        //    {
        //        return NotFound();
        //    }
        //    return NoContent();
        //}
    }
}

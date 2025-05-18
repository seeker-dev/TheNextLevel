namespace TaskManagement.Domain.Commands.Task
{
    public interface IListCommand
    {
        Task<IEnumerable<ListTaskResponse>> ExecuteAsync();
    }
}

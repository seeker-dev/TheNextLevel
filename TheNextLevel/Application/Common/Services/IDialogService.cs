namespace TheNextLevel.Application.Common.Services;

public interface IDialogService
{
    Task<T?> ShowAsync<T>(Type componentType, Dictionary<string, object>? parameters = null) where T : class;
    Task ShowAsync(Type componentType, Dictionary<string, object>? parameters = null);
    void Hide();
    event Action<DialogRequest>? OnShowDialog;
    event Action? OnHideDialog;
}

public class DialogRequest
{
    public Type ComponentType { get; set; } = null!;
    public Type? ResultType { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
    public TaskCompletionSource<object?>? CompletionSource { get; set; }
}
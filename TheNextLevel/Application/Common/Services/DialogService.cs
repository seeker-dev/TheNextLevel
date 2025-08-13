namespace TheNextLevel.Application.Common.Services;

public class DialogService : IDialogService
{
    public event Action<DialogRequest>? OnShowDialog;
    public event Action? OnHideDialog;

    public async Task<T?> ShowAsync<T>(Type componentType, Dictionary<string, object>? parameters = null) where T : class
    {
        var completionSource = new TaskCompletionSource<object?>();
        
        var request = new DialogRequest
        {
            ComponentType = componentType,
            ResultType = typeof(T),
            Parameters = parameters,
            CompletionSource = completionSource
        };

        OnShowDialog?.Invoke(request);
        
        var result = await completionSource.Task;
        return result as T;
    }

    public async Task ShowAsync(Type componentType, Dictionary<string, object>? parameters = null)
    {
        await ShowAsync<object>(componentType, parameters);
    }

    public void Hide()
    {
        OnHideDialog?.Invoke();
    }
}
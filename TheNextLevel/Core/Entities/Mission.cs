namespace TheNextLevel.Core.Entities;
public class Mission
{
    public int Id { get; private set; }
    public int AccountId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public bool IsCompleted { get; private set; }

    public Mission(int id, int accountId, string title, string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title, nameof(title));

        Id = id;
        AccountId = accountId;
        Title = title.Trim();
        Description = description?.Trim() ?? string.Empty;
        IsCompleted = false;
    }

    public void UpdateTitle(string newTitle)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newTitle, nameof(newTitle));
            
        Title = newTitle.Trim();
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription?.Trim() ?? string.Empty;
    }

    public void Complete()
    {
        if (!IsCompleted)
        {
            IsCompleted = true;
        }
    }

    public void Reset()
    {
        if (IsCompleted)
        {
            IsCompleted = false;
        }
    }
}
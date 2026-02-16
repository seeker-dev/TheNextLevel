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
        Id = id;
        AccountId = accountId;
        Title = title;
        Description = description;
        IsCompleted = false;
    }

    public void UpdateTitle(string newTitle)
    {
        Title = newTitle;
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
    }

    public void Complete()
    {
        IsCompleted = true;
    }

    public void Reset()
    {
        IsCompleted = false;
    }
}
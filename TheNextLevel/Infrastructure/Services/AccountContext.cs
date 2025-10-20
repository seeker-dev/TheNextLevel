using TheNextLevel.Core.Interfaces;

namespace TheNextLevel.Infrastructure.Services;

public class AccountContext : IAccountContext
{
    public int GetCurrentAccountId()
    {
        // Hardcoded for single-user scenario
        // TODO: Replace with actual authentication when user system is implemented
        return 1;
    }
}

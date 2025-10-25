using System.Threading.Tasks;

/// <summary>
/// Concrete implementation of IAuthenticationManager using AuthenticationFlowManager singleton
/// </summary>
public class AuthenticationManagerAdapter : IAuthenticationManager
{
    public bool IsGameReady()
    {
        return AuthenticationFlowManager.Instance != null && AuthenticationFlowManager.Instance.IsGameReady();
    }

    public void StartGame()
    {
        if (AuthenticationFlowManager.Instance != null)
        {
            AuthenticationFlowManager.Instance.StartGame();
        }
    }

    public async Task<bool> EnsureSolanaWallet()
    {
        if (AuthenticationFlowManager.Instance != null)
        {
            return await AuthenticationFlowManager.Instance.EnsureSolanaWallet();
        }
        return false;
    }

    public async Task<string> GetSolanaWalletAddress()
    {
        if (AuthenticationFlowManager.Instance != null)
        {
            return await AuthenticationFlowManager.Instance.GetSolanaWalletAddress();
        }
        return string.Empty;
    }

    public async Task<string> SignSolanaMessage(string message)
    {
        if (AuthenticationFlowManager.Instance != null)
        {
            return await AuthenticationFlowManager.Instance.SignSolanaMessage(message);
        }
        return string.Empty;
    }
}

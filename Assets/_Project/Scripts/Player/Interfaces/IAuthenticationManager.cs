using System.Threading.Tasks;

/// <summary>
/// Interface for authentication flow management
/// </summary>
public interface IAuthenticationManager
{
    bool IsGameReady();
    void StartGame();
    Task<bool> EnsureSolanaWallet();
    Task<string> GetSolanaWalletAddress();
    Task<string> SignSolanaMessage(string message);
}

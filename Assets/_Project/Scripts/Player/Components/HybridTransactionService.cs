using UnityEngine;
using System.Threading.Tasks;
using System;


/// <summary>
/// Hybrid transactio service
/// Combines privy auth with MagicBlocks SDK
/// </summary>
public class HybridTransactionService : MonoBehaviour
{

    [Header("Service Configuration")]
    [SerializeField] private MagicBlocksConfig config;
    [SerializeField] private bool enableHybridMode = true;
    [SerializeField] private bool enableFallbackMode = false;

    //Service components
    private IAuthenticationManager authenticationManager;
    private MagicBlocksSolanaAdapter magicBlocksAdapter;

    //Transaction Tracking
    private int transactionCount = 0;
    private float lastTransactionTime = 0f;
    private System.Collections.Generic.List<string> recentTransactions = new System.Collections.Generic.List<string>();

    //Events
    public static event Action<string> OnTransactionSuccess;
    public static event Action<string> OnTransactionFailure;
    public static event Action<int> OnTransactionCountUpdate;

    public static HybridTransactionService Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeServices();
    }

    /// <summary>
    /// init hybrid transaction services
    /// </summary>
    /// 
    private void InitializeServices()
    {
        try
        {
            Debug.Log("init hybrid transaction services...");

            //init privy auth manager
            authenticationManager = new AuthenticationManagerAdapter();

            //init magic blocks adapter
            magicBlocksAdapter = MagicBlocksSolanaAdapter.Instance;

            if(magicBlocksAdapter == null)
            {
                Debug.LogError("MagicBlocksSolanaAdapter instance is null!");
                var adapterObj = new GameObject("MagicBlocksSolanaAdapter");
                magicBlocksAdapter = adapterObj.AddComponent<MagicBlocksSolanaAdapter>();
            }

            //validate config
            if(config != null && !config.IsValid())
            {
                Debug.LogError("Invalid MagicBlocks configuration!");
                return;
            }

            Debug.Log("Hybrid transaction services initialized.");

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Hybrid Transaction Service: {ex.Message}");
        }
    }

    /// <summary>
    /// sending movement transaction
    /// </summary>
    /// 
    public async void SendMovementTransaction(Vector3 direction)
    {
        try
        {
            //validate service
            if (!ValidateServices())
            {
                Debug.LogError("Hybrid Transaction Service: Services not ready");
                return;
            }

            //create transaction message
            var message = CreateMovementMessage(direction);
            Debug.Log($"Hybrid Service: Sending movement transaction - {message}");

            //sendinf transaction using the hybrid approach
            var signature = await SendHybridTransaction(message);

            if (!string.IsNullOrEmpty(signature))
            {
                //transaction successfull
                transactionCount++;
                lastTransactionTime = Time.deltaTime;

                recentTransactions.Add(signature);

                //keep only 10 transactions
                if (recentTransactions.Count > 10)
                {
                    recentTransactions.RemoveAt(0);
                }

                Debug.Log($"Hybrid Service! Transaction successful with Signature: {signature}");
                OnTransactionSuccess?.Invoke(signature);
                OnTransactionCountUpdate?.Invoke(transactionCount);

                //sync to Privy dashboard if enabled
                if (config != null && config.enablePrivySync)
                {
                    await SyncToPrivyDashboard(signature, message);
                }
            }
            else
            {
                //transaction failed
                Debug.LogWarning("Hybrid Service: Transaction failed");
                OnTransactionFailure?.Invoke("Transaction failed");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Hybrid Transaction Service error: {ex.Message}");
            OnTransactionFailure?.Invoke(ex.Message);
        }
    }

    /// <summary>
    /// sending transaction using the hybrid approach 
    /// </summary>
    /// 
    private async Task<string> SendHybridTransaction(string message)
    {
        //if (!enableHybridMode)
        //{
        //    //return await SendFallbackTransaction(message);
        //    Debug.LogError("Hybrid mode disabled; refusing to use fallback. Enable hybrid mode to send transactions.");
        //    return null;
        //}

        try
        {
            //verify privy auth
            if (!await VerifyPrivyAuthentication())
            {
                Debug.LogError("Privy authentication failed; not sending fallback. Fix auth/wallet and retry.");
                return null;
            }

            //send transaction via magic blocks
            var signature = await magicBlocksAdapter.SendTransaction(message);

            if (!string.IsNullOrEmpty(signature))
            {
                return signature;
            }

            //fallback if magicblocks fails
            //if (enableFallbackMode)
            //{
            //    Debug.LogWarning("MagicBlocks transaction failed, using fallback");
            //    return await SendFallbackTransaction(message);
            //}

            return null;
        }
        catch(Exception ex)
        {
            Debug.LogError($"Hybrid transaction failed: {ex.Message}");

            //if (enableFallbackMode)
            //{
            //    return await SendFallbackTransaction(message);
            //}

            return null;
        }
    }

    /// <summary>
    /// Verify Privy authentication status
    /// </summary>
    private async Task<bool> VerifyPrivyAuthentication()
    {
        try
        {
            if (authenticationManager == null) return false;

            //check if game is ready and user is authenticated
            if (!authenticationManager.IsGameReady())
            {
                Debug.LogWarning("Privy: Game not ready (user not authenticated)");
                return false;
            }

            //check if Solana wallet exists
            var hasWallet = await authenticationManager.EnsureSolanaWallet();
            if (!hasWallet)
            {
                Debug.LogWarning("Privy: No Solana wallet found");
                return false;
            }

            //get wallet address to verify connection
            var walletAddress = await authenticationManager.GetSolanaWalletAddress();
            if (string.IsNullOrEmpty(walletAddress) || walletAddress == "No Wallet")
            {
                Debug.LogWarning("Privy: Invalid wallet address");
                return false;
            }

            Debug.Log($"Privy authentication verified with Wallet: {walletAddress}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Privy authentication verification failed: {ex.Message}");
            return false;
        }
    }


    /// <summary>
    /// Send transaction using fallback method
    /// </summary>
    private async Task<string> SendFallbackTransaction(string message)
    {
        try
        {
            Debug.Log("Using fallback transaction method...");

            //create a mock transaction
            var mockSignature = GenerateMockSignature(message);

            //log transaction
            await LogTransactionToDashboard(mockSignature, message);

            return mockSignature;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Fallback transaction failed: {ex.Message}");
            return null;
        }
    }


    /// <summary>
    /// Create movement message for transaction
    /// </summary>
    private string CreateMovementMessage(Vector3 direction)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
        var directionStr = $"({direction.x:F1},{direction.y:F1},{direction.z:F1})";

        return $"CrossyRoad_Move_{directionStr}_{timestamp}";
    }


    /// <summary>
    /// Generate mock signature for fallback transactions
    /// </summary>
    private string GenerateMockSignature(string message)
    {
        var data = $"{message}_{transactionCount}_{Time.time}";
        var hash = System.Security.Cryptography.SHA256.Create()
            .ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
        return System.Convert.ToBase64String(hash);
    }


    /// <summary>
    /// Sync transaction to Privy dashboard
    /// </summary>
    private async Task SyncToPrivyDashboard(string signature, string message)
    {
        try
        {
            if (magicBlocksAdapter != null)
            {
                await magicBlocksAdapter.SyncToPrivyDashboard(signature, message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to sync to Privy dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Log transaction to dashboard
    /// </summary>
    private async Task LogTransactionToDashboard(string signature, string message)
    {
        try
        {
            // TODO: Implement dashboard logging
            // This could be a webhook, API call, or local logging

            await Task.Delay(10); //simulate logging delay
            Debug.Log($"Transaction logged to dashboard: {signature}");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to log transaction to dashboard: {ex.Message}");
        }
    }


    /// <summary>
    /// Validate that all services are ready
    /// </summary>
    private bool ValidateServices()
    {
        if (authenticationManager == null)
        {
            Debug.LogError("Authentication manager not initialized");
            return false;
        }

        if (magicBlocksAdapter == null)
        {
            Debug.LogError("MagicBlocks adapter not initialized");
            return false;
        }

        if (config == null)
        {
            Debug.LogError("MagicBlocks configuration not set");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get transaction statistics
    /// </summary>
    public (int count, float lastTime, string[] recent) GetTransactionStats()
    {
        return (transactionCount, lastTransactionTime, recentTransactions.ToArray());
    }

    /// <summary>
    /// Check if service is ready for transactions
    /// </summary>
    public bool IsReady()
    {
        return ValidateServices() && magicBlocksAdapter.IsReady();
    }

    /// <summary>
    /// Get current wallet address
    /// </summary>
    public async Task<string> GetWalletAddress()
    {
        try
        {
            if (authenticationManager != null)
            {
                return await authenticationManager.GetSolanaWalletAddress();
            }
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get wallet address: {ex.Message}");
            return null;
        }
    }

    private void OnDestroy()
    {
        // Cleanup
        recentTransactions.Clear();
    }
}

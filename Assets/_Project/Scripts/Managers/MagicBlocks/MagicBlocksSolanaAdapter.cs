using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;


/// <summary>
/// Magiclocks Solana SDK adapter for Unity Inteegration
/// </summary>

public class MagicBlocksSolanaAdapter : MonoBehaviour
{

    [Header("MagicBlocks Configuration")]
    [SerializeField] private string rpcEndpoint = "https://api.devnet.solana.com";
    [SerializeField] private string walletNetwork = "devnet";

    [Header("Transaction Settings")]
    [SerializeField] private float transactionTimeout = 5f;
    [SerializeField] private bool enableTransactionBatching = true;
    [SerializeField] private int batchSize = 5;

    /// <summary>
    /// here add the SDK componets after the installation
    /// </summary>
    /// 

    //transaction batching
    private System.Collections.Generic.Queue<string> pendingTransactions = new System.Collections.Generic.Queue<string>();
    private float lastBatchTime = 0f;
    private const float BATCH_INTERVAL = 2f;

    public static MagicBlocksSolanaAdapter Instance { get; private set; }

    //Events
    public static event Action<string> OnTransactionSent;
    public static event Action<string> OnTransactionFailed;

    private void Awake()
    {
        if(Instance != null && Instance != this)
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
        InitializeMagicBlocks();
    }

    /// <summary>
    /// Initializing the MagicBlocks SDK with the privy wallet
    /// </summary>
    /// 
    private async void InitializeMagicBlocks()
    {
        //Here add the SDK initialization code after installation
        try
        {
            Debug.Log("Initializing MagicBlocks Solana SDK...");
            //init magic block SDK here
            //walletadapter = new solana wallet adapter()
            //repcClient = new solana rpc client(rpcEndpoint)

            //await walletadapter.connect();
            //await connectToPrvywallet(); 

            Debug.Log("MagicBlocks Solana SDK Initialized");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing MagicBlocks Solana SDK: {ex.Message}");
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Connect Magic Blocks to Privy Wallet
    /// </summary>
    /// 
    private async Task connectToPrvywallet()
    {
        //get wallet information
        try
        {
            if(AuthenticationFlowManager.Instance != null)
            {
                var walletAddress = await AuthenticationFlowManager.Instance.GetSolanaWalletAddress();

                if (!string.IsNullOrEmpty(walletAddress) && walletAddress != "No Wallet")
                {
                    Debug.Log($"Connecting MagicBlocks to Privy Wallet: {walletAddress}");

                    //TODO: Connect MagicBlocks SDK to Privy Wallet using walletAddress

                    Debug.Log("Connected to Privy Wallet successfully.");
                }
                else
                {
                    Debug.LogWarning("no wallet address found for magicblocks connection.");
                }
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error connecting to Privy Wallet: {ex.Message}");
        }
        await Task.CompletedTask;
    }


    /// <summary>
    /// Send Solana transaction using magicblocks SDK
    /// </summary>
    /// 
    public async Task<string> SendTransaction(string message)
    {
        try
        {
            Debug.Log($"MagicBlocks: sending transcation = {message}");

            if (enableTransactionBatching)
            {
                return await SendBatchedTransaction(message);
            }
            else
            {
                return await SendImmediateTransaction(message);
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"MagicBlocks transaction failed: {ex.Message}");
            OnTransactionFailed?.Invoke(ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Send transacion immediatly without batching
    /// </summary>
    /// 
    private async Task<string> SendImmediateTransaction(string message)
    {
        //TODO: impl immediate transaction with magicblocks SDK

        //placeholder impl
        await Task.Delay(100); //simulate network delay
        var mockSignature = GenerateMockSignature(message);

        Debug.Log($"MagicBlocks! Transaction sent immediately with Signature: {mockSignature}");
        OnTransactionSent?.Invoke(mockSignature);

        return mockSignature;
    }

    /// <summary>
    /// sending transaction as a batch
    /// </summary>
    /// 
    private async Task<string> SendBatchedTransaction(string message)
    {
        //adding the batch to the queue
        pendingTransactions.Enqueue(message);

        //checking if we should send the batch or not
        if (ShouldSendBatch())
        {
            return await ProcessBatch();
        }

        //return a batch ID for tracking
        return $"BATCH_{pendingTransactions.Count}_{Time.time}";
    }

    /// <summary>
    /// Check if we should send the current batch
    /// </summary>
    private bool ShouldSendBatch()
    {
        return pendingTransactions.Count >= batchSize ||
               (Time.time - lastBatchTime) >= BATCH_INTERVAL;
    }

    /// <summary>
    /// process the current batch of transactions
    /// </summary>
    /// 
    private async Task<string> ProcessBatch()
    {
        if(pendingTransactions.Count == 0)
        {
            return null;
        }

        Debug.Log($"MagicBlocks! processing batch of {pendingTransactions.Count} transactions");

        //TODO: impl batch transcations with magicblocks sdk

        //placeholder impl
        await Task.Delay(200); // Simulate batch processing
        var batchSignature = GenerateMockSignature($"BATCH_{pendingTransactions.Count}");

        // Clear the batch
        pendingTransactions.Clear();
        lastBatchTime = Time.time;

        Debug.Log($"MagicBlocks: Batch processed - Signature: {batchSignature}");
        OnTransactionSent?.Invoke(batchSignature);

        return batchSignature;
    }


    /// <summary>
    /// create movement transcation
    /// </summary>
    /// 
    private object CreateMovementTransaction(string message)
    {

        //TODO: impl transacion creation with magicblocks sdk

        return new
        {
            type = "movement_transaction",
            message = message,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            game = "CrossyRoad",
            network = walletNetwork
        };
    }


    /// <summary>
    /// generate a mock signature for testing
    /// </summary>
    private string GenerateMockSignature(string data)
    {
        var hash = System.Security.Cryptography.SHA256.Create()
            .ComputeHash(System.Text.Encoding.UTF8.GetBytes($"{data}_{Time.time}"));
        return System.Convert.ToBase64String(hash);
    }

    /// <summary>
    /// Sync transaction data to Privy dashboard
    /// </summary>
    public async Task SyncToPrivyDashboard(string signature, string message)
    {
        try
        {
            Debug.Log($"Syncing transaction to Privy dashboard: {signature}");

            // TODO: Implement Privy dashboard sync
            // This could be done via webhook or API call to Privy

            //placeholder impl
            await Task.Delay(50);
            Debug.Log("Transaction synced to Privy dashboard successfully");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Failed to sync to Privy dashboard: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if MagicBlocks is ready for transactions
    /// </summary>
    public bool IsReady()
    {
        // TODO: Check MagicBlocks SDK status
        // return walletAdapter != null && walletAdapter.IsConnected;

        // Placeholder implementation
        return true;
    }

    /// <summary>
    /// Get current wallet address from MagicBlocks
    /// </summary>
    public async Task<string> GetWalletAddress()
    {
        try
        {
            // TODO: Get wallet address from MagicBlocks SDK
            // return await walletAdapter.GetWalletAddress();

            // Placeholder implementation
            await Task.Delay(10);
            return "MagicBlocks_Wallet_Address_Placeholder";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get MagicBlocks wallet address: {ex.Message}");
            return null;
        }
    }

    private void OnDestroy()
    {
        // Cleanup MagicBlocks resources
        // walletAdapter?.Dispose();
        // rpcClient?.Dispose();
    }
}

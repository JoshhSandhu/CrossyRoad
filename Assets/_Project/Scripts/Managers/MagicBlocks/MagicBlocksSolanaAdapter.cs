using Cysharp.Threading.Tasks;
using Org.BouncyCastle.Asn1.Ocsp;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Magiclocks Solana SDK adapter for Unity Inteegration
/// </summary>

public class MagicBlocksSolanaAdapter : MonoBehaviour
{

    [Header("MagicBlocks Configuration")]
    [SerializeField] private string rpcEndpoint = "https://api.devnet.solana.com";
    [SerializeField] private string walletNetwork = "devnet";
    [SerializeField] private MagicBlocksConfig config;
    [SerializeField] private bool usePrivyBridge = true;

    [Header("Transaction Settings")]
    [SerializeField] private float transactionTimeout = 5f;
    [SerializeField] private bool enableTransactionBatching = true;
    [SerializeField] private int batchSize = 5;

    /// <summary>
    /// here add the SDK componets after the installation
    /// </summary>
    /// 

    //custom pruivy wallet adapter
    private CustomPrivyWalletAdapter customPrivyAdapter;

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
    /// Initializing the MagicBlocks SDK with the custom privy wallet adapter
    /// </summary>
    /// 
    private async void InitializeMagicBlocks()
    {
        //Here add the SDK initialization code after installation
        try
        {
            Debug.Log("Init Solana SDK with custom privy wallet adapter...");

            //waiting for the custom wallet adapter to init
            await Task.Delay(1000);

            customPrivyAdapter = CustomPrivyWalletAdapter.Instance;
            if(customPrivyAdapter == null)
            {
                Debug.LogError("Custom Privy Wallet Adapter not found!");
                return;
            }

            //checking if the adapter is ready
            if(customPrivyAdapter.IsReady())
            {
                Debug.Log("Custom Privy Wallet Adapter ready!");
                Debug.Log($"Wallet address: {customPrivyAdapter.GetWalletAddress()}");
            }
            else
            {
                Debug.LogWarning("Custom Privy Wallet Adapter not ready yet, will retry...");
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Solana SDK: {ex.Message}");
        }
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
    /// Send transacion immediatly using custom privy wallet adapter
    /// </summary>
    /// 
    private async Task<string> SendImmediateTransaction(string message)
    {
        try
        {
            //checking if the adapter is ready
            if(customPrivyAdapter == null || !customPrivyAdapter.IsReady())
            {
                Debug.LogError("Custom Privy Wallet Adapter not ready!");
                OnTransactionFailed?.Invoke("Adapter not ready");
                return null;
            }

            Debug.Log($"Signing message with Custom Privy Wallet Adapter: {message}");
            var signature = await customPrivyAdapter.SignAndSendTransaction(message);

            //sign and send transaction via custom privy adapter
            if (!string.IsNullOrEmpty(signature))
            {
                Debug.Log($"Transaction sent successfully - Signature: {signature}");
                OnTransactionSent?.Invoke(signature);
                return signature;
            }
            else
            {
                Debug.LogError("Transaction signature failed!");
                OnTransactionFailed?.Invoke("Signature failed");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"transaction failed: {ex.Message}");
            OnTransactionFailed?.Invoke(ex.Message);
            return null;
        }
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

        try
        {
            Debug.Log($"processing batch of {pendingTransactions.Count} transactions");

            Debug.Log($"processing batch of {pendingTransactions.Count} transactions");

            //check if custom privy adapter is ready
            if (customPrivyAdapter == null || !customPrivyAdapter.IsReady())
            {
                Debug.LogError("Custom Privy Wallet Adapter not ready!");
                OnTransactionFailed?.Invoke("Wallet not connected");
                return null;
            }

            //get wallet address for batch reference
            var walletAddress = customPrivyAdapter.GetWalletAddress();

            //create batch message with all pending transactions
            var batchMessages = string.Join("|", pendingTransactions);
            var batchMessage = $"BATCH_{pendingTransactions.Count}_{walletAddress}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            //send batch transaction via custom privy adapter
            var batchSignature = await customPrivyAdapter.SignAndSendTransaction(batchMessage);

            if (!string.IsNullOrEmpty(batchSignature))
            {
                //clear batch
                pendingTransactions.Clear();
                lastBatchTime = Time.time;

                Debug.Log($"Batch processed successfully - Signature: {batchSignature}");
                OnTransactionSent?.Invoke(batchSignature);

                return batchSignature;
            }
            else
            {
                Debug.LogError("Batch transaction failed");
                OnTransactionFailed?.Invoke("Batch transaction failed");
                return null;
            }
        }
        catch(Exception ex)
        {
            Debug.LogError($"Batch processing failed: {ex.Message}");
            OnTransactionFailed?.Invoke(ex.Message);

            //clear pending transactions on error
            pendingTransactions.Clear();
            return null;
        }
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
        try
        {
            return customPrivyAdapter != null && customPrivyAdapter.IsReady();
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error checking wallet status: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Get current wallet address from MagicBlocks
    /// </summary>
    public async Task<string> GetWalletAddress()
    {
        try
        {

            if (customPrivyAdapter != null && customPrivyAdapter.IsReady())
            {
                return customPrivyAdapter.GetWalletAddress();
            }

            Debug.LogWarning("Custom Privy Wallet Adapter not ready");
            return null;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get Solana wallet address: {ex.Message}");
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

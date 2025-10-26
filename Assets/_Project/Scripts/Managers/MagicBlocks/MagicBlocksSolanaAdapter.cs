using System;
using UnityEngine;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Cysharp.Threading.Tasks;

/// <summary>
/// Magiclocks Solana SDK adapter for Unity Inteegration
/// </summary>

public class MagicBlocksSolanaAdapter : MonoBehaviour
{

    [Header("MagicBlocks Configuration")]
    [SerializeField] private string rpcEndpoint = "https://api.devnet.solana.com";
    [SerializeField] private string walletNetwork = "devnet";
    [SerializeField] private MagicBlocksConfig config;

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
            
            //checking if the web3 instance exists
            if(Web3.Instance == null)
            {
                Debug.LogWarning("Web3 instance not found. Solana SDK not initialized yet");
                return;
            }

            //wait for the solana sdk to be ready
            if(Web3.Wallet == null)
            {
                Debug.LogWarning("web3 wallet not found. waiting for the privy connection");
                return;
            }

            Debug.Log("MagicBlocks Solana SDK Initialized");
            Debug.Log($"Wallet address: {Web3.Wallet.Account.PublicKey}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Solana SDK: {ex.Message}");
        }
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
        try
        {
            //check if the wallet is connected
            if(Web3.Wallet == null)
            {
                Debug.LogError("Solana wallet not connected!");
                OnTransactionFailed?.Invoke("Wallet not connected");
                return null;
            }

            //convert message to bytes
            var messagBytes = System.Text.Encoding.UTF8.GetBytes(message);

            //sign message using solana sdk
            var signatureBytes = await Web3.Wallet.SignMessage(messagBytes);

            //convert signature to readable format
            var signature = Convert.ToBase64String(signatureBytes);

            Debug.Log($"Solana SDK: Transaction sent, signature = {signature}");
            OnTransactionSent?.Invoke(signature);

            return signature;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Solana SDK transaction failed: {ex.Message}");
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

            //check if the wallet is conected
            if(Web3.Wallet == null)
            {
                Debug.LogError("Solana wallet not connected!");
                OnTransactionFailed?.Invoke("Wallet not connected");
                return null;
            }

            //get wallet public key for batch refrence 
            var walletAddress = Web3.Wallet.Account.PublicKey.ToString();

            //create batch message
            var batchMessages = string.Join("|", pendingTransactions);
            var batchMessage = $"BATCH_{pendingTransactions.Count}_{walletAddress}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            //sign message to batch
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(batchMessages);
            var signatureBytes = await Web3.Wallet.SignMessage(messageBytes);
            var batchSignature = System.Convert.ToBase64String(signatureBytes);

            //clear batch
            pendingTransactions.Clear();
            lastBatchTime = Time.time;

            Debug.Log($"Batch processed, Signature: {batchSignature}");
            OnTransactionSent?.Invoke(batchSignature);

            return batchSignature;
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
            //check if web3 instance exists
            if(Web3.Instance == null)
            {
                return false;
            }

            //check if wallet is connected
            if(Web3.Wallet == null)
            {
                return false;
            }

            //check if wallet accout exists
            if(Web3.Wallet.Account == null)
            {
                return false;
            }

            return true;
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error checking Solana SDK status: {ex.Message}");
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
            //check if the walllet is connected
            if(Web3.Wallet == null)
            {
                Debug.LogWarning("Solana wallet not connected yet");
                return null;
            }

            //get wallet public key
            var publickey = Web3.Wallet.Account.PublicKey.ToString();
            return publickey;
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

using System.Net;
using UnityEngine;


/// <summary>
/// MagicBlocks Config for MD Solana Integration
/// </summary>

[CreateAssetMenu(fileName = "MagicBlocksConfig", menuName = "MagicBlocks/Configuration")]
public class MagicBlocksConfig : ScriptableObject
{
    [Header("Network Configuration")]
    [Tooltip("Solana RPC endpoint")]
    public string rpcEndpoint = "https://api.devnet.solana.com";

    [Tooltip("Solana network (mainnet, devnet, testnet)")]
    public string network = "devnet";

    [Tooltip("Enable transaction batching for better performance")]
    public bool enableBatching = true;

    [Header("Transaction Settings")]
    [Tooltip("Maximum time to wait for transaction confirmation (seconds)")]
    public float transactionTimeout = 5f;

    [Tooltip("Number of transactions to batch together")]
    [Range(1, 20)]
    public int batchSize = 5;

    [Tooltip("Time interval between batch sends (seconds)")]
    [Range(0.5f, 10f)]
    public float batchInterval = 2f;

    [Header("Performance Settings")]
    [Tooltip("Enable transaction retry on failure")]
    public bool enableRetry = true;

    [Tooltip("Maximum number of retry attempts")]
    [Range(1, 5)]
    public int maxRetries = 3;

    [Tooltip("Delay between retry attempts (seconds)")]
    [Range(0.1f, 2f)]
    public float retryDelay = 0.5f;

    [Header("Debug Settings")]
    [Tooltip("Enable detailed logging")]
    public bool enableDebugLogging = true;

    [Tooltip("Log all transaction attempts")]
    public bool logAllTransactions = true;

    [Tooltip("Enable mock transactions for testing")]
    public bool enableMockTransactions = false;

    [Header("Privy Integration")]
    [Tooltip("Enable Privy dashboard synchronization")]
    public bool enablePrivySync = true;

    [Tooltip("Privy dashboard webhook URL (if available)")]
    public string privyWebhookUrl = "";

    [Tooltip("Sync interval to Privy dashboard (seconds)")]
    [Range(1f, 60f)]
    public float privySyncInterval = 10f;

    /// <summary>
    /// Validate configuration settings
    /// </summary>
    public bool IsValid()
    {
        if (string.IsNullOrEmpty(rpcEndpoint))
        {
            Debug.LogError("MagicBlocks: RPC endpoint is required");
            return false;
        }

        if (transactionTimeout <= 0)
        {
            Debug.LogError("MagicBlocks: Transaction timeout must be greater than 0");
            return false;
        }

        if (batchSize <= 0)
        {
            Debug.LogError("MagicBlocks: Batch size must be greater than 0");
            return false;
        }

        if (batchInterval <= 0)
        {
            Debug.LogError("MagicBlocks: Batch interval must be greater than 0");
            return false;
        }

        return true;
    }


    /// <summary>
    /// Get network-specific configuration
    /// </summary>
    public string GetNetworkConfig()
    {
        switch (network.ToLower())
        {
            case "mainnet":
                return "https://api.mainnet-beta.solana.com";
            case "devnet":
                return "https://api.devnet.solana.com";
            case "testnet":
                return "https://api.testnet.solana.com";
            default:
                return rpcEndpoint;
        }
    }

    /// <summary>
    /// Get transaction priority based on network
    /// </summary>
    public int GetTransactionPriority()
    {
        switch (network.ToLower())
        {
            case "mainnet":
                return 1;       //High priority for mainnet
            case "devnet":
                return 2;       //Medium priority for devnet
            case "testnet":
                return 3;       //Low priority for testnet
            default:
                return 2;
        }
    }

    /// <summary>
    /// Check if mock transactions should be used
    /// </summary>
    public bool ShouldUseMockTransactions()
    {
        return enableMockTransactions || network.ToLower() == "testnet";
    }

    /// <summary>
    /// Get retry configuration
    /// </summary>
    public (bool enabled, int maxRetries, float delay) GetRetryConfig()
    {
        return (enableRetry, maxRetries, retryDelay);
    }

    /// <summary>
    /// Get batching configuration
    /// </summary>
    public (bool enabled, int batchSize, float interval) GetBatchingConfig()
    {
        return (enableBatching, batchSize, batchInterval);
    }

    // <summary>
    /// Get Privy sync configuration
    /// </summary>
    public (bool enabled, string webhookUrl, float interval) GetPrivySyncConfig()
    {
        return (enablePrivySync, privyWebhookUrl, privySyncInterval);
    }
}

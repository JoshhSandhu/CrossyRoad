using System;
using System.Threading.Tasks;
using UnityEngine;
using Solana.Unity.SDK;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc.Types;

/// <summary>
/// Manages connection to Seeker wallet via Mobile Wallet Adapter (MWA)
/// Handles Seeker wallet operations: connection, balance checking, and transfers
/// </summary>
public class SeekerWalletManager : MonoBehaviour
{
    public static SeekerWalletManager Instance { get; private set; }

    private bool isConnected = false;
    public bool IsConnected => isConnected && Web3.Wallet != null;

    // Transaction cost per move in lamports (0.000005 SOL = 5000 lamports)
    public const ulong TRANSACTION_COST_PER_MOVE = 5000;

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

    /// <summary>
    /// Connect to Seeker wallet via Mobile Wallet Adapter
    /// </summary>
    public async Task<bool> ConnectToSeekerWallet()
    {
        try
        {
            if (Web3.Instance == null)
            {
                Debug.LogError("Web3.Instance is null. Make sure Web3 script is attached to a GameObject in the scene.");
                return false;
            }

            // Set RPC cluster to DevNet
            Web3.Instance.rpcCluster = RpcCluster.DevNet;

            // This automatically uses SolanaMobileWalletAdapter on Android/iOS
            // On Seeker emulator, this connects to the Seeker wallet
            Debug.Log("Connecting to Seeker wallet via Mobile Wallet Adapter...");
            var account = await Web3.Instance.LoginWalletAdapter();

            if (account != null && Web3.Wallet != null)
            {
                isConnected = true;
                Debug.Log($"Connected to Seeker wallet: {Web3.Wallet.Account.PublicKey}");
                return true;
            }
            else
            {
                Debug.LogWarning("Failed to connect to Seeker wallet. Account or Wallet is null.");
                return false;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to connect to Seeker wallet: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// Get Seeker wallet balance in lamports
    /// </summary>
    public async Task<ulong> GetSeekerBalance()
    {
        if (!IsConnected)
        {
            Debug.LogWarning("Seeker wallet not connected. Attempting to connect...");
            var connected = await ConnectToSeekerWallet();
            if (!connected)
            {
                return 0;
            }
        }

        try
        {
            var balance = await Web3.Instance.WalletBase.GetBalance();
            // GetBalance returns double (SOL), convert to lamports (ulong)
            return (ulong)(balance * 1_000_000_000);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to get Seeker balance: {ex.Message}");
            return 0;
        }
    }

    /// <summary>
    /// Transfer SOL from Seeker wallet to Privy wallet
    /// </summary>
    public async Task<string> TransferToPrivyWallet(string privyAddress, ulong amountLamports)
    {
        if (!IsConnected)
        {
            Debug.LogError("Seeker wallet not connected. Please connect first.");
            return null;
        }

        if (string.IsNullOrEmpty(privyAddress))
        {
            Debug.LogError("Privy wallet address is null or empty");
            return null;
        }

        try
        {
            Debug.Log($"Transferring {amountLamports} lamports from Seeker wallet to Privy wallet: {privyAddress}");

            // Use the built-in Transfer method from SDK
            // Transfer method signature: Transfer(PublicKey destination, ulong amount)
            var destinationPubKey = new PublicKey(privyAddress);
            var result = await Web3.Instance.WalletBase.Transfer(
                destinationPubKey,
                amountLamports
            );

            // RequestResult has Result (null if failed) and Reason (error message)
            if (result.Result != null)
            {
                Debug.Log($"Transfer successful! Transaction signature: {result.Result}");
                return result.Result; // Transaction signature
            }
            else
            {
                string errorMsg = result.Reason ?? "Unknown error";
                Debug.LogError($"Transfer failed: {errorMsg}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Transfer exception: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Get Seeker wallet address
    /// </summary>
    public string GetSeekerAddress()
    {
        if (!IsConnected)
        {
            return null;
        }
        return Web3.Wallet.Account.PublicKey;
    }

    /// <summary>
    /// Disconnect from Seeker wallet
    /// </summary>
    public void Disconnect()
    {
        if (Web3.Instance != null)
        {
            Web3.Instance.Logout();
            isConnected = false;
            Debug.Log("Disconnected from Seeker wallet");
        }
    }
}




using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Privy;
using Solana.Unity.Wallet;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Builders;
using Solana.Unity.Rpc.Models;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Programs;
using Cysharp.Threading.Tasks;


/// <summary>
/// custom wallet adapter for Privy wallet integration
/// impl the bridge between Privy SDK and Solana.Unity.SDK wallet adapter system
/// </summary>
public class CustomPrivyWalletAdapter : MonoBehaviour
{
    public static CustomPrivyWalletAdapter Instance { get; private set; }

    private IEmbeddedSolanaWallet privyWallet;
    private bool isInitialized = false;
    private string walletAddress = "";

    // RPC client for sending transactions
    private IRpcClient rpcClient;

    // Approval mechanism: User approves transactions by pressing "Play Game" button
    // This flag is set when the user presses Play Game, indicating they approve Privy to sign transactions
    private bool hasUserApprovedViaPlayGame = false;

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

    private void Start()
    {
        // Wait for authentication to complete before initializing
        StartCoroutine(WaitForAuthenticationAndInitialize());
    }

    private System.Collections.IEnumerator WaitForAuthenticationAndInitialize()
    {
        // Wait until AuthenticationFlowManager is ready
        yield return new WaitForSeconds(1f);

        // Keep checking until user is authenticated
        while (AuthenticationFlowManager.Instance == null || !AuthenticationFlowManager.Instance.IsAuthenticated)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Authentication confirmed, initializing adapter...");
        InitializeAdapter();
    }

    /// <summary>
    /// init the custom adapter by connecting the privy wallet
    /// </summary>
    /// 
    private async void InitializeAdapter()
    {
        try
        {
            Debug.Log("Initializing Privy Wallet Adapter...");

            //connect to Privy wallet
            privyWallet = await GetPrivyWallet();
            if (privyWallet == null)
            {
                Debug.LogError("Failed to connect to Privy wallet.");
                return;
            }

            walletAddress = privyWallet.Address;
            Debug.Log($"connected to Privy wallet with address: {walletAddress}");

            // Initialize Solana RPC client for devnet
            rpcClient = ClientFactory.GetClient("https://api.devnet.solana.com");
            Debug.Log("Solana RPC client initialized for devnet");

            isInitialized = true;
            Debug.Log("Privy Wallet Adapter initialized successfully.");

            // Quick diagnostic: attempt a simple message sign to verify Privy UI flow
            // DISABLED: This test times out because Privy SDK is headless and requires our custom modal
            // Uncomment the line below if you want to test the signing flow manually
            // _ = TestPrivySimpleSign();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error initializing Privy Wallet Adapter: {ex.Message}");
        }
    }


    /// <summary>
    /// getting the privy wallet
    /// </summary>
    /// 
    private async Task<IEmbeddedSolanaWallet> GetPrivyWallet()
    {
        try
        {
            Debug.Log("Getting Privy wallet...");

            if (AuthenticationFlowManager.Instance == null)
            {
                Debug.LogError("AuthenticationFlowManager instance is null.");
                return null;
            }

            // Ensure user is authenticated and has wallet
            if (!AuthenticationFlowManager.Instance.IsAuthenticated)
            {
                Debug.LogError("User is not authenticated yet.");
                return null;
            }

            // Ensure Solana wallet exists
            var hasWallet = await AuthenticationFlowManager.Instance.EnsureSolanaWallet();
            if (!hasWallet)
            {
                Debug.LogError("Failed to ensure Solana wallet exists.");
                return null;
            }

            Debug.Log("Getting Solana wallets from Privy...");
            var walllets = await AuthenticationFlowManager.Instance.GetSolanaWallets();

            if (walllets == null)
            {
                Debug.LogError("GetSolanaWallets returned null.");
                return null;
            }

            if (walllets.Length == 0)
            {
                Debug.LogError("No Solana wallets found in Privy.");
                return null;
            }

            Debug.Log($"Found {walllets.Length} Solana wallet(s), using first one: {walllets[0].Address}");
            return walllets[0];
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error getting Privy wallet: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// sign mesaage with privy and send transaction via solana sdk
    /// </summary>
    /// 
    public async Task<string> SignAndSendTransaction(string message)
    {
        try
        {
            if (!isInitialized || privyWallet == null)
            {
                Debug.LogError("Privy Wallet Adapter is not initialized.");
                return null;
            }

            Debug.Log($"Building and sending full transaction to Solana devnet: {message}");

            // 1. Build Solana transaction
            var transaction = await BuildSolanaTransaction(message);

            Debug.Log($"Transaction built, proceeding to sign and send...{transaction}");

            // 2. Sign and send transaction with Privy (this will show the signing modal)
            var signature = await SignAndSendTransactionInternal(transaction);

            if (!string.IsNullOrEmpty(signature))
            {
                Debug.Log($"View on explorer: https://explorer.solana.com/tx/{signature}?cluster=devnet");
                return signature;
            }

            return null;

        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sign and send transaction: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Build Solana transaction with memo instruction
    /// </summary>
    private async Task<Transaction> BuildSolanaTransaction(string message)
    {
        try
        {
            Debug.Log("Building Solana transaction...");

            // Get wallet public key
            var walletPubKey = new PublicKey(privyWallet.Address);

            // Get recent blockhash
            var blockHashResult = await rpcClient.GetLatestBlockHashAsync();
            if (!blockHashResult.WasSuccessful)
            {
                throw new Exception($"Failed to get blockhash: {blockHashResult.Reason}");
            }

            var recentBlockHash = blockHashResult.Result.Value.Blockhash;

            // Build transaction bytes - TransactionBuilder.Build returns byte[]
            //var transactionBytes = new TransactionBuilder()
            //    .SetRecentBlockHash(recentBlockHash)
            //    .SetFeePayer(walletPubKey)
            //    .Build(new List<Account>());
            var instructions = new List<TransactionInstruction>();
            instructions.Add(MemoProgram.NewMemo(walletPubKey, message));

            // Deserialize bytes into Transaction object
            //var transaction = Transaction.Deserialize(transactionBytes);

            var transaction = new Transaction
            {
                RecentBlockHash = recentBlockHash,
                FeePayer = walletPubKey,
                Instructions = instructions,
                Signatures = new List<SignaturePubKeyPair>()
            };

            Debug.Log($"Tx prepared: feePayer={walletPubKey}, blockhash={recentBlockHash}, instructions={instructions.Count}");
            Debug.Log($"Transaction built with recent blockhash: {recentBlockHash}");
            Debug.Log("Transaction built successfully");
            return transaction;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to build transaction: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Sign and send transaction with Privy wallet
    /// Returns the transaction signature from the blockchain
    /// </summary>
    private async Task<string> SignAndSendTransactionInternal(Transaction transaction)
    {
        try
        {
            Debug.Log("Signing transaction with Privy wallet...");

            await UniTask.SwitchToMainThread();

            // Check if user has approved transactions via Play Game button
            // If not approved, transaction will fail (user must press Play Game first)
            if (!hasUserApprovedViaPlayGame)
            {
                Debug.LogWarning("Transaction signing rejected: User has not approved via Play Game button.");
                return null;
            }

            Debug.Log("Transaction approved via Play Game button. Proceeding with Privy signing...");

            // Android/iOS: Native WebView is used automatically by Privy SDK
            // No iframe or manual WebView handling needed
#if !(UNITY_ANDROID || UNITY_IOS)
            Debug.LogWarning("Privy signing only works in Android/iOS builds!");
            Debug.LogWarning("Testing in Unity Editor will timeout. Please build for Android to test.");
#endif

            // Serialize the transaction message for signing
            var messageBytes = transaction.CompileMessage();
            var transactionBase64 = System.Convert.ToBase64String(messageBytes);

            if (privyWallet.EmbeddedSolanaWalletProvider == null)
            {
                Debug.LogError("EmbeddedSolanaWalletProvider is null on Privy wallet.");
                return null;
            }

            // Request signature from Privy
            var signTask = privyWallet.EmbeddedSolanaWalletProvider.SignMessage(transactionBase64);
            var completed = await Task.WhenAny(signTask, Task.Delay(TimeSpan.FromSeconds(30)));
            string signatureString;
            if (completed != signTask)
            {
                Debug.LogWarning("Privy signing timed out with compiled-message base64. Retrying with FULL-TX base64...");

                // Retry with full serialized transaction (some SDKs expect this as the payload)
                transaction.CompileMessage();
                var fullTxBytes = transaction.Serialize();
                var fullTxBase64 = System.Convert.ToBase64String(fullTxBytes);
                var retryTask = privyWallet.EmbeddedSolanaWalletProvider.SignMessage(fullTxBase64);
                var retried = await Task.WhenAny(retryTask, Task.Delay(TimeSpan.FromSeconds(20)));
                if (retried != retryTask)
                {
                    Debug.LogError("Privy signing timed out after retry (full-tx base64).");
                    Debug.LogError("NOTE: Privy signing only works in Android/iOS builds! If testing in Unity Editor, build for Android instead.");
                    return null;
                }
                signatureString = await retryTask;
            }
            else
            {
                signatureString = await signTask;
            }

            if (string.IsNullOrEmpty(signatureString))
            {
                Debug.LogError("Privy signature failed!");
                return null;
            }

            Debug.Log($"Transaction signed with Privy. Signature: {signatureString}");

            // Attach signature to transaction
            var walletPubKey = new PublicKey(privyWallet.Address);
            var signatureBytes = System.Convert.FromBase64String(signatureString);

            transaction.Signatures.Add(new SignaturePubKeyPair
            {
                PublicKey = walletPubKey,
                Signature = signatureBytes
            });

            transaction.CompileMessage();
            // Serialize the fully signed transaction
            var signedTransactionBytes = transaction.Serialize();

            // Send to blockchain
            Debug.Log("Sending transaction to Solana devnet...");
            var sendResult = await rpcClient.SendTransactionAsync(signedTransactionBytes);

            // Transaction sent successfully

            if (sendResult.WasSuccessful && !string.IsNullOrEmpty(sendResult.Result))
            {
                Debug.Log($"? Transaction sent successfully! Signature: {sendResult.Result}");
                return sendResult.Result;
            }
            else
            {
                Debug.LogError($"? Transaction failed: {sendResult.Reason}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to sign and send transaction: {ex.Message}");
            Debug.LogError($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// checking if the adapter is ready
    /// </summary>
    /// 
    public bool IsReady()
    {
        return isInitialized && privyWallet != null;
    }

    /// <summary>
    /// getting the walllet address
    /// </summary>
    /// 
    public string GetWalletAddress()
    {
        return walletAddress;
    }


    /// <summary>
    /// getting the privy wallet
    /// </summary>
    /// 
    public IEmbeddedSolanaWallet GetPrivyWalletInstance()
    {
        return privyWallet;
    }

    /// <summary>
    /// Approve transactions via Play Game button
    /// This is called when the user presses the Play Game button, indicating they approve Privy to sign transactions
    /// </summary>
    public void ApproveTransactionsViaPlayGame()
    {
        hasUserApprovedViaPlayGame = true;
        Debug.Log("Transactions approved via Play Game button. Privy will sign transactions automatically.");
    }

    /// <summary>
    /// Reset approval state (useful for testing or logout)
    /// </summary>
    public void ResetApproval()
    {
        hasUserApprovedViaPlayGame = false;
        Debug.Log("Transaction approval reset. User must press Play Game again to approve transactions.");
    }

    /// <summary>
    /// Quick diagnostic to confirm Privy can sign a trivial payload
    /// </summary>
    private async UniTaskVoid TestPrivySimpleSign()
    {
        try
        {
            await UniTask.SwitchToMainThread();
            var testMsg = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("hello"));
            Debug.Log("[Privy Diagnostic] Prompting simple SignMessage('hello' base64)...");
            var signTask = privyWallet?.EmbeddedSolanaWalletProvider?.SignMessage(testMsg);
            if (signTask == null)
            {
                Debug.LogError("[Privy Diagnostic] EmbeddedSolanaWalletProvider is null");
                return;
            }
            var completed = await Task.WhenAny(signTask, Task.Delay(TimeSpan.FromSeconds(20)));
            if (completed != signTask)
            {
                Debug.LogError("[Privy Diagnostic] SignMessage timed out (no modal?)");
                return;
            }
            var sig = await signTask;
            if (string.IsNullOrEmpty(sig))
            {
                Debug.LogError("[Privy Diagnostic] SignMessage returned empty signature");
                return;
            }
            Debug.Log($"[Privy Diagnostic] SignMessage success. Sig(len)={sig.Length}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[Privy Diagnostic] SignMessage error: {ex.Message}");
        }
    }
}

# Web3 Integration Guide - CrossyRoad Game

> **📖 Deep Dive Documentation**

> This is a comprehensive technical guide explaining all Web3/Solana integration details in the CrossyRoad Unity game.

> For quick start instructions, see [README.md](README.md).

This document explains all the Solana/Web3 integration steps implemented in the CrossyRoad Unity game, with detailed explanations of **why** each decision was made. Key topics include **Privy's batch transaction approval** (avoiding popups), **transaction batching** for performance, and the **custom wallet adapter** architecture.

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Setup & Prerequisites](#setup--prerequisites)
4. [Privy Authentication](#privy-authentication)
5. [Custom Privy Wallet Adapter](#custom-privy-wallet-adapter)
6. [Transaction Signing](#transaction-signing)
7. [Transaction Batching](#transaction-batching)
8. [Hybrid Transaction Service](#hybrid-transaction-service)
9. [Session Persistence](#session-persistence)
10. [Error Handling](#error-handling)
11. [Testing & Development](#testing--development)

---

## Overview

CrossyRoad is a Unity-based mobile game that combines classic endless runner gameplay with Solana blockchain integration. Players authenticate via Privy (email login), get embedded Solana wallets automatically, and send on-chain transactions for each movement. The game uses **Privy's batch approval mechanism** to avoid transaction popups during gameplay, combined with transaction batching to reduce fees.

**Key Features:**
- **Batch Transaction Approval**: User approves once via "Play Game" button, then all transactions sign automatically without popups
- Solana blockchain integration for wallet management and transaction signing
- Transaction batching to reduce fees (80% cost reduction)
- Custom wallet adapter bridging Privy SDK and Solana Unity SDK
- Real-time balance checking and transaction status updates

---

## Project Structure

The project follows a Unity-based architecture optimized for Web3 integration:

```
CrossyRoad/
├── Assets/
│   ├── _Project/
│   │   ├── Scripts/
│   │   │   ├── Managers/
│   │   │   │   ├── privy/
│   │   │   │   │   ├── AuthenticationFlowManager.cs      # Privy auth flow (⭐ KEY FILE)
│   │   │   │   │   └── CustomPrivyWalletAdapter.cs       # Custom wallet adapter (⭐ KEY FILE)
│   │   │   │   ├── Wallet/
│   │   │   │   │   └── SeekerWalletManager.cs           # Seeker wallet support
│   │   │   │   ├── MagicBlocks/
│   │   │   │   │   ├── MagicBlocksSolanaAdapter.cs     # Transaction batching (⭐ KEY FILE)
│   │   │   │   │   └── MagicBlocksConfig.cs
│   │   │   │   ├── GameManager.cs                        # Game state management
│   │   │   │   └── UI/
│   │   │   │       ├── TokenTransferPanel.cs
│   │   │   │       └── TransactionToastManager.cs
│   │   │   ├── Player/
│   │   │   │   ├── Components/
│   │   │   │   │   ├── HybridTransactionService.cs      # Hybrid transaction service (⭐ KEY FILE)
│   │   │   │   │   ├── PlayerTransactionService.cs
│   │   │   │   │   └── PlayerController.cs
│   │   │   │   └── Interfaces/
│   │   │   │       └── IAuthenticationManager.cs
│   │   │   ├── Data/
│   │   │   │   └── privy/
│   │   │   │       └── PrivyConfiguration.cs              # Privy config ScriptableObject
│   │   │   └── World/
│   │   │       └── WorldGenerator.cs
│   │   ├── Prefabs/                                      # Unity prefabs
│   │   ├── Scenes/                                       # Unity scenes
│   │   ├── Art/                                          # 3D models and materials
│   │   └── UI/                                           # UI assets and sprites
│   ├── Plugins/
│   │   └── Privy/                                        # Privy SDK plugin
│   ├── Resources/
│   │   └── SolanaUnitySDK/                               # Solana Unity SDK resources
│   └── Samples/
│       └── Solana SDK/                                   # Solana SDK samples
├── ProjectSettings/                                      # Unity project settings
└── Packages/                                            # Unity package dependencies
```

### Key Files for Web3 Integration

- **[AuthenticationFlowManager.cs](Assets/_Project/Scripts/Managers/privy/AuthenticationFlowManager.cs)**: Privy authentication flow management ⭐
  - Email login with OTP verification
  - Embedded wallet creation
  - Session management
  - Balance checking and UI updates

- **[CustomPrivyWalletAdapter.cs](Assets/_Project/Scripts/Managers/privy/CustomPrivyWalletAdapter.cs)**: Custom wallet adapter bridging Privy and Solana SDK ⭐
  - Privy wallet connection
  - Transaction building and signing
  - Balance queries
  - Transaction approval mechanism

- **[MagicBlocksSolanaAdapter.cs](Assets/_Project/Scripts/Managers/MagicBlocks/MagicBlocksSolanaAdapter.cs)**: Transaction batching system ⭐
  - Transaction queue management
  - Batch processing (time-based and size-based)
  - Integration with CustomPrivyWalletAdapter

- **[HybridTransactionService.cs](Assets/_Project/Scripts/Player/Components/HybridTransactionService.cs)**: Hybrid transaction service ⭐
  - Combines Privy auth with MagicBlocks SDK
  - Movement transaction creation
  - Transaction statistics and tracking

- **[PrivyConfiguration.cs](Assets/_Project/Scripts/Data/privy/PrivyConfiguration.cs)**: Privy configuration ScriptableObject
  - App ID and Client ID
  - Solana network settings
  - RPC endpoint configuration

---

## Setup & Prerequisites

### Required Dependencies

**Unity Packages:**
- Unity 6000.2.6f2 or compatible
- Universal Render Pipeline (URP)
- Unity Input System

**External SDKs:**
- Privy SDK (included in `Assets/Plugins/Privy/`)
- Solana Unity SDK v1.2.7 (included in `Assets/Resources/SolanaUnitySDK/`)

### Critical: Privy Configuration

**⚠️ IMPORTANT**: Privy SDK requires proper configuration before use.

#### Implementation: `PrivyConfiguration.cs`

```csharp
[CreateAssetMenu(fileName = "PrivyConfig", menuName = "Privy/Configuration")]
public class PrivyConfiguration : ScriptableObject
{
    [Header("Privy Settings")]
    public string appId = "your-app-id";
    public string clientId = "your-client-id";
    
    [Header("Solana Settings")]
    public bool enableSolana = true;
    public string solanaRpcUrl = "https://api.devnet.solana.com";
    public SolanaNetwork solanaNetwork = SolanaNetwork.Devnet;
}
```

### Why This Configuration?

#### 1. **ScriptableObject Pattern**

**What**: Using Unity ScriptableObject for configuration

**Why**:
- Easy to configure in Unity Inspector
- No code changes needed for different environments
- Can create multiple configs (devnet, mainnet)
- Version control friendly (can exclude sensitive data)

#### 2. **Separate App ID and Client ID**

**What**: Two separate identifiers from Privy dashboard

**Why**:
- App ID: Identifies your application
- Client ID: Used for client-side authentication
- Required by Privy SDK for initialization
- Different IDs for different environments (dev/prod)

#### 3. **Solana Network Configuration**

**What**: Configurable Solana network (devnet, testnet, mainnet)

**Why**:
- `devnet`: Free test SOL for development
- `mainnet-beta`: Real SOL for production
- Prevents accidental mainnet transactions during development
- Easy to switch between networks

### Native Module Setup

For Privy SDK to work properly, you need a mobile development build:

```bash
# In Unity Editor:
# File > Build Settings
# Select Android
# Click "Build and Run"
```

**Why Not Unity Editor (for full functionality)**:
- Privy SDK uses native WebView for transaction signing
- Native WebView only available in mobile builds
- Editor testing will timeout on transaction signing
- Must use mobile build for full functionality

### Verify Setup

Add this test to confirm Privy is initialized:

```csharp
// In AuthenticationFlowManager.cs
var authState = await privyInstance.GetAuthState();
Debug.Log($"Privy initialized. Auth state: {authState}");
```

**Expected Output**:
```
Privy initialized. Auth state: Unauthenticated
```

---

## Privy Authentication

### Why Privy?

**The Main Reason**: Privy enables **batch transaction approval** - users approve transactions once via the "Play Game" button, then all subsequent transactions sign automatically **without popups**. This is critical for games where players make many movements, as showing a popup for every transaction would ruin the gameplay experience.

**Additional Benefits**:
- **Email Login**: Simple email + OTP authentication (no wallet needed initially)
- **Embedded Wallets**: Automatically creates Solana wallets for users
- **No Seed Phrases**: Users don't manage private keys
- **Session Persistence**: Users stay logged in across app restarts

### Implementation: [AuthenticationFlowManager.cs](Assets/_Project/Scripts/Managers/privy/AuthenticationFlowManager.cs)

```csharp
private async Task InitializePrivy()
{
    var config = new Privy.PrivyConfig
    {
        AppId = privyConfig.appId,
        ClientId = privyConfig.clientId,
        LogLevel = logLevel
    };
    
    privyInstance = PrivyManager.Initialize(config);
    var authState = await privyInstance.GetAuthState();
    privyInstance.SetAuthStateChangeCallback(OnAuthStateChanged);
}
```

### Authentication Flow

**Email + OTP Login**:
```csharp
// Send OTP
bool codeSent = await privyInstance.Email.SendCode(userEmail);

// Verify OTP
var authState = await privyInstance.Email.LoginWithCode(userEmail, code);
```

**Automatic Wallet Creation**:
```csharp
// After email login, create Solana wallet if it doesn't exist
var user = await privyInstance.GetUser();
if (user.EmbeddedSolanaWallets == null || user.EmbeddedSolanaWallets.Length == 0)
{
    var newWallet = await user.CreateSolanaWallet();
    walletAddress = newWallet.Address;
}
```

---

## Custom Privy Wallet Adapter

### Implementation: [CustomPrivyWalletAdapter.cs](Assets/_Project/Scripts/Managers/privy/CustomPrivyWalletAdapter.cs)

The Custom Privy Wallet Adapter is a **critical bridge** between Privy SDK and Solana Unity SDK. It allows the game to use Privy's embedded wallets with Solana transaction signing.

```csharp
public class CustomPrivyWalletAdapter : MonoBehaviour
{
    private IEmbeddedSolanaWallet privyWallet;
    private IRpcClient rpcClient;
    private bool hasUserApprovedViaPlayGame = false;
    
    private async void InitializeAdapter()
    {
        // Get Privy wallet
        privyWallet = await GetPrivyWallet();
        walletAddress = privyWallet.Address;
        
        // Initialize Solana RPC client
        rpcClient = ClientFactory.GetClient("https://api.devnet.solana.com");
        
        isInitialized = true;
    }
}
```

### Why This Architecture?

#### 1. **Bridging Two SDKs**

**What**: Custom adapter connects Privy SDK and Solana Unity SDK

**Why**:
- **Different Interfaces**: Privy and Solana SDK have different APIs
- **Unified Interface**: Game code uses one consistent interface
- **Abstraction**: Changes to either SDK don't break game code

#### 2. **Transaction Approval Mechanism - The Key Feature**

**The Problem**: Without Privy, every transaction would require a wallet popup, interrupting gameplay constantly.

**The Solution**: User approves transactions **once** by pressing "Play Game" button. After approval, all transactions sign automatically without popups.

**How It Works**:
```csharp
// 1. User presses "Play Game" - approves all future transactions
public void StartGame()
{
    isGameReady = true;
    
    // This sets the approval flag - no more popups needed
    if (CustomPrivyWalletAdapter.Instance != null)
    {
        CustomPrivyWalletAdapter.Instance.ApproveTransactionsViaPlayGame();
    }
    
    GameManager.Instance.StartGame();
}

// 2. During gameplay, transactions check approval flag (no popup)
private async Task<string> SignAndSendTransactionInternal(Transaction transaction)
{
    // If user hasn't approved, reject transaction
    if (!hasUserApprovedViaPlayGame)
    {
        Debug.LogWarning("Transaction signing rejected: User has not approved via Play Game button.");
        return null;
    }
    
    // User approved, sign automatically without popup
    var signatureString = await privyWallet.EmbeddedSolanaWalletProvider.SignMessage(transactionBase64);
    // ... rest of signing flow
}
```

**Result**: Player makes 100 movements → Only 1 approval needed (at game start) → 100 transactions signed automatically

#### 3. **Transaction Building**

**What**: Build Solana transactions with memo instructions

**Why**:
- **Game Data**: Store movement data in transaction memo
- **On-chain Record**: Permanent record of gameplay
- **Low Cost**: Memo instructions are cheap (~5000 lamports)
- **No Custom Program**: Don't need to deploy a Solana program

**Implementation**:
```csharp
private async Task<Transaction> BuildSolanaTransaction(string message)
{
    var walletPubKey = new PublicKey(privyWallet.Address);
    
    // Get recent blockhash
    var blockHashResult = await rpcClient.GetLatestBlockHashAsync();
    var recentBlockHash = blockHashResult.Result.Value.Blockhash;
    
    // Create memo instruction with game data
    var instructions = new List<TransactionInstruction>();
    instructions.Add(MemoProgram.NewMemo(walletPubKey, message));
    
    // Build transaction
    var transaction = new Transaction
    {
        RecentBlockHash = recentBlockHash,
        FeePayer = walletPubKey,
        Instructions = instructions,
        Signatures = new List<SignaturePubKeyPair>()
    };
    
    return transaction;
}
```

#### 3. **Privy Signing Flow**

**What**: Sign transaction using Privy's embedded wallet provider

**How It Works**:
- Privy uses native WebView for signing (mobile builds only)
- Private keys never leave Privy's secure environment
- After user approval, signing happens automatically without popups

**Implementation**:
```csharp
private async Task<string> SignAndSendTransactionInternal(Transaction transaction)
{
    // Serialize transaction message
    var messageBytes = transaction.CompileMessage();
    var transactionBase64 = System.Convert.ToBase64String(messageBytes);
    
    // Request signature from Privy (no popup if user already approved)
    var signatureString = await privyWallet.EmbeddedSolanaWalletProvider.SignMessage(transactionBase64);
    
    // Attach signature and send to blockchain
    var signatureBytes = System.Convert.FromBase64String(signatureString);
    transaction.Signatures.Add(new SignaturePubKeyPair
    {
        PublicKey = walletPubKey,
        Signature = signatureBytes
    });
    
    var signedTransactionBytes = transaction.Serialize();
    var sendResult = await rpcClient.SendTransactionAsync(signedTransactionBytes);
    
    return sendResult.Result;
}
```

**⚠️ Important**: Privy signing only works in mobile builds. Unity Editor will timeout because native WebView is not available.

---

## Transaction Signing

### Implementation: [CustomPrivyWalletAdapter.cs](Assets/_Project/Scripts/Managers/privy/CustomPrivyWalletAdapter.cs)

```csharp
public async Task<string> SignAndSendTransaction(string message)
{
    // 1. Check balance before sending
    var balance = await GetPrivyBalance();
    if (balance < TRANSACTION_COST_PER_MOVE)
    {
        Debug.LogWarning("Insufficient balance");
        return null;
    }
    
    // 2. Build Solana transaction
    var transaction = await BuildSolanaTransaction(message);
    
    // 3. Sign and send with Privy
    var signature = await SignAndSendTransactionInternal(transaction);
    
    return signature;
}
```

### Why This Flow?

#### 1. **Pre-Flight Balance Check**

**What**: Check wallet balance before building transaction

**Why**:
- **Prevent Wasted RPC Calls**: Don't build transaction if insufficient funds
- **Better UX**: Show error message before attempting transaction
- **Save Fees**: No failed transaction fees
- **Game Flow**: Disable "Play Game" button if insufficient balance

**Implementation**:
```csharp
// Transaction cost per move (0.000005 SOL = 5000 lamports)
public const ulong TRANSACTION_COST_PER_MOVE = 5000;

// Check balance
var balance = await GetPrivyBalance();
if (balance < TRANSACTION_COST_PER_MOVE)
{
    // Show toast message
    TransactionToastManager.Instance.ShowToastBottom(
        "Please add SOL to continue playing",
        Color.red
    );
    return null;
}
```

#### 2. **Using Memo Instructions**

**What**: Store game data in transaction memo

**Why**:
- **Low Cost**: Memo instructions are cheap (~5000 lamports)
- **On-chain Data**: Permanent record of gameplay
- **Flexible**: Can include any game data
- **No Program Needed**: Don't need custom Solana program

**Message Format**:
```csharp
// Movement message format
var message = $"CrossyRoad_Move_({direction.x:F1},{direction.y:F1},{direction.z:F1})_{timestamp}";

// Batch message format
var batchMessage = $"BATCH_{count}_{walletAddress}_{timestamp}";
```

#### 3. **Recent Blockhash**

**What**: Get fresh blockhash for each transaction

**Why**:
- **Transaction Expiry**: Transactions expire after ~150 blocks (~1-2 minutes)
- **Prevent Replay**: Old blockhashes prevent replay attacks
- **Required**: Solana requires recent blockhash for transactions
- **Fresh Blockhash**: Get new one for each transaction

#### 4. **Balance Checking**

**What**: Query wallet balance from Solana RPC

**Why**:
- **UI Updates**: Show balance in welcome panel
- **Game Logic**: Enable/disable play button based on balance
- **Transaction Validation**: Check before sending
- **User Feedback**: Show low balance warnings

**Implementation**:
```csharp
public async Task<ulong> GetPrivyBalance()
{
    var pubKey = new PublicKey(walletAddress);
    var balanceResult = await rpcClient.GetBalanceAsync(pubKey);
    
    if (balanceResult.WasSuccessful)
    {
        return balanceResult.Result.Value; // Returns lamports
    }
    
    return 0;
}
```

**Balance Display**:
```csharp
// Convert lamports to SOL for display
double solBalance = balance / 1_000_000_000.0;
balanceText.text = $"Balance: {solBalance:F6} SOL";
```

---

## Transaction Batching

### Implementation: [MagicBlocksSolanaAdapter.cs](Assets/_Project/Scripts/Managers/MagicBlocks/MagicBlocksSolanaAdapter.cs)

Transaction batching is a **critical performance optimization** that groups multiple game movements into a single blockchain transaction, reducing fees and improving game performance.

```csharp
public class MagicBlocksSolanaAdapter : MonoBehaviour
{
    private Queue<string> pendingTransactions = new Queue<string>();
    private float lastBatchTime = 0f;
    private const float BATCH_INTERVAL = 2f; // 2 seconds
    private int batchSize = 5; // 5 transactions per batch
    
    public async Task<string> SendTransaction(string message)
    {
        if (enableTransactionBatching)
        {
            return await SendBatchedTransaction(message);
        }
        else
        {
            return await SendImmediateTransaction(message);
        }
    }
}
```

### Why Batching?

#### 1. **Cost Efficiency**

**What**: Group multiple movements into one transaction

**Why**:
- **Lower Fees**: One transaction fee instead of many
- **Example**: 10 movements = 1 transaction (saves 9x fees)
- **Better UX**: Players can play longer with same SOL balance
- **Scalable**: Works for high-frequency gameplay

**Cost Comparison**:
```
Without Batching:
- 100 movements = 100 transactions = 500,000 lamports (0.0005 SOL)

With Batching (5 per batch):
- 100 movements = 20 transactions = 100,000 lamports (0.0001 SOL)
- Savings: 80% reduction in fees
```

#### 2. **Performance Optimization**

**What**: Reduce blockchain interaction frequency

**Why**:
- **Faster Gameplay**: Less waiting for transaction confirmations
- **Lower Latency**: Batch transactions don't block game loop
- **Better UX**: Game feels more responsive
- **Network Efficiency**: Fewer RPC calls

#### 3. **Batching Strategy**

**What**: Two triggers for batch processing

**Why**:
- **Size-based**: Send when batch reaches 5 transactions
- **Time-based**: Send after 2 seconds (even if < 5)
- **Balance**: Prevents stale batches
- **Flexible**: Can adjust thresholds

**Implementation**:
```csharp
private bool ShouldSendBatch()
{
    return pendingTransactions.Count >= batchSize ||
           (Time.time - lastBatchTime) >= BATCH_INTERVAL;
}

private async Task<string> SendBatchedTransaction(string message)
{
    // Add to queue
    pendingTransactions.Enqueue(message);
    
    // Check if should send batch
    if (ShouldSendBatch())
    {
        return await ProcessBatch();
    }
    
    // Return batch ID for tracking
    return $"BATCH_{pendingTransactions.Count}_{Time.time}";
}
```

#### 4. **Batch Message Format**

**What**: Combine multiple movements into single message

**Why**:
- **Single Transaction**: All movements in one memo
- **Efficient**: One signature for multiple movements
- **Traceable**: Batch ID for tracking
- **Flexible**: Can include metadata

**Implementation**:
```csharp
private async Task<string> ProcessBatch()
{
    // Create batch message
    var batchMessages = string.Join("|", pendingTransactions);
    var batchMessage = $"BATCH_{pendingTransactions.Count}_{walletAddress}_{DateTime.UtcNow:yyyyMMddHHmmss}";
    
    // Send batch transaction
    var batchSignature = await customPrivyAdapter.SignAndSendTransaction(batchMessage);
    
    // Clear batch
    pendingTransactions.Clear();
    lastBatchTime = Time.time;
    
    return batchSignature;
}
```

**Batch Message Example**:
```
BATCH_5_7xKXtg2CW87d97TXJSDpbD5jBkheTqA83TZRuJosgAsU_20240115143022
```

#### 5. **Configurable Batching**

**What**: Enable/disable batching via Inspector

**Why**:
- **Testing**: Can test immediate transactions
- **Flexibility**: Adjust based on game needs
- **Performance Tuning**: Find optimal batch size
- **Debugging**: Easier to debug without batching

**Configuration**:
```csharp
[Header("Transaction Settings")]
[SerializeField] private bool enableTransactionBatching = true;
[SerializeField] private int batchSize = 5;
```

---

## Hybrid Transaction Service

### Implementation: [HybridTransactionService.cs](Assets/_Project/Scripts/Player/Components/HybridTransactionService.cs)

The Hybrid Transaction Service combines Privy authentication with MagicBlocks SDK for a unified transaction interface.

```csharp
public class HybridTransactionService : MonoBehaviour
{
    private IAuthenticationManager authenticationManager;
    private MagicBlocksSolanaAdapter magicBlocksAdapter;
    
    public async void SendMovementTransaction(Vector3 direction)
    {
        // 1. Create movement message
        var message = CreateMovementMessage(direction);
        
        // 2. Verify Privy authentication
        if (!await VerifyPrivyAuthentication())
        {
            return;
        }
        
        // 3. Send transaction via MagicBlocks (with batching)
        var signature = await magicBlocksAdapter.SendTransaction(message);
        
        // 4. Handle success/failure
        if (!string.IsNullOrEmpty(signature))
        {
            OnTransactionSuccess?.Invoke(signature);
        }
    }
}
```

### Why This Architecture?

#### 1. **Separation of Concerns**

**What**: Separate authentication from transaction logic

**Why**:
- **Modularity**: Separates authentication from transaction logic
- **Testability**: Can mock authentication for testing
- **Maintainability**: Changes to auth don't affect transactions

#### 2. **Unified Interface**

**What**: Single interface for all transaction operations

**Why**:
- **Consistency**: Game code uses one service
- **Abstraction**: Hides complexity of Solana/Privy integration
- **Simplicity**: Game code doesn't need to know about Privy/MagicBlocks

#### 3. **Movement Message Creation**

**What**: Create standardized movement messages

**Why**:
- **Consistency**: All movements use same format
- **Traceable**: Can track movements on-chain
- **Debuggable**: Easy to parse and analyze
- **Extensible**: Can add more data later

**Implementation**:
```csharp
private string CreateMovementMessage(Vector3 direction)
{
    var timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
    var directionStr = $"({direction.x:F1},{direction.y:F1},{direction.z:F1})";
    
    return $"CrossyRoad_Move_{directionStr}_{timestamp}";
}
```

**Message Format**:
```
CrossyRoad_Move_(0.0,0.0,1.0)_2024-01-15T14:30:22Z
```

#### 4. **Authentication Verification**

**What**: Verify Privy authentication before sending transactions

**Why**:
- **Security**: Ensure user is authenticated
- **Error Prevention**: Don't send transactions if not authenticated
- **User Feedback**: Show error if authentication fails
- **Game Flow**: Prevent gameplay without wallet

**Implementation**:
```csharp
private async Task<bool> VerifyPrivyAuthentication()
{
    // Check if game is ready (user authenticated)
    if (!authenticationManager.IsGameReady())
    {
        return false;
    }
    
    // Ensure Solana wallet exists
    var hasWallet = await authenticationManager.EnsureSolanaWallet();
    if (!hasWallet)
    {
        return false;
    }
    
    // Verify wallet address
    var walletAddress = await authenticationManager.GetSolanaWalletAddress();
    if (string.IsNullOrEmpty(walletAddress) || walletAddress == "No Wallet")
    {
        return false;
    }
    
    return true;
}
```

---

## Session Persistence

### Implementation

Session persistence is handled automatically by Privy SDK, but the game implements additional checks for wallet state.

```csharp
// In AuthenticationFlowManager.cs
private async void StartAuthenticationFlow()
{
    var authState = await privyInstance.GetAuthState();
    if (authState == AuthState.Authenticated)
    {
        // User already authenticated, check wallet
        CheckWalletStatus();
    }
    else
    {
        // Show login screen
        ShowAuthPanel();
    }
}
```

### Why Persist Sessions?

#### 1. **Privy SDK Automatic Persistence**

**What**: Privy SDK automatically persists authentication state

**Why**:
- **No Re-login**: Users stay logged in across app restarts
- **Secure Storage**: Privy handles secure token storage
- **Automatic**: No manual session management needed

#### 2. **Wallet State Checking**

**What**: Verify wallet exists on app restart

**Why**:
- **Consistency**: Ensure wallet is available
- **Error Prevention**: Handle missing wallet gracefully
- **User Experience**: Auto-create wallet if missing
- **Game Flow**: Prevent gameplay without wallet

**Implementation**:
```csharp
private async void CheckWalletStatus()
{
    var user = await privyInstance.GetUser();
    var embeddedSolanaWallets = user.EmbeddedSolanaWallets;
    
    if (embeddedSolanaWallets != null && embeddedSolanaWallets.Length > 0)
    {
        // Wallet exists, show welcome panel
        walletAddress = embeddedSolanaWallets[0].Address;
        hasWallet = true;
        ShowWelcomePanel();
    }
    else
    {
        // No wallet, create one
        var newWallet = await user.CreateSolanaWallet();
        walletAddress = newWallet.Address;
        hasWallet = true;
        ShowWelcomePanel();
    }
}
```

#### 3. **Logout Handling**

**What**: Clear session on logout

**Why**:
- **Security**: User expects full disconnect
- **Privacy**: Clear wallet state
- **Fresh Start**: Allows new login
- **Best Practice**: Standard logout behavior

**Implementation**:
```csharp
public void Logout()
{
    if (privyInstance == null) return;
    
    privyInstance.Logout();
    
    // Reset game state
    isAuthenticated = false;
    hasWallet = false;
    walletAddress = "";
    
    // Reset transaction approval
    if (CustomPrivyWalletAdapter.Instance != null)
    {
        CustomPrivyWalletAdapter.Instance.ResetApproval();
    }
    
    ShowAuthPanel();
}
```

---

## Error Handling

### Common Issues & Solutions

#### Issue: "Privy SDK not initialized" Error

**Cause**: Privy configuration missing or invalid

**Solution**:
1. Check `PrivyConfiguration.asset` exists
2. Verify App ID and Client ID are set
3. Ensure configuration is assigned in `AuthenticationFlowManager`
4. Check Unity console for initialization errors

**Why It Happens**:
- Missing ScriptableObject asset
- Invalid App ID/Client ID
- Configuration not assigned in Inspector

#### Issue: "Transaction signing timed out" Error

**Cause**: Privy signing only works in mobile builds

**Solution**:
1. Build for Android (not Unity Editor)
2. Test on physical device
3. Check network connection

**Why It Happens**:
- Privy uses native WebView for signing
- WebView not available in Unity Editor
- Editor testing will always timeout

#### Issue: "Insufficient balance" Error

**Cause**: Wallet doesn't have enough SOL for transaction

**Solution**:
1. Add SOL to wallet via faucet (devnet) or transfer (mainnet)
2. Check balance in welcome panel
3. Ensure balance > transaction cost (5000 lamports per move)
4. Use Solana faucet for devnet: https://faucet.solana.com/

**Transaction Costs**:
- Per move: 5000 lamports (0.000005 SOL)
- With batching (5 per batch): 5000 lamports per batch
- Recommended minimum: 0.001 SOL for testing

#### Issue: "Custom Privy Wallet Adapter not ready" Error

**Cause**: Adapter not initialized or wallet not connected

**Solution**:
1. Ensure user is authenticated via Privy
2. Wait for adapter initialization (happens after auth)
3. Check `CustomPrivyWalletAdapter.Instance.IsReady()`
4. Verify wallet was created successfully

**Initialization Flow**:
```csharp
// Adapter waits for authentication
private System.Collections.IEnumerator WaitForAuthenticationAndInitialize()
{
    while (AuthenticationFlowManager.Instance == null || 
           !AuthenticationFlowManager.Instance.IsAuthenticated)
    {
        yield return new WaitForSeconds(0.5f);
    }
    
    InitializeAdapter();
}
```

#### Issue: "Transaction expired" Error

**Cause**: Blockhash too old (>150 blocks)

**Solution**:
1. Get fresh blockhash before each transaction
2. Reduce time between building and sending transaction
3. Check network latency
4. Implement retry with fresh blockhash

**Why It Happens**:
- Transactions expire after ~150 blocks (~1-2 minutes)
- Old blockhashes are rejected by network
- Network latency can cause expiry

#### Issue: "Batch transaction failed" Error

**Cause**: One of the batched transactions failed

**Solution**:
1. Check individual transaction messages
2. Verify wallet balance is sufficient
3. Check network connection
4. Review batch size (may be too large)

**Error Handling**:
```csharp
private async Task<string> ProcessBatch()
{
    try
    {
        // Process batch...
    }
    catch (Exception ex)
    {
        // Clear pending transactions on error
        pendingTransactions.Clear();
        OnTransactionFailed?.Invoke(ex.Message);
        return null;
    }
}
```

---

## Testing & Development

### Development Workflow

#### 1. **Setup Devnet Environment**

**What**: Use Solana devnet for development

**Why**:
- **Free SOL**: Devnet faucet provides free test SOL
- **No Risk**: Can't lose real money
- **Fast**: Devnet is faster for testing
- **Isolated**: Doesn't affect mainnet

**Configuration**:
```csharp
// In PrivyConfiguration.cs
public string solanaRpcUrl = "https://api.devnet.solana.com";
public SolanaNetwork solanaNetwork = SolanaNetwork.Devnet;
```

#### 2. **Get Devnet SOL**

**Steps**:
1. Authenticate in game
2. Copy wallet address from welcome panel
3. Visit https://faucet.solana.com/
4. Paste address and request SOL
5. Wait for airdrop (usually instant)

#### 3. **Testing Transaction Flow**

**Steps**:
1. Build for Android
2. Install on device
3. Authenticate via email
4. Add devnet SOL to wallet
5. Press "Play Game" to approve transactions (this enables auto-signing)
6. Play game and verify transactions on Solana Explorer

**Solana Explorer**:
```
https://explorer.solana.com/tx/{signature}?cluster=devnet
```

#### 4. **Debugging Transactions**

**What**: Use Unity console and Solana Explorer

**Why**:
- **Unity Console**: See transaction signatures and errors
- **Solana Explorer**: Verify transactions on-chain
- **Transaction Details**: See memo data, fees, confirmations
- **Error Messages**: Understand why transactions failed

**Debug Logs**:
```csharp
Debug.Log($"Transaction sent successfully! Signature: {signature}");
Debug.Log($"View on explorer: https://explorer.solana.com/tx/{signature}?cluster=devnet");
```

### Important Constants

**Transaction Costs**:
```csharp
// In CustomPrivyWalletAdapter.cs
public const ulong TRANSACTION_COST_PER_MOVE = 5000; // 0.000005 SOL

// In AuthenticationFlowManager.cs
const ulong transactionCostPerMove = 5000;
const ulong lowBalanceThreshold = transactionCostPerMove * 3; // 3 moves
const ulong minBalanceThreshold = transactionCostPerMove * 4; // 4 moves
```

**Batching Configuration**:
```csharp
// In MagicBlocksSolanaAdapter.cs
[SerializeField] private bool enableTransactionBatching = true;
[SerializeField] private int batchSize = 5;
private const float BATCH_INTERVAL = 2f; // 2 seconds
```

**RPC Endpoints**:
```csharp
// Devnet
"https://api.devnet.solana.com"

// Mainnet
"https://api.mainnet-beta.solana.com"
```

---

## Resources

### Official Documentation

- [Privy Documentation](https://docs.privy.io)
- [Solana Unity SDK](https://github.com/magicblock-labs/Solana.Unity-SDK)
- [Solana Web3.js Docs](https://solana-labs.github.io/solana-web3.js/)
- [Solana Cookbook](https://solanacookbook.com/)

### Developer Tools

- [Solana Explorer (Devnet)](https://explorer.solana.com/?cluster=devnet)
- [Solana Faucet](https://faucet.solana.com/)
- [Solana CLI Tools](https://docs.solana.com/cli/install-solana-cli-tools)

### Sample Code

- [Privy Unity SDK Examples](https://github.com/privy-io/unity-sdk)
- [Solana Unity SDK Samples](Assets/Samples/Solana SDK/)

---

## Key Design Decisions Summary

### Why Privy?

1. **Batch Transaction Approval**: User approves once, all transactions sign automatically without popups
2. **User-Friendly**: Email login (no crypto knowledge needed)
3. **Embedded Wallets**: Automatic Solana wallet creation
4. **Secure**: Private keys managed by Privy
5. **No Backend**: Privy handles authentication

### Why Transaction Batching?

1. **Cost Efficiency**: 80% reduction in fees
2. **Performance**: Faster gameplay, less blockchain interaction
3. **Scalability**: Works for high-frequency gameplay
4. **User Experience**: Players can play longer with same balance

### Why Custom Wallet Adapter?

1. **Abstraction**: Hides complexity of Privy + Solana integration
2. **Unified Interface**: Game code uses one consistent API
3. **Testability**: Can mock adapter for testing
4. **Maintainability**: Changes to SDKs don't break game code

### Why Memo Instructions?

1. **Low Cost**: Cheap way to store game data on-chain
2. **No Program Needed**: Don't need custom Solana program
3. **Flexible**: Can include any game data
4. **On-chain Record**: Permanent record of gameplay

---

This technical guide covers all aspects of Web3 integration in the CrossyRoad game. For implementation details, refer to the source code files mentioned throughout this document.


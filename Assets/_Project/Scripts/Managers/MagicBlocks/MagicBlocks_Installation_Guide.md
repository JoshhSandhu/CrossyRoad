# MagicBlocks Solana Unity SDK Installation Guide

## Overview
This guide will help you install and configure the MagicBlocks Solana Unity SDK for reliable Solana transactions in your CrossyRoad game.

## Step 1: Install MagicBlocks SDK

### Option A: Unity Package Manager (Recommended)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click the `+` button and select `Add package from git URL`
3. Enter the MagicBlocks SDK URL:
   ```
   https://github.com/magicblocks/solana-unity-sdk.git
   ```
4. Click `Add`

### Option B: Manual Installation
1. Download the MagicBlocks SDK from: https://github.com/magicblocks/solana-unity-sdk
2. Extract the package to your project's `Packages` folder
3. Update `Packages/manifest.json` to include:
   ```json
   {
     "dependencies": {
       "com.magicblocks.solana": "file:../Packages/magicblocks-solana-unity-sdk"
     }
   }
   ```

## Step 2: Configure MagicBlocks

### Create Configuration Asset
1. Right-click in Project window
2. Select `Create > MagicBlocks > Configuration`
3. Name it `MagicBlocksConfig`
4. Configure the settings:
   - **RPC Endpoint**: `https://api.devnet.solana.com`
   - **Network**: `devnet`
   - **Enable Batching**: `true`
   - **Batch Size**: `5`
   - **Transaction Timeout**: `5`
   - **Enable Privy Sync**: `true`

### Assign Configuration
1. Select the `MagicBlocksSolanaAdapter` in your scene
2. Assign the `MagicBlocksConfig` asset to the `Config` field

## Step 3: Update Code for MagicBlocks Integration

### Update MagicBlocksSolanaAdapter.cs
Replace the placeholder code with actual MagicBlocks SDK calls:

```csharp
// Replace this placeholder:
// walletAdapter = new SolanaWalletAdapter();

// With actual MagicBlocks initialization:
using MagicBlocks.Solana;

private SolanaWalletAdapter walletAdapter;
private SolanaRpcClient rpcClient;

private async void InitializeMagicBlocks()
{
    try
    {
        Debug.Log("Initializing MagicBlocks Solana SDK...");
        
        // Initialize MagicBlocks SDK
        walletAdapter = new SolanaWalletAdapter();
        rpcClient = new SolanaRpcClient(config.GetNetworkConfig());
        
        await walletAdapter.Initialize();
        await ConnectToPrivyWallet();
        
        Debug.Log("MagicBlocks SDK initialized successfully");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to initialize MagicBlocks SDK: {ex.Message}");
    }
}
```

### Update Transaction Methods
Replace mock implementations with real MagicBlocks calls:

```csharp
private async Task<string> SendImmediateTransaction(string message)
{
    try
    {
        // Create transaction
        var transaction = CreateMovementTransaction(message);
        
        // Sign and send transaction
        var signature = await walletAdapter.SignAndSendTransaction(transaction);
        
        Debug.Log($"MagicBlocks: Transaction sent - Signature: {signature}");
        OnTransactionSent?.Invoke(signature);
        
        return signature;
    }
    catch (Exception ex)
    {
        Debug.LogError($"MagicBlocks transaction failed: {ex.Message}");
        return null;
    }
}
```

## Step 4: Test the Integration

### Test Transaction Flow
1. Play the game
2. Move the player
3. Check console for MagicBlocks logs
4. Verify transactions appear in Solana Explorer

### Debug Commands
Add these debug methods to test the integration:

```csharp
[ContextMenu("Test MagicBlocks Connection")]
public async void TestMagicBlocksConnection()
{
    if (magicBlocksAdapter != null)
    {
        var isReady = magicBlocksAdapter.IsReady();
        Debug.Log($"MagicBlocks Ready: {isReady}");
        
        if (isReady)
        {
            var walletAddress = await magicBlocksAdapter.GetWalletAddress();
            Debug.Log($"MagicBlocks Wallet: {walletAddress}");
        }
    }
}

[ContextMenu("Test Transaction")]
public async void TestTransaction()
{
    if (hybridTransactionService != null)
    {
        hybridTransactionService.SendMovementTransaction(Vector3.forward);
    }
}
```

## Step 5: Performance Optimization

### Enable Transaction Batching
1. Set `Enable Batching` to `true` in MagicBlocksConfig
2. Configure `Batch Size` (recommended: 5-10)
3. Set `Batch Interval` (recommended: 2-5 seconds)

### Network Configuration
- **Devnet**: Use for testing and development
- **Mainnet**: Use for production (requires real SOL)
- **Testnet**: Use for extensive testing

## Step 6: Privy Dashboard Integration

### Webhook Configuration (Optional)
If you have a Privy webhook URL:
1. Set `Enable Privy Sync` to `true`
2. Enter your webhook URL in `Privy Webhook Url`
3. Set sync interval (recommended: 10 seconds)

### Manual Dashboard Sync
The system will automatically log transactions to the Privy dashboard through the hybrid service.

## Troubleshooting

### Common Issues

#### 1. SDK Not Found
**Error**: `MagicBlocks SDK not found`
**Solution**: Ensure the SDK is properly installed via Package Manager

#### 2. Wallet Connection Failed
**Error**: `Failed to connect to Privy wallet`
**Solution**: 
- Verify Privy authentication is working
- Check wallet address is valid
- Ensure network configuration is correct

#### 3. Transaction Timeout
**Error**: `Transaction timed out`
**Solution**:
- Increase `Transaction Timeout` in config
- Check network connectivity
- Verify RPC endpoint is accessible

#### 4. Batch Processing Issues
**Error**: `Batch processing failed`
**Solution**:
- Reduce `Batch Size`
- Increase `Batch Interval`
- Check for memory issues

### Debug Logging
Enable detailed logging in MagicBlocksConfig:
- Set `Enable Debug Logging` to `true`
- Set `Log All Transactions` to `true`
- Check console for detailed transaction logs

## Production Checklist

Before deploying to production:

- [ ] Switch to mainnet configuration
- [ ] Set up proper RPC endpoints
- [ ] Configure transaction fees
- [ ] Test with real SOL
- [ ] Set up monitoring and alerts
- [ ] Configure backup RPC endpoints
- [ ] Test transaction batching
- [ ] Verify Privy dashboard integration
- [ ] Test error handling and fallbacks
- [ ] Performance test with high transaction volume

## Support

For issues with MagicBlocks SDK:
- GitHub: https://github.com/magicblocks/solana-unity-sdk
- Documentation: https://docs.magicblocks.io
- Discord: https://discord.gg/magicblocks

For CrossyRoad integration issues:
- Check console logs for detailed error messages
- Verify all configuration settings
- Test with mock transactions first
- Use debug commands for testing

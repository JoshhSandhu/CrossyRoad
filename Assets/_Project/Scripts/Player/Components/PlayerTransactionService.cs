using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Handles Solana transactions for player movement
/// </summary>
public class PlayerTransactionService : ITransactionService
{
    private readonly IAuthenticationManager authenticationManager;

    public PlayerTransactionService(IAuthenticationManager authenticationManager)
    {
        this.authenticationManager = authenticationManager;
    }

    public async void SendMovementTransaction(Vector3 direction)
    {
        //Debug.Log("SendMovementTransaction called with direction: " + direction);
        if (authenticationManager == null)
        {
            Debug.LogWarning("AuthenticationManager is null. Cannot send movement transaction.");
            return;
        }

        try
        {
            //Debug.Log("Checking if user has Solana wallet...");
            //first ensure the user has a solana wallet
            if (!await authenticationManager.EnsureSolanaWallet())
            {
                Debug.LogWarning("No solana wallet avilable, skipping transaction");
                return;
            }
            //Debug.Log("Solana wallet check passed");
            //create movement message
            string directionText = GetDirectionText(direction);
            string message = $"Crossy Road: Player moved {directionText} at {System.DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}";
            //Debug.Log($"Sending Solana transaction: {message}");

            string walletAddress = await authenticationManager.GetSolanaWalletAddress();
            //Debug.Log($"From Solana wallet: {walletAddress}");


        }
        catch (Exception ex)
        {
            Debug.LogError($"Error sending Solana transaction: {ex.Message}");
        }
    }

    private string GetDirectionText(Vector3 direction)
    {
        if (direction == Vector3.forward) return "FORWARD";
        if (direction == Vector3.back) return "BACKWARD";
        if (direction == Vector3.left) return "LEFT";
        if (direction == Vector3.right) return "RIGHT";
        return "UNKNOWN";
    }
}

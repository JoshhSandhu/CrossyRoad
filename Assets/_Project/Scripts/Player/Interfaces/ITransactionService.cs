using UnityEngine;

/// <summary>
/// Interface for handling Solana transactions
/// </summary>
public interface ITransactionService
{
    void SendMovementTransaction(Vector3 direction);
}

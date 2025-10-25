using UnityEngine;

/// <summary>
/// Interface for managing game state interactions
/// </summary>
public interface IGameStateManager
{
    bool IsGameActive();
    void StartGame();
    void AddCoins();
}

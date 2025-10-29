using UnityEngine;

/// <summary>
/// Concrete implementation of IGameStateManager using GameManager singleton
/// </summary>
public class GameStateManagerAdapter : IGameStateManager
{
    public bool IsGameActive()
    {
        return GameManager.Instance != null && GameManager.Instance.IsGameActive();
    }

    public void StartGame()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
    }

    public void AddCoins()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoins();
        }
    }
}

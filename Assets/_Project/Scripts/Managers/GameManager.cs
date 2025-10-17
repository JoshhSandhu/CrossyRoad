using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Transform playerTransform { get; private set; }

    public int Coins { get; private set; }
    public int Score { get; private set; }
    public static event Action<int> OnCoinsChanged;
    public static event Action OnGameReset;

    private WorldGenerator worldGenerator;
    private bool gameActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }

        //getting the world gen reference
        worldGenerator = FindAnyObjectByType<WorldGenerator>();
    }

    private void OnEnable()
    {
        StartScreenManager.OnGameStart += StartGame;
        PlayerController.OnScoreChanged += UpdateScore;
        PlayerController.OnPlayerMovedForward += OnPlayerMovedForward;
    }

    private void OnDisable()
    {
        StartScreenManager.OnGameStart -= StartGame;
        PlayerController.OnScoreChanged -= UpdateScore;
        PlayerController.OnPlayerMovedForward -= OnPlayerMovedForward;
    }

    public void StartGame()
    {
        gameActive = true;
        ResetGameState();
    }
    public bool IsGameActive()
    {
        return gameActive;
    }

    private void OnPlayerMovedForward()
    {
        if (!gameActive)
        {
            return;
        }
        Score++;
    }
    private void UpdateScore(int newScore)
    {
        Score = newScore;
    }
    public void AddCoins()
    {
        Coins++;
        OnCoinsChanged?.Invoke(Coins);
    }

    public void RestartGame()
    {
        ResetGameState();
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideGameOver();
        }
        //fade screen then the player is returned to the start screen
        if (FadeTransition.Instance != null)
        {
            FadeTransition.Instance.CyanFade(() =>
            {
                ResetPlayerAndCamera();
                //reset world generation during fade
                ResetWorldInBackground();

                //return to start screen after transition
                if (StartScreenManager.Instance != null)
                {
                    StartScreenManager.Instance.RestartToStartScreen();
                }

                OnGameReset?.Invoke();
            });
        }
        else
        {
            ResetPlayerAndCamera();
            //fallback if no fade transition available
            ResetWorldInBackground();
            if (StartScreenManager.Instance != null)
            {
                StartScreenManager.Instance.RestartToStartScreen();
            }
            OnGameReset?.Invoke();
        }
    }

    private void ResetPlayerAndCamera()
    {
        //reset player position and state
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            PlayerController playerController = playerObject.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //use the new reset method
                playerController.ResetPlayer();
            }
        }
    }

    private void ResetGameState()
    {
        Coins = 0;
        Score = 0;
        OnCoinsChanged?.Invoke(Coins);
    }

    //resetting the world during the fade transition
    private void ResetWorldInBackground()
    {
        if (worldGenerator != null)
        {
            worldGenerator.ResetWorld();
        }
    }
}

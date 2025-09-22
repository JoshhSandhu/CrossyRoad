using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public Transform playerTransform { get; private set; }

    public int Coins { get; private set; }
    public static event Action<int> OnCoinsChanged;

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
    }

    public void AddCoins()
    {
        Coins++;
        OnCoinsChanged?.Invoke(Coins);
    }
}

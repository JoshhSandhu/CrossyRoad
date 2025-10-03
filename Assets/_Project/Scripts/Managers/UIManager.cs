using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private TextMeshProUGUI scoreText;

    [SerializeField]
    private TextMeshProUGUI coinText;

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
    }

    private void OnEnable()
    {
        PlayerController.OnScoreChanged += UpdateScoreText;
        GameManager.OnCoinsChanged += UpdateCoinsText;
        GameManager.OnGameReset += OnGameReset;
    }

    private void OnDisable()
    {
        PlayerController.OnScoreChanged -= UpdateScoreText;
        GameManager.OnCoinsChanged -= UpdateCoinsText;
        GameManager.OnGameReset -= OnGameReset;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        UpdateScoreText(0);
        UpdateCoinsText(0);
    }

    private void UpdateScoreText(int newScore)
    {
        scoreText.text = "Score: " + newScore.ToString();
    }

    private void UpdateCoinsText(int newCoins)
    {
        coinText.text = "Coins: " + newCoins.ToString();
    }

    public void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
    }

    public void RestartGame()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }

    private void OnGameReset()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        UpdateScoreText(0);
        UpdateCoinsText(0);
    }

    public void HideGameOver()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
}

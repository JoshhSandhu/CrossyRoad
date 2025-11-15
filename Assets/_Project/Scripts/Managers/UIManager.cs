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
    private GameObject fadeTransition;

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
        scoreText.text = newScore.ToString();
    }

    private void UpdateCoinsText(int newCoins)
    {
        coinText.text = newCoins.ToString();
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

    //shop functioality
    public void OpenShop()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop();
        }
        else
        {
            Debug.LogWarning("shop manager not found!");
        }
    }

    public void CloseShop()
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.CloseShop();
        }
    }

    public void OpenTokenPanel()
    {
        if (AuthenticationFlowManager.Instance != null)
        {
            AuthenticationFlowManager.Instance.OpenTokenPanel();
        }
        else
        {
            Debug.LogWarning("AuthenticationFlowManager not found!");
        }
    }
}

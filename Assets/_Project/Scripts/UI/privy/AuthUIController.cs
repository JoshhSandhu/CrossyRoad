using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class AuthUIController : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("auth panel elements")]
    [SerializeField] private Button connectionWalletButton;
    [SerializeField] private Button loginWithEmailButton;
    [SerializeField] private TextMeshProUGUI authTitleText;
    [SerializeField] private TextMeshProUGUI authDescriptionText;

    [Header("wlecome panel elements")]
    [SerializeField] private TextMeshProUGUI welcomeTitleText;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button playGameButton;

    [Header("Loading panel elements")]
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private Slider loadingProgressBar;

    [Header("animation settings")]
    [SerializeField] private float panelTransitionDuration = 0.5f;
    [SerializeField] AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private CanvasGroup authCanvasGroup;
    private CanvasGroup welcomeCanvasGroup;
    private CanvasGroup loadingCanvasGroup;

    private void Start()
    {
        
    }

    private void InitializeUI()
    {
        //geting the canvas groups for smooth transitions
        authCanvasGroup = authPanel?.GetComponent<CanvasGroup>();
        welcomeCanvasGroup = welcomePanel?.GetComponent<CanvasGroup>();
        loadingCanvasGroup = loadingPanel?.GetComponent<CanvasGroup>();


        //setting up init state
        if(authPanel != null) authPanel.SetActive(true);
        if(welcomePanel != null) welcomePanel.SetActive(false);
        if(loadingPanel != null) loadingPanel.SetActive(false);

        //setting init text
        if(authTitleText != null) authTitleText.text = "Welcome to Crossy Road";
        if(authDescriptionText != null) authDescriptionText.text = "Connect your wallet to start playing and unlock exclusive skins!";
        if(welcomeTitleText != null) welcomeTitleText.text = "Ready To Play";
    }

    private void SetupEventListeners()
    {
        if(connectionWalletButton != null)
        {
            connectionWalletButton.onClick.AddListener(OnConnectWalletClicked);
        }
        if(loginWithEmailButton != null)
        {
            loginWithEmailButton.onClick.AddListener(OnLoginWithEmailClicked);
        }
        if(logoutButton != null)
        {
            logoutButton.onClick.AddListener(OnLogoutClicked);
        }
        if(playGameButton != null)
        {
            playGameButton.onClick.AddListener(OnPlayGameClicked);
        }
    }

    private void SubscribeToAuthEvents()
    {
        PrivyAuthManager.OnAuthenticationStateChanged += OnAuthenticationStateChanged;
        PrivyAuthManager.OnWalletAddressChanged += OnWalletAddressChanged;
        PrivyAuthManager.OnUserInfoChanged += OnUserInfoChanged;
    }

    //Unsubscribe from events
    private void OnDestroy()
    {
        PrivyAuthManager.OnAuthenticationStateChanged -= OnAuthenticationStateChanged;
        PrivyAuthManager.OnWalletAddressChanged -= OnWalletAddressChanged;
        PrivyAuthManager.OnUserInfoChanged -= OnUserInfoChanged;
    }

    private void OnConnectWalletClicked()
    {
        Debug.Log("Connect wallet button clicked");
        if (PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.ConnectWallet();
        }
    }

    private void OnLoginWithEmailClicked()
    {
        Debug.Log("login with email button clicked");
        if(PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.ShowEmailLoginPanel();
        }
    }
    private void OnLogoutClicked()
    {
        Debug.Log("Logout button clicked");
        if (PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.Logout();
        }
    }

    private void OnPlayGameClicked()
    {
        Debug.Log("Play game button clicked");
        //hide the welcome panel and start the game
        HideWelcomePanel();

        //trigger game start
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.StartGame();
        }
    }

    private void OnAuthenticationStateChanged(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            ShowWelcomePanel();
        }
        else
        {
            ShowAuthPanel();
        }
    }

    private void OnWalletAddressChanged(string address)
    {
        if (walletAddressText != null && !string.IsNullOrEmpty(address))
        {
            string shortAddress = address.Length > 12 ?
                $"{address.Substring(0, 6)}...{address.Substring(address.Length - 6)}" :
                address;
            walletAddressText.text = $"Wallet: {shortAddress}";
        }
    }
    private void OnUserInfoChanged(string userInfo)
    {
        if (userInfoText != null)
        {
            userInfoText.text = userInfo;
        }
    }

    public void ShowAuthPanel()
    {
        StartCoroutine(TransitionToPanel(authPanel, authCanvasGroup));
    }

    public void ShowWelcomePanel()
    {
        StartCoroutine(TransitionToPanel(welcomePanel, welcomeCanvasGroup));
    }
    public void ShowLoadingPanel(string message = "Connecting...")
    {
        if (loadingText != null)
        {
            loadingText.text = message;
        }
        StartCoroutine(TransitionToPanel(loadingPanel, loadingCanvasGroup));
    }

    public void HideWelcomePanel()
    {
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }
    }

    private IEnumerator TransitionToPanel(GameObject targetPanel, CanvasGroup targetCanvasGroup)
    {
        //hide all panels first
        if (authPanel != null) authPanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);
        if (loadingPanel != null) loadingPanel.SetActive(false);

        //show target panel
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);

            if (targetCanvasGroup != null)
            {
                targetCanvasGroup.alpha = 0f;

                // Fade in
                float elapsedTime = 0f;
                while (elapsedTime < panelTransitionDuration)
                {
                    elapsedTime += Time.deltaTime;
                    float progress = elapsedTime / panelTransitionDuration;
                    targetCanvasGroup.alpha = transitionCurve.Evaluate(progress);
                    yield return null;
                }

                targetCanvasGroup.alpha = 1f;
            }
        }
    }

    public void HideLoadingPanel()
    {
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }
}

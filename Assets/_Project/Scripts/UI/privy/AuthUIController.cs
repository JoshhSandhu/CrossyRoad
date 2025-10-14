using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI walletAddresText;
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
}

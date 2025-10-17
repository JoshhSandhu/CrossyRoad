using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Privy;
using System.Threading.Tasks;
public class AuthenticationFlowManager : MonoBehaviour
{
    public static AuthenticationFlowManager Instance { get; private set; }

    [Header("Privy Configuration")]
    [SerializeField] private string privyAppId = "cmgodao4u00c7l50caof1nhau"; // Replace with your actual App ID
    [SerializeField] private string privyClientId = "client-WY6RcMvDgyVeD25ssjFivmGBiWdhkddSMon2vEWHm4uTz"; // replace with your actual Client ID if needed
    [SerializeField] private PrivyLogLevel logLevel = PrivyLogLevel.INFO;
    [SerializeField] private bool isMobileApp = true; //set to true if building for mobile

    [Header("UI Panels")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private GameObject emailLoginPanel;
    [SerializeField] private GameObject otpVerificationPanel;
    [SerializeField] private GameObject welcomePanel;
    //[SerializeField] private GameObject loadingPanel;

    [Header("Auth Panel Elements")]
    [SerializeField] private Button connectWalletButton;
    [SerializeField] private Button loginEmailButton;
    [SerializeField] private TextMeshProUGUI authTitleText;
    [SerializeField] private TextMeshProUGUI authDescriptionText;

    [Header("Email Login Panel Elements")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private Button sendCodeButton;
    [SerializeField] private Button backToAuthButton;

    [Header("OTP Verification Panel Elements")]
    [SerializeField] private TMP_InputField otpCodeInputField;
    [SerializeField] private Button verifyCodeButton;
    [SerializeField] private Button backToEmailButton;

    [Header("Welcome Panel Elements")]
    [SerializeField] private TextMeshProUGUI welcomeTitleText;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button playGameButton;

    //[Header("Loading Panel Elements")]
    //[SerializeField] private TextMeshProUGUI loadingText;
    //[SerializeField] private Slider loadingProgressBar;

    public static event Action<bool> OnAuthenticationStateChanged;
    public static event Action<string> OnWalletAddressChanged;

    private IPrivy privyInstance;
    private bool isAuthenticated = false;
    private bool hasWallet = false;
    private string walletAddress = "";
    private string userEmail = "";

    public bool IsAuthenticated => isAuthenticated;
    public bool HasWallet => hasWallet;
    public string WalletAddress => walletAddress;

    private bool isGameReady = false;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private async void Start()
    {
        await InitializePrivy();
        SetupUI();
        StartAuthenticationFlow();
    }

    private async Task InitializePrivy()
    {
        try
        {
            Debug.Log($"Initializing Privy with App ID: {privyAppId}");
            Debug.Log($"Initializing Privy with Client ID: {privyClientId}");

            if (string.IsNullOrEmpty(privyAppId) || privyAppId == "your-app-id" || privyAppId.Length < 10)
            {
                Debug.LogError("Privy App ID is not set!");
                return;
            }

            if (string.IsNullOrEmpty(privyClientId) || privyClientId == "your-client-id" || privyClientId.Length < 10)
            {
                Debug.LogError("Privy Client ID is not set!");
                return;
            }

            //crating a privy config
            var config = new PrivyConfig
            {
                AppId = privyAppId,
                ClientId = privyClientId,
                LogLevel = logLevel
            };
            //init privy sdk
            privyInstance = PrivyManager.Initialize(config);

            //waiting for init
            await PrivyManager.AwaitReady();

            //setup auth state change callback
            privyInstance.SetAuthStateChangeCallback(OnAuthStateChanged);

            Debug.Log("Privy SDK initialized successfully");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Privy SDK: {ex.Message}");
        }
    }

    private void SetupUI()
    {
        //auth panel buttons
        if (connectWalletButton != null)
        {
            connectWalletButton.onClick.AddListener(ConnectWallet);
        }

        if (loginEmailButton != null)
        {
            loginEmailButton.onClick.AddListener(ShowEmailLoginPanel);
        }

        //Email Login UI
        if (sendCodeButton != null)
        {
            sendCodeButton.onClick.AddListener(SendOTPCode);
        }
        if (backToAuthButton != null)
        {
            backToAuthButton.onClick.AddListener(ShowAuthPanel);
        }

        //OTP verification UI
        if (verifyCodeButton != null)
        {
            verifyCodeButton.onClick.AddListener(VerifyOTPCode);
        }
        if (backToEmailButton != null)
        {
            backToEmailButton.onClick.AddListener(ShowEmailLoginPanel);
        }

        // Welcome Panel buttons
        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }
        if (playGameButton != null)
        {
            playGameButton.onClick.AddListener(StartGame);
        }

        UpdateUI();
    }

    //starting the auth flow
    private void StartAuthenticationFlow()
    {
        Debug.Log("Starting authentication flow...");

        if (privyInstance != null && privyInstance.IsReady)
        {
            if (privyInstance.AuthState == AuthState.Authenticated)
            {
                Debug.Log("User is already authenticated, checking wallet status");
                CheckWalletStatus();
            }
            else
            {
                Debug.Log("User not authenticated, showing auth panel");
                ShowAuthPanel();
            }
        }
        else
        {
            Debug.Log("Privy not ready, showing auth panel");
            ShowAuthPanel();
        }
    }

    //checking the wallet ststus
    private void CheckWalletStatus()
    {
        if (privyInstance?.User == null)
        {
            return;
        }

        var embeddedWallets = privyInstance.User.EmbeddedWallets;
        if (embeddedWallets != null && embeddedWallets.Length > 0)
        {
            walletAddress = embeddedWallets[0].Address;
            hasWallet = true;
            Debug.Log($"User has an embedded wallet: {walletAddress}");
            ShowWelcomePanel();
        }
        else
        {
            Debug.Log("User does not have an embedded wallet");
            hasWallet = false;
            ShowAuthPanel();
        }
    }

    //on auth state changed event
    private void OnAuthStateChanged(AuthState newState)
    {
        Debug.Log($"Authentication state changed to: {newState}");

        switch (newState)
        {
            case AuthState.Authenticated:
                isAuthenticated = true;
                CheckWalletStatus();
                break;
            case AuthState.Unauthenticated:
                isAuthenticated = false;
                hasWallet = false;
                walletAddress = "";
                ShowAuthPanel();
                break;
        }

        OnAuthenticationStateChanged?.Invoke(isAuthenticated);
    }

    //managing the ui panels
    private void ShowAuthPanel()
    {
        HideAllPanels();
        if (authPanel != null) authPanel.SetActive(true);
        Debug.Log("Showing Auth Panel");
    }

    private void ShowEmailLoginPanel()
    {
        HideAllPanels();
        if (emailLoginPanel != null) emailLoginPanel.SetActive(true);
        Debug.Log("Showing Email Login Panel");
    }

    private void ShowOTPVerificationPanel()
    {
        HideAllPanels();
        if (otpVerificationPanel != null) otpVerificationPanel.SetActive(true);
        Debug.Log("Showing OTP Verification Panel");
    }

    private void ShowWelcomePanel()
    {
        HideAllPanels();
        if (welcomePanel != null) welcomePanel.SetActive(true);
        UpdateWelcomePanel();
        isGameReady = false;
        Debug.Log("Showing Welcome Panel");
    }

    //private void ShowLoadingPanel()
    //{
    //    HideAllPanels();
    //    if (loadingPanel != null) loadingPanel.SetActive(true);
    //    Debug.Log("Showing Loading Panel");
    //}
    //method to hide all panels
    private void HideAllPanels()
    {
        if (authPanel != null) authPanel.SetActive(false);
        if (emailLoginPanel != null) emailLoginPanel.SetActive(false);
        if (otpVerificationPanel != null) otpVerificationPanel.SetActive(false);
        if (welcomePanel != null) welcomePanel.SetActive(false);
        //if (loadingPanel != null) loadingPanel.SetActive(false);
    }

    //authentication methods
    public async void ConnectWallet()
    {
        if (privyInstance == null) return;

        try
        {
            Debug.Log("Connecting wallet via OAuth...");

            // Configure redirect URI based on platform
            string redirectUri;
            if (isMobileApp)
            {
                // For mobile apps, use your app's URL scheme
                // Format: your-app-scheme://auth/callback
                redirectUri = "http://localhost:3000/auth/callback";
                Debug.Log("Using web redirect URI for mobile OAuth (Privy limitation)");
            }
            else
            {
                // For web builds
                redirectUri = "http://localhost:3000/auth/callback";
                Debug.Log("Using web redirect URI for OAuth");
            }

            var authState = await privyInstance.OAuth.LoginWithProvider(OAuthProvider.Google, redirectUri);

            if (authState == AuthState.Authenticated)
            {
                Debug.Log("OAuth login successful");
            }
            else
            {
                Debug.LogWarning("OAuth login failed or was cancelled");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Wallet connection failed: {e.Message}");
            
            Debug.LogError("Note: OAuth login may not work properly in Unity Editor. Try building and testing on device/web.");
        }
    }

    //sending the otp code
    public async void SendOTPCode()
    {
        if (privyInstance == null) return;

        userEmail = emailInputField != null ? emailInputField.text : "";

        if (string.IsNullOrEmpty(userEmail))
        {
            Debug.LogError("Please enter an email address");
            return;
        }

        try
        {
            Debug.Log($"Sending OTP code to {userEmail}");
            bool codeSent = await privyInstance.Email.SendCode(userEmail);

            if (codeSent)
            {
                Debug.Log("OTP code sent successfully");
                ShowOTPVerificationPanel();
            }
            else
            {
                Debug.LogError("Failed to send OTP code");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send OTP code: {e.Message}");
        }
    }

    //verifying the otp code
    public async void VerifyOTPCode()
    {
        if (privyInstance == null) return;

        string code = otpCodeInputField != null ? otpCodeInputField.text : "";

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogError("Please enter OTP code");
            return;
        }

        try
        {
            Debug.Log($"Verifying OTP code for {userEmail}");
            var authState = await privyInstance.Email.LoginWithCode(userEmail, code);

            if (authState == AuthState.Authenticated)
            {
                Debug.Log("Email login successful");
                // Check if user has wallet after authentication
                await CheckWalletAfterEmailLogin();
            }
            else
            {
                Debug.LogWarning("Email login failed - invalid code");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Email code verification failed: {e.Message}");
        }
    }

    //checking if the user has a wallet after email login
    private async Task CheckWalletAfterEmailLogin()
    {
        if (privyInstance?.User == null)
        {
            Debug.LogError("Privy instance or User is null - cannot check wallet");
            return;
        }

        Debug.Log($"Checking wallet for user: {privyInstance.User.Id}");
        Debug.Log($"Auth state: {privyInstance.AuthState}");
        Debug.Log($"Is ready: {privyInstance.IsReady}");

        var embeddedWallets = privyInstance.User.EmbeddedWallets;
        Debug.Log($"Found {embeddedWallets?.Length ?? 0} embedded wallets");

        if (embeddedWallets != null && embeddedWallets.Length > 0)
        {
            walletAddress = embeddedWallets[0].Address;
            hasWallet = true;
            Debug.Log($"User has wallet after email login: {walletAddress}");
            ShowWelcomePanel();
        }
        else
        {
            Debug.Log("User authenticated via email but no wallet - need to connect wallet");
            hasWallet = false;
            //if the user has no wallet, show auth panel to connect
            try
            {
                Debug.Log("Creating embedded wallet for authenticated user...");
                var createWalletTask = privyInstance.User.CreateWallet();
                var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30)); // 30 second timeout

                var completedTask = await Task.WhenAny(createWalletTask, timeoutTask);

                if (completedTask == timeoutTask)
                {
                    Debug.LogError("Wallet creation timed out after 30 seconds");
                    ShowAuthPanel();
                    return;
                }

                var newWallet = await createWalletTask;
                walletAddress = newWallet.Address;
                hasWallet = true;
                Debug.Log($"Successfully created embedded wallet: {walletAddress}");
                ShowWelcomePanel();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to create embedded wallet: {e.Message}");
                Debug.LogError($"Exception type: {e.GetType().Name}");
                Debug.LogError($"Stack trace: {e.StackTrace}");
                //If wallet creation fails, show auth panel to connect wallet manually
                ShowAuthPanel();
            }
        }
    }

    //the logout method
    public void Logout()
    {
        if (privyInstance == null) return;

        try
        {
            privyInstance.Logout();
            Debug.Log("User logged out");
        }
        catch (Exception e)
        {
            Debug.LogError($"Logout failed: {e.Message}");
        }
    }

    public bool IsGameReady()
    {
        return isGameReady;
    }
    //the start game method
    public void StartGame()
    {
        Debug.Log("Starting game...");
        isGameReady = true;
        //ShowLoadingPanel();
        HideAllPanels();
        //if (StartScreenManager.Instance != null)
        //{
        //    StartScreenManager.Instance.StartGame();
        //}
        // Start the actual game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogWarning("GameManager not found!");
        }
    }


    //updating the welcome panel with user info
    private void UpdateWelcomePanel()
    {
        if (walletAddressText != null && !string.IsNullOrEmpty(walletAddress))
        {
            string shortAddress = walletAddress.Length > 12 ?
                $"{walletAddress.Substring(0, 6)}...{walletAddress.Substring(walletAddress.Length - 6)}" :
                walletAddress;
            walletAddressText.text = $"Wallet: {shortAddress}";
        }

        if (userInfoText != null)
        {
            userInfoText.text = $"User ID: {privyInstance?.User?.Id ?? "Unknown"}";
        }
    }


    //updating the ui based on auth state
    private void UpdateUI()
    {
        if (authTitleText != null)
            authTitleText.text = "Welcome to Crossy Road!";

        if (authDescriptionText != null)
            authDescriptionText.text = "Connect your wallet to start playing and unlock exclusive skins!";

        if (welcomeTitleText != null)
            welcomeTitleText.text = "Ready to Play!";
    }
    private void OnDestroy()
    {
        // Cleanup
    }
}

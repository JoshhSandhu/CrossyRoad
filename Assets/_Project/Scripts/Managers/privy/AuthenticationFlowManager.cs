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
    [SerializeField] private PrivyConfiguration privyConfig;
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
    [SerializeField] private TextMeshProUGUI balanceText;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button playGameButton;
    [SerializeField] private Button addTokensButton;

    [Header("Token Panel")]
    [SerializeField] private TokenTransferPanel tokenTransferPanel;

    //[Header("Loading Panel Elements")]
    //[SerializeField] private TextMeshProUGUI loadingText;
    //[SerializeField] private Slider loadingProgressBar;

    public static event Action<bool> OnAuthenticationStateChanged;

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
            if (privyConfig == null)
            {
                Debug.LogError("PrivyConfig is not assigned!");
                return;
            }

            //Debug.Log($"Initializing Privy with App ID: {privyConfig.appId}");
            //Debug.Log($"Initializing Privy with Client ID: {privyConfig.clientId}");
            //Debug.Log($"Using Solana RPC URL: {privyConfig.solanaRpcUrl}");
            //Debug.Log($"Using Solana Network: {privyConfig.solanaNetwork}");
            //Debug.Log($"Solana Enabled: {privyConfig.enableSolana}");

            if (string.IsNullOrEmpty(privyConfig.appId) || privyConfig.appId == "your-app-id" || privyConfig.appId.Length < 10)
            {
                Debug.LogError("Privy App ID is not set!");
                return;
            }
            if (string.IsNullOrEmpty(privyConfig.clientId) || privyConfig.clientId == "your-client-id" || privyConfig.clientId.Length < 10)
            {
                Debug.LogError("Privy Client ID is not set!");
                return;
            }

            //creating a privy config
            var config = new Privy.PrivyConfig
            {
                AppId = privyConfig.appId,
                ClientId = privyConfig.clientId,
                LogLevel = logLevel
            };

            privyInstance = PrivyManager.Initialize(config);

            //waiting for init using new method
            var authState = await privyInstance.GetAuthState();

            //setup auth state change callback
            privyInstance.SetAuthStateChangeCallback(OnAuthStateChanged);

            //Debug.Log("Privy SDK initialized successfully");
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
            playGameButton.onClick.AddListener(OnPlayGameButtonClicked);
        }
        if (addTokensButton != null)
        {
            addTokensButton.onClick.AddListener(OpenTokenPanel);
        }

        UpdateUI();
    }

    //starting the auth flow
    private async void StartAuthenticationFlow()
    {
        //Debug.Log("Starting authentication flow...");

        if (privyInstance != null)
        {
            var authState = await privyInstance.GetAuthState();
            if (authState == AuthState.Authenticated)
            {
                //Debug.Log("User is already authenticated, checking wallet status");
                CheckWalletStatus();
            }
            else
            {
                //Debug.Log("User not authenticated, showing auth panel");
                ShowAuthPanel();
            }
        }
        else
        {
            //Debug.Log("Privy not ready, showing auth panel");
            ShowAuthPanel();
        }
    }

    //checking the wallet ststus
    private async void CheckWalletStatus()
    {
        var user = await privyInstance.GetUser();
        if (user == null)
        {
            return;
        }

        var embeddedWallets = user.EmbeddedWallets;
        if (embeddedWallets != null && embeddedWallets.Length > 0)
        {
            walletAddress = embeddedWallets[0].Address;
            hasWallet = true;
            //Debug.Log($"User has an embedded wallet: {walletAddress}");
            ShowWelcomePanel();
        }
        else
        {
            //Debug.Log("User does not have an embedded wallet");
            hasWallet = false;
            ShowAuthPanel();
        }
    }

    //on auth state changed event
    private void OnAuthStateChanged(AuthState newState)
    {
        //Debug.Log($"Authentication state changed to: {newState}");

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
        //Debug.Log("Showing Auth Panel");
    }

    private void ShowEmailLoginPanel()
    {
        HideAllPanels();
        if (emailLoginPanel != null) emailLoginPanel.SetActive(true);
        //Debug.Log("Showing Email Login Panel");
    }

    private void ShowOTPVerificationPanel()
    {
        HideAllPanels();
        if (otpVerificationPanel != null) otpVerificationPanel.SetActive(true);
        //Debug.Log("Showing OTP Verification Panel");
    }

    public async void ShowWelcomePanel()
    {
        HideAllPanels();
        if (welcomePanel != null) welcomePanel.SetActive(true);
        await UpdateWelcomePanel();
        isGameReady = false;
        //Debug.Log("Showing Welcome Panel");
    }

    //private void ShowLoadingPanel()
    //{
    //    HideAllPanels();
    //    if (loadingPanel != null) loadingPanel.SetActive(true);
    //    //Debug.Log("Showing Loading Panel");
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
            //Debug.Log("Connecting wallet via OAuth...");

            // Configure redirect URI based on platform
            string redirectUri;
            if (isMobileApp)
            {
                // For mobile apps, use your app's URL scheme
                // Format: your-app-scheme://auth/callback
                redirectUri = "http://localhost:3000/auth/callback";
                //Debug.Log("Using web redirect URI for mobile OAuth (Privy limitation)");
            }
            else
            {
                // For web builds
                redirectUri = "http://localhost:3000/auth/callback";
                //Debug.Log("Using web redirect URI for OAuth");
            }

            var authState = await privyInstance.OAuth.LoginWithProvider(OAuthProvider.Google, redirectUri);

            if (authState == AuthState.Authenticated)
            {
                //Debug.Log("OAuth login successful");
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
            //Debug.Log($"Sending OTP code to {userEmail}");
            bool codeSent = await privyInstance.Email.SendCode(userEmail);

            if (codeSent)
            {
                //Debug.Log("OTP code sent successfully");
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
            //Debug.Log($"Verifying OTP code for {userEmail}");
            var authState = await privyInstance.Email.LoginWithCode(userEmail, code);

            if (authState == AuthState.Authenticated)
            {
                //Debug.Log("Email login successful");
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
        var user = await privyInstance.GetUser();
        if (user == null)
        {
            Debug.LogError("Privy instance or User is null - cannot check wallet");
            return;
        }

        var authState = await privyInstance.GetAuthState();
        //Debug.Log($"Checking wallet for user: {user.Id}");
        //Debug.Log($"Auth state: {authState}");

        var embeddedWallets = user.EmbeddedWallets;
        //Debug.Log($"Found {embeddedWallets?.Length ?? 0} embedded wallets");

        if (embeddedWallets != null && embeddedWallets.Length > 0)
        {
            walletAddress = embeddedWallets[0].Address;
            hasWallet = true;
            //Debug.Log($"User has wallet after email login: {walletAddress}");
            ShowWelcomePanel();
        }
        else
        {
            //Debug.Log("User authenticated via email but no wallet - need to connect wallet");
            hasWallet = false;
            //if the user has no wallet, show auth panel to connect
            try
            {
                //Debug.Log("Creating embedded wallet for authenticated user...");
                var createWalletTask = user.CreateWallet();
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
                //Debug.Log($"Successfully created embedded wallet: {walletAddress}");
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
            //Debug.Log("User logged out");
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
        //Debug.Log("Starting game...");
        isGameReady = true;

        if (CustomPrivyWalletAdapter.Instance != null)
        {
            CustomPrivyWalletAdapter.Instance.ApproveTransactionsViaPlayGame();
        }
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
    private async System.Threading.Tasks.Task UpdateWelcomePanel()
    {
        if (walletAddressText != null)
        {
            string solanaAddress = await GetSolanaWalletAddress();
            if (!string.IsNullOrEmpty(solanaAddress) && solanaAddress != "No Wallet")
            {
                string shortAddress = solanaAddress.Length > 12 ?
                    $"{solanaAddress.Substring(0, 6)}...{solanaAddress.Substring(solanaAddress.Length - 6)}" :
                    solanaAddress;
                walletAddressText.text = $"Solana Wallet: {shortAddress}";
            }
            else
            {
                walletAddressText.text = "Solana Wallet: Wallet Not Created";
            }
        }

        if (userInfoText != null)
        {
            var user = await privyInstance.GetUser();
            userInfoText.text = $"User ID: {user?.Id ?? "Unknown"}";
        }

        // Check balance and update UI
        await CheckBalanceAndUpdateUI();
    }

    /// <summary>
    /// Check Privy wallet balance and update UI accordingly
    /// </summary>
    private async System.Threading.Tasks.Task CheckBalanceAndUpdateUI()
    {
        // Retry mechanism: wait for adapter to be ready (max 10 seconds)
        int maxRetries = 20;
        int retryCount = 0;

        while ((CustomPrivyWalletAdapter.Instance == null || !CustomPrivyWalletAdapter.Instance.IsReady()) && retryCount < maxRetries)
        {
            retryCount++;
            await System.Threading.Tasks.Task.Delay(500); // Wait 500ms between retries
        }

        if (CustomPrivyWalletAdapter.Instance == null || !CustomPrivyWalletAdapter.Instance.IsReady())
        {
            // Wallet not ready after retries, disable play game
            if (playGameButton != null)
            {
                playGameButton.interactable = false;
            }
            if (balanceText != null)
            {
                balanceText.text = "Balance: Wallet not ready";
            }
            Debug.LogWarning("CustomPrivyWalletAdapter not ready after waiting");
            return;
        }

        try
        {
            var balance = await CustomPrivyWalletAdapter.Instance.GetPrivyBalance();
            double solBalance = balance / 1_000_000_000.0; // Convert lamports to SOL

            // Update balance text
            if (balanceText != null)
            {
                balanceText.text = $"Balance: {solBalance:F6} SOL";
            }

            // Transaction cost per move (5000 lamports = 0.000005 SOL)
            const ulong transactionCostPerMove = 5000;
            const ulong lowBalanceThreshold = transactionCostPerMove * 3; // 3 moves worth
            const ulong minBalanceThreshold = transactionCostPerMove * 4; // 4 moves worth

            // Check if balance is sufficient
            bool hasEnoughBalance = balance >= minBalanceThreshold;

            // Update Play Game button
            if (playGameButton != null)
            {
                playGameButton.interactable = hasEnoughBalance;
            }

            // Update Add Tokens button
            if (addTokensButton != null)
            {
                addTokensButton.interactable = true; // Always allow adding tokens
            }

            // Show low balance warning if balance is between 3x and 4x cost
            if (balance >= lowBalanceThreshold && balance < minBalanceThreshold)
            {
                if (TransactionToastManager.Instance != null)
                {
                    TransactionToastManager.Instance.ShowToastBottom(
                        "Low balance warning: Please add SOL to continue playing",
                        Color.yellow
                    );
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error checking balance: {ex.Message}");
            if (balanceText != null)
            {
                balanceText.text = "Balance: Error";
            }
            if (playGameButton != null)
            {
                playGameButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// Handle Play Game button click (with balance check)
    /// </summary>
    private void OnPlayGameButtonClicked()
    {
        if (playGameButton != null && !playGameButton.interactable)
        {
            // Button is disabled, show toast message
            if (TransactionToastManager.Instance != null)
            {
                TransactionToastManager.Instance.ShowToastBottom(
                    "Please add SOL to your wallet",
                    Color.red
                );
            }
            return;
        }

        // Button is enabled, start game
        StartGame();
    }

    /// <summary>
    /// Open token transfer panel
    /// </summary>
    public void OpenTokenPanel()
    {
        HideAllPanels();
        OpenTokenPanelWithSource(2);
    }

    public void OpenTokenPanelWithSource(int source)
    {
        if (tokenTransferPanel != null)
        {
            tokenTransferPanel.OpenPanel(source);
        }
        else
        {
            Debug.LogWarning("TokenTransferPanel not assigned!");
        }
    }


    //updating the ui based on auth state
    private void UpdateUI()
    {
        if (authTitleText != null)
            authTitleText.text = "Crossy Road";

        if (authDescriptionText != null)
            authDescriptionText.text = "Connect your wallet to start playing and unlock exclusive skins!";

        if (welcomeTitleText != null)
            welcomeTitleText.text = "Ready to Play!";
    }

    //this creates a solana wallet for the user if not already created
    public async Task<IEmbeddedSolanaWallet> CreateSolanaWallet()
    {
        var user = await privyInstance.GetUser();
        if (user == null)
        {
            Debug.LogError("User not authenticated");
            return null;
        }
        try
        {
            var solanaWallet = await user.CreateSolanaWallet();
            //Debug.Log($"Solana wallet created: {solanaWallet.Address}");
            return solanaWallet;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create Solana wallet: {e.Message}");
            return null;
        }
    }

    //gets all the solana wallet for the authenticated user
    public async Task<IEmbeddedSolanaWallet[]> GetSolanaWallets()
    {
        //Debug.Log("GetSolanaWallets called");
        var user = await privyInstance.GetUser();
        if (user == null)
        {
            Debug.LogError("User is null in GetSolanaWallets");
            return new IEmbeddedSolanaWallet[0];
        }

        //Debug.Log($"User ID: {user.Id}");
        //Debug.Log($"EmbeddedSolanaWallets count: {user.EmbeddedSolanaWallets?.Length ?? 0}");
        return user.EmbeddedSolanaWallets;
    }

    //this ensures the user has a solana wallet and if not then creates one
    public async Task<bool> EnsureSolanaWallet()
    {
        var solanaWallets = await GetSolanaWallets();
        if (solanaWallets.Length == 0)
        {
            //Debug.Log("No Solana wallet found, creating one...");
            var newWallet = await CreateSolanaWallet();
            return newWallet != null;
        }
        //Debug.Log($"Found {solanaWallets.Length} Solana wallet(s)");
        return true;
    }

    //gets the primary solana wallet address
    public async Task<string> GetSolanaWalletAddress()
    {
        var wallets = await GetSolanaWallets();
        return wallets.Length > 0 ? wallets[0].Address : "No Wallet";
    }

    /// <summary>
    /// Public method to refresh balance (called when adapter becomes ready)
    /// </summary>
    public async System.Threading.Tasks.Task RefreshWelcomePanelBalance()
    {
        if (welcomePanel != null && welcomePanel.activeSelf)
        {
            await CheckBalanceAndUpdateUI();
        }
    }

    private void OnDestroy()
    {
        // Cleanup
    }
}


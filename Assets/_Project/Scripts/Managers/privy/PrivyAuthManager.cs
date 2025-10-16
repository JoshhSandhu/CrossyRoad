using UnityEngine;
using System;
using Privy;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

public class PrivyAuthManager : MonoBehaviour
{
    public static PrivyAuthManager Instance { get; private set; }

    [Header("Privy Configuration")]
    [SerializeField] private string privyAppId = "cmgodao4u00c7l50caof1nhau"; // Replace with your actual App ID
    [SerializeField] private string privyClientId = "client-WY6RcMvDgyVeD25ssjFivmGBiWdhkddSMon2vEWHm4uTz"; // replace with your actual Client ID if needed
    [SerializeField] private PrivyLogLevel logLevel = PrivyLogLevel.INFO;

    [Header("UI References")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private Button connectWalletButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TextMeshProUGUI walletStatusText;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Email Login UI")]
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField otpCodeInputField;
    [SerializeField] private Button sendCodeButton;
    [SerializeField] private Button verifyCodeButton;
    [SerializeField] private GameObject emailLoginPanel;
    [SerializeField] private GameObject otpVerificationPanel;

    public static event Action<bool> OnAuthenticationStateChanged;
    public static event Action<string> OnWalletAddressChanged;
    public static event Action<string> OnUserInfoChanged;

    private IPrivy privyInstance;
    private bool isAuthenticated = false;
    private string walletAddress = "";
    private string userInfo = "";

    public bool IsAuthenticated => isAuthenticated;
    public string WalletAddress => walletAddress;
    public string UserInfo => userInfo;

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
        CheckExistingSession();
    }

    private async Task InitializePrivy()
    {
        try
        {
            Debug.Log($"Initializing Privy with App ID: {privyAppId}");
            Debug.Log($"Initializing Privy with Client ID: {privyClientId}");

            if (string.IsNullOrEmpty(privyAppId) || privyAppId == "your-app-id")
            {
                Debug.LogError("Privy App ID is not set! Please set your actual App ID in the Inspector.");
                return;
            }
            if (string.IsNullOrEmpty(privyClientId) || privyClientId == "your-client-id")
            {
                Debug.LogError("Privy Client ID is not set! Please set your actual Client ID in the Inspector.");
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
            Debug.LogError($"Stack trace: {ex.StackTrace}");
        }
    }

    private void SetupUI()
    {
        if (connectWalletButton != null)
        {
            connectWalletButton.onClick.AddListener(ConnectWallet);
        }

        if (loginButton != null)
        {
            loginButton.onClick.AddListener(ShowEmailLoginPanel);
        }

        if (logoutButton != null)
        {
            logoutButton.onClick.AddListener(Logout);
        }

        //Email Login UI
        if (sendCodeButton != null)
        {
            sendCodeButton.onClick.AddListener(SendOTPCode);
        }

        if (verifyCodeButton != null)
        {
            verifyCodeButton.onClick.AddListener(VerifyOTPCode);
        }

        UpdateUI();
    }

    private void CheckExistingSession()
    {
        if (privyInstance != null && privyInstance.IsReady)
        {
            if (privyInstance.AuthState == AuthState.Authenticated)
            {
                OnUserAuthenticated();
            }
        }
    }

    public async void ConnectWallet()
    {
        if (privyInstance == null)
        {
            Debug.LogError("Privy SDK not initialized");
            return;
        }

        SetLoading(true);

        try
        {
            //using OAuth to connect wallet
            var authState = await privyInstance.OAuth.LoginWithProvider(OAuthProvider.Google, "");

            if (authState == AuthState.Authenticated)
            {
                Debug.Log("OAuth login successful");
            }
            else
            {
                Debug.LogWarning("OAuth login failed or was cancelled");
                SetLoading(false);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Wallet connection failed: {e.Message}");
            SetLoading(false);
        }
    }

    public void ShowEmailLoginPanel()
    {
        if (emailLoginPanel != null)
        {
            emailLoginPanel.SetActive(true);
        }
        if (otpVerificationPanel != null)
        {
            otpVerificationPanel.SetActive(false);
        }
    }

    public async void SendOTPCode()
    {
        if (privyInstance == null)
        {
            Debug.LogError("Privy SDK not initialized - cannot send OTP code");
            return;
        }

        string email = emailInputField != null ? emailInputField.text : "demo@example.com";

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Please enter an email address");
            return;
        }
        Debug.Log($"Attempting to send OTP code to: {email}");
        Debug.Log($"Privy instance ready: {privyInstance.IsReady}");
        Debug.Log($"Privy auth state: {privyInstance.AuthState}");
        SetLoading(true);

        try
        {
            Debug.Log($"Sending OTP code to {email}");
            bool codeSent = await privyInstance.Email.SendCode(email);

            if (codeSent)
            {
                Debug.Log("OTP code sent successfully. Please check your email.");
                //enalble OTP verification panel
                if (otpVerificationPanel != null)
                {
                    otpVerificationPanel.SetActive(true);
                }
                if (emailLoginPanel != null)
                {
                    emailLoginPanel.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("Failed to send OTP code");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to send OTP code: {e.Message}");
            Debug.LogError($"Exception type: {e.GetType().Name}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
        finally
        {
            SetLoading(false);
        }
    }

    public async void VerifyOTPCode()
    {
        if (privyInstance == null)
        {
            Debug.LogError("Privy SDK not initialized");
            return;
        }

        string email = emailInputField != null ? emailInputField.text : "demo@example.com";
        string code = otpCodeInputField != null ? otpCodeInputField.text : "123456";

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(code))
        {
            Debug.LogError("Please enter both email and OTP code");
            return;
        }

        SetLoading(true);

        try
        {
            Debug.Log($"Verifying OTP code for {email}");
            var authState = await privyInstance.Email.LoginWithCode(email, code);

            if (authState == AuthState.Authenticated)
            {
                Debug.Log("Email login successful");
                if (emailLoginPanel != null)
                {
                    emailLoginPanel.SetActive(false);
                }
                if (otpVerificationPanel != null)
                {
                    otpVerificationPanel.SetActive(false);
                }
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
        finally
        {
            SetLoading(false);
        }
    }

    public void Logout()
    {
        if (privyInstance == null)
        {
            Debug.LogError("Privy SDK not initialized");
            return;
        }

        try
        {
            privyInstance.Logout();
        }
        catch (Exception e)
        {
            Debug.LogError($"Logout failed: {e.Message}");
        }
    }

    private void OnAuthStateChanged(AuthState newState)
    {
        Debug.Log($"Authentication state changed to: {newState}");

        switch (newState)
        {
            case AuthState.Authenticated:
                OnUserAuthenticated();
                break;
            case AuthState.Unauthenticated:
                OnUserLoggedOut();
                break;
            case AuthState.NotReady:
                break;
        }
    }

    private async void OnUserAuthenticated()
    {
        if (privyInstance?.User == null)
        {
            Debug.LogWarning("User is null despite being authenticated");
            return;
        }

        isAuthenticated = true;

        //geting the first embedded wallet address
        var embeddedWallets = privyInstance.User.EmbeddedWallets;
        if (embeddedWallets != null && embeddedWallets.Length > 0)
        {
            walletAddress = embeddedWallets[0].Address;
            Debug.Log($"User authenticated with wallet: {walletAddress}");
        }
        else
        {
            Debug.LogWarning("No embedded wallet found for authenticated user, creating one automatically");
            await CreateEmbeddedWalletForUser();
        }

        //getting user info
        userInfo = $"User ID: {privyInstance.User.Id}";

        //getting linked accounts info
        var linkedAccounts = privyInstance.User.LinkedAccounts;
        if (linkedAccounts != null && linkedAccounts.Length > 0)
        {
            foreach (var account in linkedAccounts)
            {
                if (account is PrivyEmbeddedWalletAccount walletAccount)
                {
                    userInfo += $"\nWallet Address: {walletAccount.Address}";
                }
            }
        }

        SetLoading(false);
        UpdateUI();

        OnAuthenticationStateChanged?.Invoke(true);
        OnWalletAddressChanged?.Invoke(walletAddress);
        OnUserInfoChanged?.Invoke(userInfo);

        if (authPanel != null)
        {
            authPanel.SetActive(false);
        }
    }

    private void OnUserLoggedOut()
    {
        isAuthenticated = false;
        walletAddress = "";
        userInfo = "";

        UpdateUI();

        OnAuthenticationStateChanged?.Invoke(false);
        OnWalletAddressChanged?.Invoke("");
        OnUserInfoChanged?.Invoke("");

        if (authPanel != null)
        {
            authPanel.SetActive(true);
        }
    }

    private void UpdateUI()
    {
        if (walletStatusText != null)
        {
            walletStatusText.text = isAuthenticated ? "Connected" : "Not Connected";
            walletStatusText.color = isAuthenticated ? Color.green : Color.red;
        }

        if (walletAddressText != null)
        {
            if (isAuthenticated && !string.IsNullOrEmpty(walletAddress))
            {
                string shortAddress = walletAddress.Length > 12 ?
                    $"{walletAddress.Substring(0, 6)}...{walletAddress.Substring(walletAddress.Length - 6)}" :
                    walletAddress;
                walletAddressText.text = $"Wallet: {shortAddress}";
            }
            else
            {
                walletAddressText.text = "No wallet connected";
            }
        }

        if (userInfoText != null)
        {
            userInfoText.text = isAuthenticated ? userInfo : "Not logged in";
        }

        if (connectWalletButton != null)
        {
            connectWalletButton.gameObject.SetActive(!isAuthenticated);
        }

        if (loginButton != null)
        {
            loginButton.gameObject.SetActive(!isAuthenticated);
        }

        if (logoutButton != null)
        {
            logoutButton.gameObject.SetActive(isAuthenticated);
        }
    }

    private void SetLoading(bool loading)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(loading);
        }

        if (connectWalletButton != null)
        {
            connectWalletButton.interactable = !loading;
        }

        if (loginButton != null)
        {
            loginButton.interactable = !loading;
        }
    }

    public void ShowAuthPanel()
    {
        if (authPanel != null)
        {
            authPanel.SetActive(true);
        }
    }

    public void HideAuthPanel()
    {
        if (authPanel != null)
        {
            authPanel.SetActive(false);
        }
    }

    //getting solana wallet address for transactions
    public async Task<string> GetSolanaWalletAddress()
    {
        if (!isAuthenticated || privyInstance?.User == null)
        {
            return null;
        }

        try
        {
            var embeddedWallets = privyInstance.User.EmbeddedWallets;
            if (embeddedWallets != null && embeddedWallets.Length > 0)
            {
                return embeddedWallets[0].Address;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to get wallet address: {e.Message}");
        }

        return null;
    }

    //creating a new embedded wallet for the user
    private async Task CreateEmbeddedWalletForUser()
    {
        try
        {
            Debug.Log("Creating embedded wallet for authenticated user...");
            var newWallet = await privyInstance.User.CreateWallet();
            walletAddress = newWallet.Address;
            Debug.Log($"Successfully created embedded wallet: {walletAddress}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create embedded wallet: {e.Message}");
            // Continue without wallet - user can still use the app
        }
    }
    private void OnDestroy()
    {
        //cleanup if needed
    }
}
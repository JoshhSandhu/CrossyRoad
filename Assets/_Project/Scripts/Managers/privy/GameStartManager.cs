using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject gameStartPanel;

    [Header("Settings")]
    [SerializeField] private bool requireAuthentication = true;
    [SerializeField] private float authCheckDelay = 1f;

    private void Start()
    {
        if (requireAuthentication)
        {
            Invoke(nameof(CheckAuthenticationStatus), authCheckDelay);
        }
        else
        {
            ShowMainMenu();
        }
    }

    private void CheckAuthenticationStatus()
    {
        Debug.Log("GameStartManager: Checking authentication status...");

        if (PrivyAuthManager.Instance != null)
        {
            Debug.Log($"GameStartManager: PrivyAuthManager found. IsAuthenticated: {PrivyAuthManager.Instance.IsAuthenticated}");

            if (PrivyAuthManager.Instance.IsAuthenticated)
            {
                Debug.Log("GameStartManager: User is authenticated, showing main menu");
                ShowMainMenu();
            }
            else
            {
                Debug.Log("GameStartManager: User not authenticated, showing auth flow");
                ShowAuthenticationFlow();
            }
        }
        else
        {
            Debug.LogError("GameStartManager: PrivyAuthManager instance not found!");
            ShowMainMenu();
        }
    }

    private void ShowAuthenticationFlow()
    {
        Debug.Log("GameStartManager: Showing authentication flow");
        //hide the main menu panel
        if (mainMenuPanel != null)
        {
            Debug.Log("GameStartManager: Hiding main menu panel");
            mainMenuPanel.SetActive(false);
        }

        //show the auth panel
        if(PrivyAuthManager.Instance != null)
        {
            Debug.Log("GameStartManager: Showing auth panel");
            PrivyAuthManager.Instance.ShowAuthPanel();
        }

        //sub to the authentation events
        PrivyAuthManager.OnAuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void ShowMainMenu()
    {
        Debug.Log("GameStartManager: Showing main menu");

        //hide auth panel
        if (PrivyAuthManager.Instance != null)
        {
            Debug.Log("GameStartManager: Hiding auth panel");
            PrivyAuthManager.Instance.HideAuthPanel();
        }

        //show main menu
        if (mainMenuPanel != null)
        {
            Debug.Log("GameStartManager: Activating main menu panel");
            mainMenuPanel.SetActive(true);
        }
    }

    private void OnAuthenticationStateChanged(bool isAuthenticated)
    {
        if (isAuthenticated)
        {
            Debug.Log("User authenticated successfully, showing main menu");
            ShowMainMenu();
        }
    }
    public void StartGame()
    {
        if (requireAuthentication && PrivyAuthManager.Instance != null && !PrivyAuthManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("Cannot start game without authentication");
            ShowAuthenticationFlow();
            return;
        }

        Debug.Log("Starting game...");

        //loading the gameplay scene
        SceneManager.LoadScene("Gameplay");
    }

    public void OpenShop()
    {
        if (requireAuthentication && PrivyAuthManager.Instance != null && !PrivyAuthManager.Instance.IsAuthenticated)
        {
            Debug.LogWarning("Cannot open shop without authentication");
            ShowAuthenticationFlow();
            return;
        }

        Debug.Log("Opening shop...");

        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.OpenShop();
        }
    }

    public void Logout()
    {
        if (PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.Logout();
        }
    }

    private void OnDestroy()
    {
        PrivyAuthManager.OnAuthenticationStateChanged -= OnAuthenticationStateChanged;
    }
}

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
        if(PrivyAuthManager.Instance != null)
        {
            if (PrivyAuthManager.Instance.IsAuthenticated)
            {
               ShowMainMenu();
            }
            else
            {
                ShowAuthenticationFlow();
            }
        }
        else
        {
            Debug.LogError("PrivyAuthManager instance not found!");
            ShowMainMenu();
        }
    }

    private void ShowAuthenticationFlow()
    {
        //hide the main menu panel
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }

        //show the auth panel
        if(PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.ShowAuthPanel();
        }

        //sub to the authentation events
        PrivyAuthManager.OnAuthenticationStateChanged += OnAuthenticationStateChanged;
    }

    private void ShowMainMenu()
    {
        //hide auth panel
        if (PrivyAuthManager.Instance != null)
        {
            PrivyAuthManager.Instance.HideAuthPanel();
        }

        //show main menu
        if (mainMenuPanel != null)
        {
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

using UnityEngine;
using System;
using Privy;
using TMPro;
using UnityEngine.UI
public class PrivyAuthManager : MonoBehaviour
{
    public static PrivyAuthManager Instance { get; private set; }

    //update app id with the app id from your privy dashboard
    [Header("Privy Configuration")]
    [SerializeField] private string privyAppId = "your-app-id";
    [SerializeField] private bool useDevnet = true; //make false for main net

    [Header("UI References")]
    [SerializeField] private GameObject authPanel;
    [SerializeField] private Button connectWalletButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private TextMeshProUGUI walletStatusText;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private TextMeshProUGUI userInfoText;
    [SerializeField] private GameObject loadingIndicator;

    //[Header("Events")]
    public static event Action<bool> OnAuthenticationStateChanged;
    public static event Action<string> OnWalletAddressChanged;
    public static event Action<string> OnUserInfoChanged;

    private PrivyClient privyClient;
    private bool isAuthenticated = false;
    private string walletAddress = "";
    private string userInfo = "";



}

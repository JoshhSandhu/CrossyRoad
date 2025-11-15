using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the token transfer panel UI for transferring SOL from Seeker wallet to Privy wallet
/// </summary>
public class TokenTransferPanel : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject tokenPanel;

    [Header("Wallet Address Displays")]
    [SerializeField] private TextMeshProUGUI seekerAddressText;
    [SerializeField] private TextMeshProUGUI privyAddressText;
    [SerializeField] private Button copySeekerAddressButton; // Add this button in Unity Inspector
    [SerializeField] private Button copyPrivyAddressButton; // Add this button in Unity Inspector

    [Header("Balance Displays")]
    [SerializeField] private TextMeshProUGUI seekerBalanceText;
    [SerializeField] private TextMeshProUGUI privyBalanceText;

    [Header("Transfer Input")]
    [SerializeField] private TMP_InputField transferAmountInput;
    [SerializeField] private Button transferButton;
    [SerializeField] private TextMeshProUGUI transferStatusText;

    [Header("Buttons")]
    [SerializeField] private Button backButton;
    [SerializeField] private Button refreshButton;

    [Header("Loading Indicator")]
    [SerializeField] private GameObject loadingIndicator;

    private bool isRefreshing = false;
    private bool isTransferring = false;

    private void Awake()
    {
        // Setup button listeners
        if (transferButton != null)
        {
            transferButton.onClick.AddListener(OnTransferButtonClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(OnRefreshButtonClicked);
        }

        // Add copy button listeners
        if (copySeekerAddressButton != null)
        {
            copySeekerAddressButton.onClick.AddListener(OnCopySeekerAddressClicked);
        }

        if (copyPrivyAddressButton != null)
        {
            copyPrivyAddressButton.onClick.AddListener(OnCopyPrivyAddressClicked);
        }

        // Hide panel initially
        if (tokenPanel != null)
        {
            tokenPanel.SetActive(false);
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Open the token transfer panel
    /// </summary>
    public async void OpenPanel()
    {
        if (tokenPanel != null)
        {
            tokenPanel.SetActive(true);
            await RefreshBalances();
        }
    }

    /// <summary>
    /// Close the token transfer panel
    /// </summary>
    public void ClosePanel()
    {
        if (tokenPanel != null)
        {
            tokenPanel.SetActive(false);
        }
        ClearStatus();
    }

    /// <summary>
    /// Refresh wallet balances and addresses
    /// </summary>
    public async Task RefreshBalances()
    {
        if (isRefreshing) return;

        isRefreshing = true;
        SetLoading(true);

        try
        {
            // Connect to Seeker wallet if not connected
            if (SeekerWalletManager.Instance != null && !SeekerWalletManager.Instance.IsConnected)
            {
                await SeekerWalletManager.Instance.ConnectToSeekerWallet();
            }

            // Update Seeker wallet info
            if (SeekerWalletManager.Instance != null && SeekerWalletManager.Instance.IsConnected)
            {
                var seekerAddress = SeekerWalletManager.Instance.GetSeekerAddress();
                var seekerBalance = await SeekerWalletManager.Instance.GetSeekerBalance();

                if (seekerAddressText != null)
                {
                    // Show shortened address in UI, copy button has full address
                    string shortAddress = seekerAddress != null && seekerAddress.Length > 12
                        ? $"{seekerAddress.Substring(0, 6)}...{seekerAddress.Substring(seekerAddress.Length - 6)}"
                        : seekerAddress ?? "Not Connected";
                    seekerAddressText.text = $"Seeker: {shortAddress}";
                }

                if (seekerBalanceText != null)
                {
                    double solBalance = seekerBalance / 1_000_000_000.0; // Convert lamports to SOL
                    seekerBalanceText.text = $"Balance: {solBalance:F6} SOL";
                }
            }
            else
            {
                if (seekerAddressText != null)
                {
                    seekerAddressText.text = "Seeker: Not Connected";
                }
                if (seekerBalanceText != null)
                {
                    seekerBalanceText.text = "Balance: 0 SOL";
                }
            }

            // Update Privy wallet info
            if (CustomPrivyWalletAdapter.Instance != null && CustomPrivyWalletAdapter.Instance.IsReady())
            {
                var privyAddress = CustomPrivyWalletAdapter.Instance.GetWalletAddress();
                var privyBalance = await CustomPrivyWalletAdapter.Instance.GetPrivyBalance();

                if (privyAddressText != null)
                {
                    // Show shortened address in UI, copy button has full address
                    string shortAddress = privyAddress != null && privyAddress.Length > 12
                        ? $"{privyAddress.Substring(0, 6)}...{privyAddress.Substring(privyAddress.Length - 6)}"
                        : privyAddress ?? "Not Available";
                    privyAddressText.text = $"Privy: {shortAddress}";
                }

                if (privyBalanceText != null)
                {
                    double solBalance = privyBalance / 1_000_000_000.0; // Convert lamports to SOL
                    privyBalanceText.text = $"Balance: {solBalance:F6} SOL";
                }
            }
            else
            {
                if (privyAddressText != null)
                {
                    privyAddressText.text = "Privy: Not Available";
                }
                if (privyBalanceText != null)
                {
                    privyBalanceText.text = "Balance: 0 SOL";
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error refreshing balances: {ex.Message}");
            SetStatus("Error refreshing balances", Color.red);
        }
        finally
        {
            isRefreshing = false;
            SetLoading(false);
        }
    }

    /// <summary>
    /// Handle transfer button click
    /// </summary>
    private async void OnTransferButtonClicked()
    {
        if (isTransferring) return;

        if (SeekerWalletManager.Instance == null || !SeekerWalletManager.Instance.IsConnected)
        {
            SetStatus("Please connect to Seeker wallet first", Color.red);
            return;
        }

        if (CustomPrivyWalletAdapter.Instance == null || !CustomPrivyWalletAdapter.Instance.IsReady())
        {
            SetStatus("Privy wallet not available", Color.red);
            return;
        }

        // Parse transfer amount
        if (transferAmountInput == null || string.IsNullOrEmpty(transferAmountInput.text))
        {
            SetStatus("Please enter an amount", Color.red);
            return;
        }

        if (!double.TryParse(transferAmountInput.text, out double solAmount) || solAmount <= 0)
        {
            SetStatus("Invalid amount. Please enter a positive number", Color.red);
            return;
        }

        // Convert SOL to lamports
        ulong amountLamports = (ulong)(solAmount * 1_000_000_000);

        // Check Seeker balance
        var seekerBalance = await SeekerWalletManager.Instance.GetSeekerBalance();
        if (seekerBalance < amountLamports)
        {
            SetStatus("Insufficient balance in Seeker wallet", Color.red);
            return;
        }

        isTransferring = true;
        SetLoading(true);
        SetStatus("Transferring...", Color.yellow);

        if (transferButton != null)
        {
            transferButton.interactable = false;
        }

        try
        {
            var privyAddress = CustomPrivyWalletAdapter.Instance.GetWalletAddress();
            var signature = await SeekerWalletManager.Instance.TransferToPrivyWallet(privyAddress, amountLamports);

            if (!string.IsNullOrEmpty(signature))
            {
                SetStatus("Transfer successful!", Color.green);

                // Show success toast
                if (TransactionToastManager.Instance != null)
                {
                    string shortSig = signature.Length > 16
                        ? $"{signature.Substring(0, 8)}...{signature.Substring(signature.Length - 8)}"
                        : signature;
                    TransactionToastManager.Instance.ShowToastTop($"Transfer: {shortSig}", Color.green);
                }

                // Refresh balances
                await RefreshBalances();

                // Clear input
                if (transferAmountInput != null)
                {
                    transferAmountInput.text = "";
                }

                // Close panel after short delay
                await Task.Delay(1500);
                ClosePanel();
            }
            else
            {
                SetStatus("Transfer failed. Please try again", Color.red);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Transfer error: {ex.Message}");
            SetStatus($"Transfer failed: {ex.Message}", Color.red);
        }
        finally
        {
            isTransferring = false;
            SetLoading(false);
            if (transferButton != null)
            {
                transferButton.interactable = true;
            }
        }
    }

    /// <summary>
    /// Handle back button click
    /// </summary>
    private void OnBackButtonClicked()
    {
        ClosePanel();
    }

    /// <summary>
    /// Handle refresh button click
    /// </summary>
    private async void OnRefreshButtonClicked()
    {
        await RefreshBalances();
    }

    /// <summary>
    /// Copy Seeker wallet address to clipboard
    /// </summary>
    private void OnCopySeekerAddressClicked()
    {
        if (SeekerWalletManager.Instance != null && SeekerWalletManager.Instance.IsConnected)
        {
            var address = SeekerWalletManager.Instance.GetSeekerAddress();
            if (!string.IsNullOrEmpty(address))
            {
                GUIUtility.systemCopyBuffer = address;
                SetStatus("Seeker address copied to clipboard!", Color.green);

                // Show toast
                if (TransactionToastManager.Instance != null)
                {
                    TransactionToastManager.Instance.ShowToastTop("Address copied!", Color.green);
                }
            }
        }
    }

    /// <summary>
    /// Copy Privy wallet address to clipboard
    /// </summary>
    private void OnCopyPrivyAddressClicked()
    {
        if (CustomPrivyWalletAdapter.Instance != null && CustomPrivyWalletAdapter.Instance.IsReady())
        {
            var address = CustomPrivyWalletAdapter.Instance.GetWalletAddress();
            if (!string.IsNullOrEmpty(address))
            {
                GUIUtility.systemCopyBuffer = address;
                SetStatus("Privy address copied to clipboard!", Color.green);

                // Show toast
                if (TransactionToastManager.Instance != null)
                {
                    TransactionToastManager.Instance.ShowToastTop("Address copied!", Color.green);
                }
            }
        }
    }

    /// <summary>
    /// Set status message
    /// </summary>
    private void SetStatus(string message, Color color)
    {
        if (transferStatusText != null)
        {
            transferStatusText.text = message;
            transferStatusText.color = color;
        }
    }

    /// <summary>
    /// Clear status message
    /// </summary>
    private void ClearStatus()
    {
        if (transferStatusText != null)
        {
            transferStatusText.text = "";
        }
    }

    /// <summary>
    /// Set loading indicator visibility
    /// </summary>
    private void SetLoading(bool loading)
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(loading);
        }
    }
}




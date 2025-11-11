using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the transaction signing modal UI
/// Since Privy SDK is headless, we need to create our own signing modal
/// </summary>
public class TransactionSigningModal : MonoBehaviour
{
    public static TransactionSigningModal Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject modalPanel;
    [SerializeField] private GameObject backgroundOverlay; // Optional: Semi-transparent background overlay
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI transactionDetailsText;
    [SerializeField] private TextMeshProUGUI walletAddressText;
    [SerializeField] private Button approveButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private GameObject loadingIndicator;

    private TaskCompletionSource<bool> userDecisionTask;
    private bool isModalActive = false;

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

    private void Start()
    {
        InitializeModal();
    }

    private void InitializeModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }

        // Initialize background overlay (if provided)
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }

        if (approveButton != null)
        {
            approveButton.onClick.AddListener(OnApproveClicked);
        }

        if (rejectButton != null)
        {
            rejectButton.onClick.AddListener(OnRejectClicked);
        }
    }

    /// <summary>
    /// Shows the signing modal and waits for user decision
    /// Returns true if user approves, false if user rejects
    /// </summary>
    public async Task<bool> ShowSigningModal(string transactionMessage, string walletAddress)
    {
        if (isModalActive)
        {
            Debug.LogWarning("Signing modal is already active. Closing previous modal.");
            await CloseModal();
        }

        isModalActive = true;
        userDecisionTask = new TaskCompletionSource<bool>();

        // Update UI with transaction details
        UpdateModalContent(transactionMessage, walletAddress);

        // Show the modal
        if (modalPanel != null)
        {
            modalPanel.SetActive(true);
        }

        // Show background overlay (if provided)
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(true);
        }

        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }

        // Enable buttons
        if (approveButton != null)
        {
            approveButton.interactable = true;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable = true;
        }

        Debug.Log("Transaction signing modal displayed. Waiting for user decision...");

        // Wait for user decision
        bool userApproved = await userDecisionTask.Task;

        return userApproved;
    }

    /// <summary>
    /// Shows loading state while transaction is being processed
    /// </summary>
    public void ShowLoadingState()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(true);
        }

        if (approveButton != null)
        {
            approveButton.interactable = false;
        }

        if (rejectButton != null)
        {
            rejectButton.interactable = false;
        }
    }

    /// <summary>
    /// Hides loading state
    /// </summary>
    public void HideLoadingState()
    {
        if (loadingIndicator != null)
        {
            loadingIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the modal content with transaction details
    /// </summary>
    private void UpdateModalContent(string transactionMessage, string walletAddress)
    {
        if (titleText != null)
        {
            titleText.text = "Sign Transaction";
        }

        if (transactionDetailsText != null)
        {
            // Format transaction message for display
            string displayMessage = string.IsNullOrEmpty(transactionMessage)
                ? "Transaction details not available"
                : $"Message: {transactionMessage}";

            transactionDetailsText.text = displayMessage;
        }

        if (walletAddressText != null)
        {
            if (!string.IsNullOrEmpty(walletAddress))
            {
                // Show shortened wallet address
                string shortAddress = walletAddress.Length > 12
                    ? $"{walletAddress.Substring(0, 6)}...{walletAddress.Substring(walletAddress.Length - 6)}"
                    : walletAddress;
                walletAddressText.text = $"Wallet: {shortAddress}";
            }
            else
            {
                walletAddressText.text = "Wallet: Not available";
            }
        }
    }

    /// <summary>
    /// Called when user clicks approve button
    /// </summary>
    private void OnApproveClicked()
    {
        Debug.Log("User approved transaction signing");

        if (userDecisionTask != null && !userDecisionTask.Task.IsCompleted)
        {
            userDecisionTask.SetResult(true);
        }

        ShowLoadingState();
    }

    /// <summary>
    /// Called when user clicks reject button
    /// </summary>
    private void OnRejectClicked()
    {
        Debug.Log("User rejected transaction signing");

        if (userDecisionTask != null && !userDecisionTask.Task.IsCompleted)
        {
            userDecisionTask.SetResult(false);
        }

        CloseModal();
    }

    /// <summary>
    /// Closes the modal
    /// </summary>
    public async Task CloseModal()
    {
        if (modalPanel != null)
        {
            modalPanel.SetActive(false);
        }

        // Hide background overlay (if provided)
        if (backgroundOverlay != null)
        {
            backgroundOverlay.SetActive(false);
        }

        isModalActive = false;

        // If task is still pending, cancel it
        if (userDecisionTask != null && !userDecisionTask.Task.IsCompleted)
        {
            userDecisionTask.SetResult(false);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Checks if modal is currently active
    /// </summary>
    public bool IsModalActive()
    {
        return isModalActive;
    }
}


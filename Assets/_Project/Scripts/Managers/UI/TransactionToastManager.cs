using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages toast messages for transaction notifications
/// Shows transaction hashes in green (success) or red (failure)
/// </summary>
public class TransactionToastManager : MonoBehaviour
{
    public static TransactionToastManager Instance { get; private set; }

    [Header("Toast UI Elements")]
    [SerializeField] private GameObject toastPanel;
    [SerializeField] private TextMeshProUGUI toastText;
    [SerializeField] private CanvasGroup toastCanvasGroup; 
    [SerializeField] private float toastDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;

    private Coroutine currentToastCoroutine;

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
        // Subscribe to transaction events
        HybridTransactionService.OnTransactionSuccess += OnTransactionSuccess;
        HybridTransactionService.OnTransactionFailure += OnTransactionFailure;

        // Initialize toast panel as hidden
        if (toastPanel != null)
        {
            toastPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        HybridTransactionService.OnTransactionSuccess -= OnTransactionSuccess;
        HybridTransactionService.OnTransactionFailure -= OnTransactionFailure;
    }

    /// <summary>
    /// Handle successful transaction
    /// </summary>
    private void OnTransactionSuccess(string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            ShowToast("Transaction successful", successColor);
        }
        else
        {
            // Format signature for display show first 5 and last 5 characters
            string shortSignature = signature.Length > 16
                ? $"{signature.Substring(0, 5)}...{signature.Substring(signature.Length - 5)}"
                : signature;

            ShowToast($"Tx: {shortSignature}", successColor);
        }
    }

    /// <summary>
    /// Handle failed transaction
    /// </summary>
    private void OnTransactionFailure(string errorMessage)
    {
        string message = string.IsNullOrEmpty(errorMessage)
            ? "Transaction failed"
            : $"Failed: {errorMessage}";

        ShowToast(message, failureColor);
    }

    /// <summary>
    /// Show a toast message with the specified color
    /// </summary>
    public void ShowToast(string message, Color color)
    {
        if (toastPanel == null || toastText == null)
        {
            Debug.LogWarning("TransactionToastManager: Toast UI elements not assigned!");
            return;
        }

        // Stop any existing toast
        if (currentToastCoroutine != null)
        {
            StopCoroutine(currentToastCoroutine);
        }

        // Start new toast
        currentToastCoroutine = StartCoroutine(ShowToastCoroutine(message, color));
    }

    /// <summary>
    /// Coroutine to show toast with fade in/out animation
    /// </summary>
    private IEnumerator ShowToastCoroutine(string message, Color color)
    {
        // Set message and color
        toastText.text = message;
        toastText.color = color;

        // Show panel
        toastPanel.SetActive(true);

        // Fade in (using CanvasGroup if available, otherwise fade text)
        if (toastCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, 0f, 1f, fadeInDuration));
        }
        else
        {
            yield return StartCoroutine(FadeText(toastText, 0f, 1f, fadeInDuration));
        }

        // Wait for duration
        yield return new WaitForSeconds(toastDuration);

        // Fade out
        if (toastCanvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(toastCanvasGroup, 1f, 0f, fadeOutDuration));
        }
        else
        {
            yield return StartCoroutine(FadeText(toastText, 1f, 0f, fadeOutDuration));
        }

        // Hide panel
        toastPanel.SetActive(false);
    }

    /// <summary>
    /// Fade CanvasGroup alpha from startAlpha to endAlpha over duration
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            canvasGroup.alpha = alpha;
            yield return null;
        }

        // Ensure final alpha
        canvasGroup.alpha = endAlpha;
    }

    /// <summary>
    /// Fade text alpha from startAlpha to endAlpha over duration
    /// </summary>
    private IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color originalColor = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // Ensure final alpha
        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, endAlpha);
    }
}


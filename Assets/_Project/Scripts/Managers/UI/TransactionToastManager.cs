using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages toast messages for transaction notifications
/// Shows transaction hashes in green (success) or red (failure)
/// Supports both top and bottom toast positions
/// </summary>
public class TransactionToastManager : MonoBehaviour
{
    public static TransactionToastManager Instance { get; private set; }

    public enum ToastPosition
    {
        Top,
        Bottom
    }

    [Header("Top Toast UI Elements")]
    [SerializeField] private GameObject topToastPanel;
    [SerializeField] private TextMeshProUGUI topToastText;
    [SerializeField] private CanvasGroup topToastCanvasGroup;

    [Header("Bottom Toast UI Elements")]
    [SerializeField] private GameObject bottomToastPanel;
    [SerializeField] private TextMeshProUGUI bottomToastText;
    [SerializeField] private CanvasGroup bottomToastCanvasGroup;

    [Header("Toast Settings")]
    [SerializeField] private float toastDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Colors")]
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color failureColor = Color.red;

    private Coroutine currentTopToastCoroutine;
    private Coroutine currentBottomToastCoroutine;

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

        // Initialize toast panels as hidden
        if (topToastPanel != null)
        {
            topToastPanel.SetActive(false);
        }
        if (bottomToastPanel != null)
        {
            bottomToastPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        HybridTransactionService.OnTransactionSuccess -= OnTransactionSuccess;
        HybridTransactionService.OnTransactionFailure -= OnTransactionFailure;
    }

    /// <summary>
    /// Handle successful transaction (shows at top)
    /// </summary>
    private void OnTransactionSuccess(string signature)
    {
        if (string.IsNullOrEmpty(signature))
        {
            ShowToast("Transaction successful", successColor, ToastPosition.Top);
        }
        else
        {
            // Format signature for display show first 5 and last 5 characters
            string shortSignature = signature.Length > 16
                ? $"{signature.Substring(0, 5)}...{signature.Substring(signature.Length - 5)}"
                : signature;

            ShowToast($"Tx: {shortSignature}", successColor, ToastPosition.Top);
        }
    }

    /// <summary>
    /// Handle failed transaction (shows at top)
    /// </summary>
    private void OnTransactionFailure(string errorMessage)
    {
        string message = string.IsNullOrEmpty(errorMessage)
            ? "Transaction failed"
            : $"Failed: {errorMessage}";

        ShowToast(message, failureColor, ToastPosition.Top);
    }

    /// <summary>
    /// Show a toast message with the specified color at the specified position
    /// </summary>
    public void ShowToast(string message, Color color, ToastPosition position = ToastPosition.Top)
    {
        if (position == ToastPosition.Top)
        {
            if (topToastPanel == null || topToastText == null)
            {
                Debug.LogWarning("TransactionToastManager: Top toast UI elements not assigned!");
                return;
            }

            // Stop any existing top toast
            if (currentTopToastCoroutine != null)
            {
                StopCoroutine(currentTopToastCoroutine);
            }

            // Start new top toast
            currentTopToastCoroutine = StartCoroutine(ShowToastCoroutine(message, color, ToastPosition.Top));
        }
        else
        {
            if (bottomToastPanel == null || bottomToastText == null)
            {
                Debug.LogWarning("TransactionToastManager: Bottom toast UI elements not assigned!");
                return;
            }

            // Stop any existing bottom toast
            if (currentBottomToastCoroutine != null)
            {
                StopCoroutine(currentBottomToastCoroutine);
            }

            // Start new bottom toast
            currentBottomToastCoroutine = StartCoroutine(ShowToastCoroutine(message, color, ToastPosition.Bottom));
        }
    }

    /// <summary>
    /// Convenience method for showing top toast (backward compatibility)
    /// </summary>
    public void ShowToastTop(string message, Color color)
    {
        ShowToast(message, color, ToastPosition.Top);
    }

    /// <summary>
    /// Convenience method for showing bottom toast
    /// </summary>
    public void ShowToastBottom(string message, Color color)
    {
        ShowToast(message, color, ToastPosition.Bottom);
    }

    /// <summary>
    /// Coroutine to show toast with fade in/out animation
    /// </summary>
    private IEnumerator ShowToastCoroutine(string message, Color color, ToastPosition position)
    {
        GameObject panel;
        TextMeshProUGUI text;
        CanvasGroup canvasGroup;

        // Select UI elements based on position
        if (position == ToastPosition.Top)
        {
            panel = topToastPanel;
            text = topToastText;
            canvasGroup = topToastCanvasGroup;
        }
        else
        {
            panel = bottomToastPanel;
            text = bottomToastText;
            canvasGroup = bottomToastCanvasGroup;
        }

        if (panel == null || text == null)
        {
            Debug.LogWarning($"TransactionToastManager: {position} toast UI elements not assigned!");
            yield break;
        }

        // Set message and color
        text.text = message;
        text.color = color;

        // Show panel
        panel.SetActive(true);

        // Fade in (using CanvasGroup if available, otherwise fade text)
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));
        }
        else
        {
            yield return StartCoroutine(FadeText(text, 0f, 1f, fadeInDuration));
        }

        // Wait for duration
        yield return new WaitForSeconds(toastDuration);

        // Fade out
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));
        }
        else
        {
            yield return StartCoroutine(FadeText(text, 1f, 0f, fadeOutDuration));
        }

        // Hide panel
        panel.SetActive(false);
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


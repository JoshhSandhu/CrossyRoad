using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Manages toast messages for transaction notifications
/// Shows transaction hashes with success (green) or failure (red) background images
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
    [SerializeField] private GameObject topSuccessImage;
    [SerializeField] private GameObject topFailedImage;
    [SerializeField] private TextMeshProUGUI topSuccessText;
    [SerializeField] private TextMeshProUGUI topFailedText;
    [SerializeField] private CanvasGroup topToastCanvasGroup;

    [Header("Bottom Toast UI Elements")]
    [SerializeField] private GameObject bottomToastPanel;
    [SerializeField] private GameObject bottomSuccessImage;
    [SerializeField] private GameObject bottomFailedImage;
    [SerializeField] private TextMeshProUGUI bottomSuccessText;
    [SerializeField] private TextMeshProUGUI bottomFailedText;
    [SerializeField] private CanvasGroup bottomToastCanvasGroup;

    [Header("Toast Settings")]
    [SerializeField] private float toastDuration = 3f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    [Header("Text Color")]
    [SerializeField] private Color textColor = Color.white;

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

        // Initialize success and failed images as hidden
        if (topSuccessImage != null)
        {
            topSuccessImage.SetActive(false);
        }
        if (topFailedImage != null)
        {
            topFailedImage.SetActive(false);
        }
        if (bottomSuccessImage != null)
        {
            bottomSuccessImage.SetActive(false);
        }
        if (bottomFailedImage != null)
        {
            bottomFailedImage.SetActive(false);
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
            ShowToast("Transaction successful", true, ToastPosition.Top);
        }
        else
        {
            // Format signature for display show first 5 and last 5 characters
            string shortSignature = signature.Length > 16
                ? $"{signature.Substring(0, 5)}...{signature.Substring(signature.Length - 5)}"
                : signature;

            ShowToast($"Tx: {shortSignature}", true, ToastPosition.Top);
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

        ShowToast(message, false, ToastPosition.Top);
    }

    /// <summary>
    /// Show a toast message with success or failure image at the specified position
    /// </summary>
    public void ShowToast(string message, bool isSuccess, ToastPosition position = ToastPosition.Top)
    {
        if (position == ToastPosition.Top)
        {
            if (topToastPanel == null)
            {
                Debug.LogWarning("TransactionToastManager: Top toast panel not assigned!");
                return;
            }

            if (isSuccess && topSuccessImage == null)
            {
                Debug.LogWarning("TransactionToastManager: Top success image not assigned!");
                return;
            }

            if (!isSuccess && topFailedImage == null)
            {
                Debug.LogWarning("TransactionToastManager: Top failed image not assigned!");
                return;
            }

            // Stop any existing top toast
            if (currentTopToastCoroutine != null)
            {
                StopCoroutine(currentTopToastCoroutine);
            }

            // Start new top toast
            currentTopToastCoroutine = StartCoroutine(ShowToastCoroutine(message, isSuccess, ToastPosition.Top));
        }
        else
        {
            if (bottomToastPanel == null)
            {
                Debug.LogWarning("TransactionToastManager: Bottom toast panel not assigned!");
                return;
            }

            if (isSuccess && bottomSuccessImage == null)
            {
                Debug.LogWarning("TransactionToastManager: Bottom success image not assigned!");
                return;
            }

            if (!isSuccess && bottomFailedImage == null)
            {
                Debug.LogWarning("TransactionToastManager: Bottom failed image not assigned!");
                return;
            }

            // Stop any existing bottom toast
            if (currentBottomToastCoroutine != null)
            {
                StopCoroutine(currentBottomToastCoroutine);
            }

            // Start new bottom toast
            currentBottomToastCoroutine = StartCoroutine(ShowToastCoroutine(message, isSuccess, ToastPosition.Bottom));
        }
    }

    /// <summary>
    /// Convenience method for showing top toast (backward compatibility with color)
    /// </summary>
    public void ShowToastTop(string message, Color color)
    {
        // Determine if success based on color (green = success, red = failure)
        bool isSuccess = color.g > color.r;
        ShowToast(message, isSuccess, ToastPosition.Top);
    }

    /// <summary>
    /// Convenience method for showing bottom toast (backward compatibility with color)
    /// </summary>
    public void ShowToastBottom(string message, Color color)
    {
        // Determine if success based on color (green = success, red = failure)
        bool isSuccess = color.g > color.r;
        ShowToast(message, isSuccess, ToastPosition.Bottom);
    }

    /// <summary>
    /// Coroutine to show toast with fade in/out animation
    /// </summary>
    private IEnumerator ShowToastCoroutine(string message, bool isSuccess, ToastPosition position)
    {
        GameObject panel;
        GameObject successImage;
        GameObject failedImage;
        TextMeshProUGUI successText;
        TextMeshProUGUI failedText;
        CanvasGroup canvasGroup;

        // Select UI elements based on position
        if (position == ToastPosition.Top)
        {
            panel = topToastPanel;
            successImage = topSuccessImage;
            failedImage = topFailedImage;
            successText = topSuccessText;
            failedText = topFailedText;
            canvasGroup = topToastCanvasGroup;
        }
        else
        {
            panel = bottomToastPanel;
            successImage = bottomSuccessImage;
            failedImage = bottomFailedImage;
            successText = bottomSuccessText;
            failedText = bottomFailedText;
            canvasGroup = bottomToastCanvasGroup;
        }

        if (panel == null)
        {
            Debug.LogWarning($"TransactionToastManager: {position} toast panel not assigned!");
            yield break;
        }

        // Show/hide appropriate image
        if (successImage != null)
        {
            successImage.SetActive(isSuccess);
        }
        if (failedImage != null)
        {
            failedImage.SetActive(!isSuccess);
        }

        // Get the appropriate text component
        TextMeshProUGUI toastText = null;
        if (isSuccess)
        {
            toastText = successText;
        }
        else
        {
            toastText = failedText;
        }

        if (toastText == null)
        {
            Debug.LogWarning($"TransactionToastManager: {position} toast text not found!");
            yield break;
        }

        // Set message and color (always white)
        toastText.text = message;
        toastText.color = textColor;

        // Show panel
        panel.SetActive(true);

        // Fade in (using CanvasGroup if available, otherwise fade text)
        if (canvasGroup != null)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));
        }
        else
        {
            yield return StartCoroutine(FadeText(toastText, 0f, 1f, fadeInDuration));
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
            yield return StartCoroutine(FadeText(toastText, 1f, 0f, fadeOutDuration));
        }

        // Hide panel and images
        if (successImage != null)
        {
            successImage.SetActive(false);
        }
        if (failedImage != null)
        {
            failedImage.SetActive(false);
        }
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


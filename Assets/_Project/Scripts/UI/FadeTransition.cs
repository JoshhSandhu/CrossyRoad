using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class FadeTransition : MonoBehaviour
{
    public static FadeTransition Instance { get; private set; }

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Default Colors")]
    [SerializeField] private Color cyanColor = new Color(0f, 1f, 1f, 0f);
    [SerializeField] private Color blackColor = new Color(0f, 0f, 0f, 0f);

    private bool isTransitioning = false;
    private System.Action onFadeComplete;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        if(fadeImage != null)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0f);
        }
    }

    public void FadeInOut(Color fadecolor, System.Action onComplete = null)
    {
        if (isTransitioning)
        {
            return;
        }
        onFadeComplete = onComplete;
        StartCoroutine(FadeInOutCoroutine(fadecolor));
    }

    public void CyanFade(System.Action onComplete = null)
    {
        FadeInOut(cyanColor, onComplete);
    }

    //public void FadeTo(Color targetColor, System.Action onComplete = null)
    //{
    //    if (isTransitioning) return;

    //    onFadeComplete = onComplete;
    //    StartCoroutine(FadeToCoroutine(targetColor));
    //}

    private IEnumerator FadeInOutCoroutine(Color fadecolor)
    {
        isTransitioning = true;
        float halfDuration = fadeDuration * 0.5f;

        //fade in
        yield return StartCoroutine(FadeToCoroutine(new Color(fadecolor.r, fadecolor.g, fadecolor.b, 1f)));

        if (onFadeComplete != null)
        {
            onFadeComplete.Invoke();
            onFadeComplete = null;
        }
        else
        {
            Debug.LogWarning("No callback to invoke!");
        }
        //wait at full opacity
        yield return new WaitForSeconds(0.5f);
        //fade out
        yield return StartCoroutine(FadeToCoroutine(new Color(fadecolor.r, fadecolor.g, fadecolor.b, 0f)));

        isTransitioning = false;
    }

    private IEnumerator FadeToCoroutine(Color fadecolor)
    {
        if(fadeImage == null)
        {
            yield break;
        }

        Color startColor = fadeImage.color;
        float elapsedTime = 0f;

        while(elapsedTime < fadeDuration)
        {
            if (fadeImage == null)
            {
                break;
            }
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / fadeDuration;
            float easedProgress = fadeCurve.Evaluate(progress);

            fadeImage.color = Color.Lerp(startColor, fadecolor, progress);
            yield return null;
        }
        //fadeImage.color = startColor;
        if (fadeImage != null)
        {
            fadeImage.color = startColor;
        }
    }

    public void SetFadeDuration(float duration)
    {
        fadeDuration = duration;
    }

    public bool IsTransitioning()
    {
        return isTransitioning;
    }
}

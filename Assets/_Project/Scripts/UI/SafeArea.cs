using UnityEngine;

public class SafeArea : MonoBehaviour
{

    [Header("Safe Area Settings")]
    [SerializeField] private bool applyTop = true;
    [SerializeField] private bool applyBottom = true;
    [SerializeField] private bool applyLeft = true;
    [SerializeField] private bool applyRight = true;

    [Header("Padding Override")]
    [SerializeField] private bool useCustomPadding = false;
    [SerializeField] private float customTopPadding = 0f;
    [SerializeField] private float customBottomPadding = 0f;
    [SerializeField] private float customLeftPadding = 0f;
    [SerializeField] private float customRightPadding = 0f;

    private RectTransform rectTransform;
    private Rect lastSafeArea = Rect.zero;
    private Vector2 lastScreenSize = Vector2.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ApplySafeArea();
    }

    // Update is called once per frame
    void Update()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        if (safeArea != lastSafeArea || screenSize != lastScreenSize)
        {
            ApplySafeArea();
            lastSafeArea = safeArea;
            lastScreenSize = screenSize;
        }
    }

    private void ApplySafeArea()
    {
        if (rectTransform == null) return;

        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float topInset = (screenSize.y - safeArea.yMax) / screenSize.y;
        float bottomInset = safeArea.yMin / screenSize.y;
        float leftInset = safeArea.xMin / screenSize.x;
        float rightInset = (screenSize.x - safeArea.xMax) / screenSize.x;

        if (useCustomPadding)
        {
            topInset = Mathf.Max(topInset, customTopPadding);
            bottomInset = Mathf.Max(bottomInset, customBottomPadding);
            leftInset = Mathf.Max(leftInset, customLeftPadding);
            rightInset = Mathf.Max(rightInset, customRightPadding);
        }

        Vector2 anchorMin = Vector2.zero;
        Vector2 anchorMax = Vector2.one;

        if (applyTop) anchorMin.y = topInset;
        if (applyBottom) anchorMax.y = 1f - bottomInset;
        if (applyLeft) anchorMin.x = leftInset;
        if (applyRight) anchorMax.x = 1f - rightInset;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Debug.Log($"SafeArea applied top: {topInset:F3}, bottom: {bottomInset:F3}, left: {leftInset:F3}, right: {rightInset:F3}");
    }

    public void RefreshSafeArea()
    {
        ApplySafeArea();
    }

    public Vector4 GetSafeAreaInsets()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);

        float topInset = (screenSize.y - safeArea.yMax) / screenSize.y;
        float bottomInset = safeArea.yMin / screenSize.y;
        float leftInset = safeArea.xMin / screenSize.x;
        float rightInset = (screenSize.x - safeArea.xMax) / screenSize.x;

        return new Vector4(leftInset, bottomInset, rightInset, topInset);
    }
}

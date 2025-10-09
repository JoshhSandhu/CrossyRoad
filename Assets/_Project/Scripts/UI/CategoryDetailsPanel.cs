using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CategoryDetailPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject categoryDetailPanel;
    [SerializeField] private Transform skinGridParent;
    [SerializeField] private GameObject skinItemPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI categoryTitleText;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("Animation")]
    [SerializeField] private float slideInDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private SkinCategory currentCategory;
    private List<SkinItemUI> skinItems = new List<SkinItemUI>();
    private Vector3 originalPosition;
    private bool isAnimating = false;

    private void Awake()
    {
        if (categoryDetailPanel != null)
        {
            originalPosition = categoryDetailPanel.transform.localPosition;
        }
    }

    private void Start()
    {
        if (categoryDetailPanel != null)
        {
            categoryDetailPanel.SetActive(false);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        SetupGridLayout();
    }

    private void SetupGridLayout()
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.cellSize = new Vector2(200, 250);
            gridLayoutGroup.spacing = new Vector2(20, 20);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = 2; //2 columns for mobile
        }
    }

    public void ShowCategory(SkinCategory category)
    {
        if (isAnimating) return;

        currentCategory = category;

        if (categoryDetailPanel != null)
        {
            categoryDetailPanel.SetActive(true);
        }

        if (categoryTitleText != null)
        {
            categoryTitleText.text = category.ToString().Replace("_", " ");
        }

        LoadSkinsForCategory(category);

        StartCoroutine(SlideIn());
    }

    private void LoadSkinsForCategory(SkinCategory category)
    {
        if (ShopManager.Instance == null || ShopManager.Instance.SkinDatabase == null) return;

        ClearSkinItems();

        SkinData[] categorySkins = ShopManager.Instance.SkinDatabase.GetSkinsByCategory(category);

        foreach (SkinData skin in categorySkins)
        {
            CreateSkinItem(skin);
        }
    }

    private void CreateSkinItem(SkinData skin)
    {
        if (skinItemPrefab == null || skinGridParent == null) return;

        GameObject skinItemObj = Instantiate(skinItemPrefab, skinGridParent);
        SkinItemUI skinItemUI = skinItemObj.GetComponent<SkinItemUI>();

        if (skinItemUI != null)
        {
            skinItemUI.SetupSkinItem(skin, OnSkinItemClicked);
            skinItems.Add(skinItemUI);
        }
    }

    private void ClearSkinItems()
    {
        foreach (SkinItemUI skinItem in skinItems)
        {
            if (skinItem != null)
                Destroy(skinItem.gameObject);
        }
        skinItems.Clear();
    }

    private void OnSkinItemClicked(SkinData skin)
    {
        if (skin.isOwned)
        {
            EquipSkin(skin);
        }
        else
        {
            ShowPurchaseConfirmation(skin);
        }
    }

    private void EquipSkin(SkinData skin)
    {
        if (ShopManager.Instance != null)
        {
            ShopManager.Instance.EquipSkin(skin);
        }

        OnBackButtonClicked();
    }

    private void ShowPurchaseConfirmation(SkinData skin)
    {
        Debug.Log($"Purchase confirmation for {skin.skinName}");

        skin.isOwned = true;
        RefreshSkinItems();
    }

    private void RefreshSkinItems()
    {
        foreach (SkinItemUI skinItem in skinItems)
        {
            if (skinItem != null)
                skinItem.RefreshUI();
        }
    }

    private void OnBackButtonClicked()
    {
        if (isAnimating) return;

        StartCoroutine(SlideOut());
    }

    private System.Collections.IEnumerator SlideIn()
    {
        isAnimating = true;

        if (categoryDetailPanel != null)
        {
            categoryDetailPanel.SetActive(true);

            Vector3 startPos = originalPosition + new Vector3(Screen.width, 0, 0);
            categoryDetailPanel.transform.localPosition = startPos;

            float elapsedTime = 0f;

            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / slideInDuration;
                float easedProgress = slideCurve.Evaluate(progress);

                categoryDetailPanel.transform.localPosition = Vector3.Lerp(startPos, originalPosition, easedProgress);

                yield return null;
            }

            categoryDetailPanel.transform.localPosition = originalPosition;
        }

        isAnimating = false;
    }

    private System.Collections.IEnumerator SlideOut()
    {
        isAnimating = true;

        if (categoryDetailPanel != null)
        {
            Vector3 startPos = categoryDetailPanel.transform.localPosition;
            Vector3 endPos = originalPosition + new Vector3(Screen.width, 0, 0);

            float elapsedTime = 0f;

            while (elapsedTime < slideInDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / slideInDuration;
                float easedProgress = slideCurve.Evaluate(progress);

                categoryDetailPanel.transform.localPosition = Vector3.Lerp(startPos, endPos, easedProgress);

                yield return null;
            }

            categoryDetailPanel.SetActive(false);
        }

        isAnimating = false;
    }
}

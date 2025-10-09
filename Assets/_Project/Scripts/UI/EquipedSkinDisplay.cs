using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquippedSkinDisplay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image skinIconImage;
    [SerializeField] private TextMeshProUGUI skinNameText;
    [SerializeField] private TextMeshProUGUI skinCategoryText;
    [SerializeField] private TextMeshProUGUI runsText;
    [SerializeField] private TextMeshProUGUI topScoreText;
    [SerializeField] private Button infoButton;
    [SerializeField] private Image backgroundImage;

    [Header("Stats")]
    [SerializeField] private int runs = 0;
    [SerializeField] private int topScore = 0;

    private SkinData currentSkin;

    private void Start()
    {
        if (infoButton != null)
        {
            infoButton.onClick.AddListener(OnInfoButtonClicked);
        }

        UpdateEquippedSkin();
    }

    public void UpdateEquippedSkin()
    {
        if (ShopManager.Instance != null)
        {
            SkinData equippedSkin = ShopManager.Instance.GetEquippedSkin();
            if (equippedSkin != null)
            {
                SetEquippedSkin(equippedSkin);
            }
            else
            {
                SetDefaultSkin();
            }
        }
        else
        {
            SetDefaultSkin();
        }
    }

    private void SetEquippedSkin(SkinData skin)
    {
        currentSkin = skin;

        if (skinIconImage != null && skin.skinIcon != null)
        {
            skinIconImage.sprite = skin.skinIcon;
        }

        if (skinNameText != null)
        {
            skinNameText.text = skin.skinName.ToUpper();
        }

        if (skinCategoryText != null)
        {
            skinCategoryText.text = skin.category.ToString().Replace("_", " ");
        }

        if (runsText != null)
        {
            runsText.text = runs.ToString();
        }

        if (topScoreText != null)
        {
            topScoreText.text = topScore.ToString();
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = GetRarityColor(skin.rarity);
        }
    }

    private void SetDefaultSkin()
    {
        if (skinNameText != null)
        {
            skinNameText.text = "CHICKEN";
        }

        if (skinCategoryText != null)
        {
            skinCategoryText.text = "ORIGINAL CAST";
        }

        if (runsText != null)
        {
            runsText.text = "0";
        }

        if (topScoreText != null)
        {
            topScoreText.text = "0";
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = Color.white;
        }
    }

    private Color GetRarityColor(SkinRarity rarity)
    {
        switch (rarity)
        {
            case SkinRarity.Common:
                return Color.white;
            case SkinRarity.Rare:
                return Color.blue;
            case SkinRarity.Epic:
                return Color.magenta;
            case SkinRarity.Legendary:
                return Color.yellow;
            default:
                return Color.white;
        }
    }

    private void OnInfoButtonClicked()
    {
        Debug.Log($"Showing info for skin: {currentSkin?.skinName}");
    }

    public void UpdateStats(int newRuns, int newTopScore)
    {
        runs = newRuns;
        topScore = newTopScore;

        if (runsText != null)
        {
            runsText.text = runs.ToString();
        }

        if (topScoreText != null)
        {
            topScoreText.text = topScore.ToString();
        }
    }
}

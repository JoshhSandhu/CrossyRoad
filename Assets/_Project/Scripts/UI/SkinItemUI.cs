using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkinItemUI : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private Image skinIconImage;
    [SerializeField] private TextMeshProUGUI skinNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button purchaseButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image rarityBorderImage;
    [SerializeField] private GameObject ownedIndicator;
    [SerializeField] private GameObject equippedIndicator;
    [SerializeField] private GameObject lockedOverlay;

    [Header("Rarity Colors")]
    [SerializeField] private Color commonColor = Color.white;
    [SerializeField] private Color rareColor = Color.blue;
    [SerializeField] private Color epicColor = Color.magenta;
    [SerializeField] private Color legendaryColor = Color.yellow;

    private SkinData skinData;
    private System.Action<SkinData> onSkinClicked;

    private void Awake()
    {
        //setting up listners
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseClicked);
        }
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(OnEquipClicked);
        }
    }

    public void SetupSkinItem(SkinData skin, System.Action<SkinData> onClickCallback)
    {
        skinData = skin;
        onSkinClicked = onClickCallback;

        UpdateUI();
    }

    private void UpdateUI()
    {
        if(skinData == null)
        {
            return;
        }

        //setting the skin icon
        if(skinIconImage != null && skinData.skinIcon != null)
        {
            skinIconImage.sprite = skinData.skinIcon;
        }

        //setting the skin name
        if(skinNameText != null)
        {
            skinNameText.text = skinData.skinName;
        }

        //set skin price
        if(priceText != null)
        {
            if (skinData.isOwned)
            {
                priceText.text = "OWNED";
                priceText.color = Color.green;
            }
            else
            {
                priceText.text = $"{skinData.priceInSolana} SOL";
                priceText.color = Color.white;
            }
        }

        //setting the rareity border color
        if(rarityBorderImage != null)
        {
            rarityBorderImage.color = GetRarityColor(skinData.rarity);
        }

        //show or hide the indicators
        if (ownedIndicator != null)
        {
            ownedIndicator.SetActive(skinData.isOwned);
        }

        if (equippedIndicator != null)
        {
            equippedIndicator.SetActive(skinData.isEquipped);
        }

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!skinData.isOwned);
        }

        //show or hide the buttons
        if (purchaseButton != null)
        {
            purchaseButton.gameObject.SetActive(!skinData.isOwned);
        }

        if (equipButton != null)
        {
            equipButton.gameObject.SetActive(skinData.isOwned && !skinData.isEquipped);
        }

        //updating the background based on if the player owns the skin or not
        if (backgroundImage != null)
        {
            if (skinData.isOwned)
            {
                backgroundImage.color = new Color(0.2f, 0.8f, 0.2f, 0.3f); // Light green
            }
            else
            {
                backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Light gray
            }
        }
    }

    private Color GetRarityColor(SkinRarity rarity)
    {
        switch (rarity)
        {
            case SkinRarity.Common:
                return commonColor;
            case SkinRarity.Rare:
                return rareColor;
            case SkinRarity.Epic:
                return epicColor;
            case SkinRarity.Legendary:
                return legendaryColor;
            default:
                return Color.white;
        }
    }
    private void OnPurchaseClicked()
    {
        if (skinData != null && !skinData.isOwned)
        {
            onSkinClicked?.Invoke(skinData);
        }
    }

    private void OnEquipClicked()
    {
        if (skinData != null && skinData.isOwned)
        {
            onSkinClicked?.Invoke(skinData);
        }
    }

    public void RefreshUI()
    {
        UpdateUI();
    }
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using System.Security.Cryptography;
using UnityEngine.Rendering;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Ui Reference")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private Transform skinGridParent;
    [SerializeField] private GameObject skinItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button connectWalletButton;
    [SerializeField] private TextMeshProUGUI walletStatusText;
    [SerializeField] private TextMeshProUGUI walletAddressText;

    [Header("Skin Data")]
    [SerializeField] private SkinDatabase skinDatabase;

    [Header("Category Buttons")]
    [SerializeField] private Button[] categoryButtons;
    [SerializeField] private TextMeshProUGUI categoryTitleText;

    [Header("Scroll View")]
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GridLayoutGroup gridLayoutGroup;

    [Header("New UI Components")]
    [SerializeField] private EquippedSkinDisplay equippedSkinDisplay;
    [SerializeField] private CategoryDetailPanel categoryDetailPanel;

    private SkinCategory currentCategory = SkinCategory.Man;
    private List<SkinItemUI> skinItems = new List<SkinItemUI>();
    private SkinData currentlySelectedSkin;

    public SkinDatabase SkinDatabase => skinDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        InitializeShop();
        SetupEventListeners();
        SetupCategoryButtons();
        SetupGridLayout();
        LoadSkinsForCategory(currentCategory);
        UpdateWalletStatus();

        //subscribe to Privy authentication events
        AuthenticationFlowManager.OnAuthenticationStateChanged += OnWalletConnectionChanged;
    }

    public void InitializeShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }

        //setup category btns
        //config grid layout
        //load skins for default category
    }

    public void SetupGridLayout()
    {
        if (gridLayoutGroup != null)
        {
            //gridLayoutGroup.cellSize = new Vector2(200, 250);
            //gridLayoutGroup.spacing = new Vector2(20, 20);
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            //gridLayoutGroup.constraintCount = 3;  //3 columns

            //configure grid to start from top-left and fill downward
            gridLayoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayoutGroup.childAlignment = TextAnchor.UpperLeft;

            //gridLayoutGroup.padding = new RectOffset(20, 20, 100, 20);
        }
    }

    public void SetupEventListeners()
    {
        //close btn event listener
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseShop);
        }

        //connect wallet btn event listener
        if (connectWalletButton != null)
        {
            connectWalletButton.onClick.AddListener(ConnectWallet);
        }

        //add the solana events here
        //solana wallet connection
        //solana transaction completed
        //solana transcation failed
    }

    private void OnDestroy()
    {
        // Unsubscribe from Privy authentication events
        AuthenticationFlowManager.OnAuthenticationStateChanged -= OnWalletConnectionChanged;
    }

    private void SetupCategoryButtons()
    {
        if (categoryButtons == null)
        {
            return;
        }

        for (int i = 0; i < categoryButtons.Length; i++)
        {
            int categoryIndex = i;
            categoryButtons[i].onClick.AddListener(() => OnCategoryButtonClicked(categoryIndex));
        }
    }

    private void OnCategoryButtonClicked(int categoryIndex)
    {
        currentCategory = (SkinCategory)categoryIndex;

        // Always load skins directly in the main shop area (red zone)
        LoadSkinsForCategory(currentCategory);
        UpdateCategoryTitle();

        // Optional: You can still use the detail panel for a different view if needed
        // if (categoryDetailPanel != null)
        // {
        //     categoryDetailPanel.ShowCategory(currentCategory);
        // }
    }

    private void LoadSkinsForCategory(SkinCategory category)
    {
        if (skinDatabase == null) { return; }

        //removing the current skin
        ClearSkinItems();

        //getting the sking for the category
        SkinData[] categorySkins = skinDatabase.GetSkinsByCategory(category);

        //creating the skin item UI
        foreach (SkinData skin in categorySkins)
        {
            CreateSkinItem(skin);
        }
    }

    private void CreateSkinItem(SkinData skin)
    {
        if (skinItemPrefab == null || skinGridParent == null) { return; }

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
        currentlySelectedSkin = skin;

        //if the player owns the skin then equip it
        if (skin.isOwned)
        {
            EquipSkin(skin);
        }
        else
        {
            ShowPurchaseConfirmation(skin);  //show the purchase confermation
        }
    }

    private void ShowPurchaseConfirmation(SkinData skin)
    {
        string message = $"Purchase {skin.skinName} for {skin.priceInSolana} SOL?";
        Debug.Log($"Purchase Confermation: {message}");

        ConfirmPurchase();
    }

    private void ConfirmPurchase()
    {
        if (currentlySelectedSkin != null)
        {
            //this is a placeholder for the solana intigration
            PurchaseSkinWithSolana(currentlySelectedSkin.skinId, currentlySelectedSkin.priceInSolana);
        }
    }

    private void PurchaseSkinWithSolana(string skinId, float priceInSolana)
    {
        Debug.Log($"placeholder: Purchasing skin {skinId} for {priceInSolana} SOL");

        //on successful purchase of the skin
        OnSkinPurchased(skinId);
    }

    public void EquipSkin(SkinData skin)
    {
        foreach (SkinData s in skinDatabase.skins)
        {
            s.isEquipped = false;
        }

        //equip the skin
        skin.isEquipped = true;

        //Update the UI
        RefreshSkinItems();

        OnSkinEquipped(skin);

        Debug.Log($"Equipped skin: {skin.skinName}");
    }

    private void RefreshSkinItems()
    {
        foreach (SkinItemUI skinItem in skinItems)
        {
            if (skinItem != null)
                skinItem.RefreshUI();
        }
    }

    private void OnSkinPurchased(string skinId)
    {
        SkinData skin = skinDatabase.GetSkinById(skinId);
        if (skin != null)
        {
            skin.isOwned = true;
            RefreshSkinItems();
            Debug.Log($"Skin purchased and owned: {skin.skinName}");
        }
    }

    private void OnTransactionFailed(string errorMessage)
    {
        Debug.LogError($"Transaction failed: {errorMessage}");
        // Show error message to user
    }

    private void OnWalletConnectionChanged(bool isConnected)
    {
        UpdateWalletStatus();
    }

    private async void UpdateWalletStatus()
    {
        // Get actual wallet status from Privy
        bool isConnected = AuthenticationFlowManager.Instance != null && AuthenticationFlowManager.Instance.IsAuthenticated;
        string walletAddress = "";

        if (isConnected && AuthenticationFlowManager.Instance != null)
        {
            walletAddress = await AuthenticationFlowManager.Instance.GetSolanaWalletAddress();
        }

        if (walletStatusText != null)
        {
            walletStatusText.text = isConnected ? "Connected" : "Not Connected";
            walletStatusText.color = isConnected ? Color.green : Color.red;
        }

        if (walletAddressText != null)
        {
            if (isConnected && !string.IsNullOrEmpty(walletAddress) && walletAddress != "No wallet")
            {
                string shortAddress = walletAddress.Length > 12 ?
                    $"{walletAddress.Substring(0, 6)}...{walletAddress.Substring(walletAddress.Length - 6)}" :
                    walletAddress;
                walletAddressText.text = $"Solana Wallet: {shortAddress}";
            }
            else
            {
                walletAddressText.text = "No Solana wallet connected";
            }
        }

        if (connectWalletButton != null)
        {
            connectWalletButton.gameObject.SetActive(!isConnected);
        }
    }

    private void UpdateCategoryTitle()
    {
        if (categoryTitleText != null)
        {
            categoryTitleText.text = currentCategory.ToString().Replace("_", " ");
        }
    }

    private void ConnectWallet()
    {
        // Use Privy authentication
        if (AuthenticationFlowManager.Instance != null)
        {
            //AuthenticationFlowManager.Instance.ShowAuthPanel();
        }
        else
        {
            Debug.LogError("PrivyAuthManager not found!");
        }
    }

    public void OpenShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(true);
            UpdateWalletStatus();
        }
    }

    public void CloseShop()
    {
        Debug.Log("CloseShop() method called");
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Shop panel is null!");
        }
        TriggerButtonSlideIn();
    }

    private void TriggerButtonSlideIn()
    {
        // Call StartScreenManager to slide buttons back in
        if (StartScreenManager.Instance != null)
        {
            StartScreenManager.Instance.SlideButtonsBackIn();
            Debug.Log("Triggering button slide-in animation via StartScreenManager");
        }
        else
        {
            Debug.LogWarning("StartScreenManager not found!");
        }
    }
    private void OnSkinEquipped(SkinData skin)
    {
        Debug.Log($"Skin equipped: {skin.skinName}");
        if (equippedSkinDisplay != null)
        {
            equippedSkinDisplay.UpdateEquippedSkin();
        }
    }

    //this method gets the current equiped skin
    public SkinData GetEquippedSkin()
    {
        foreach (SkinData skin in skinDatabase.skins)
        {
            if (skin.isEquipped)
                return skin;
        }
        return null;
    }

    public bool IsShopOpen()
    {
        return shopPanel != null && shopPanel.activeSelf;
    }
}


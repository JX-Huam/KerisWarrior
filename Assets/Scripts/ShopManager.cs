using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SimpleShopItem
{
    [Header("Item Configuration")]
    public string itemName;
    public string itemID; // Unique identifier for PlayerPrefs
    public int price;
    public ItemType itemType;
    
    [Header("UI References")]
    public Button buyButton;
}


public enum ItemType
{
    Keris,  // Bullet type
    Cloth   // Player model type
}

public class ShopManager : MonoBehaviour
{
    public AudioSource buySource;
    public AudioClip buySFX;

    [Header("Shop Configuration")]
    public int totalCoins = 0;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private GameObject ShopUI;

    [Header("Shop Items (3 items)")]
    public SimpleShopItem[] shopItems = new SimpleShopItem[3];
    

    // PlayerPrefs keys
    private const string COINS_KEY = "TotalCoins";
    private const string CURRENT_KERIS_KEY = "CurrentKeris";
    private const string CURRENT_CLOTH_KEY = "CurrentCloth";
    private const string OWNED_ITEMS_PREFIX = "OwnedItem_";

    void Start()
    {
        InitializeShop();
    }

    void InitializeShop()
    {
        // Load coins from PlayerPrefs
        totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
        UpdateCoinsUI();
        
        // Setup each shop item
        for (int i = 0; i < shopItems.Length; i++)
        {
            SetupShopItem(i);
        }
        
        // Set default items if none are selected
        if (string.IsNullOrEmpty(PlayerPrefs.GetString(CURRENT_KERIS_KEY, "")))
        {
            PlayerPrefs.SetString(CURRENT_KERIS_KEY, "defaultKeris");
            PlayerPrefs.Save();
        }
        
        if (string.IsNullOrEmpty(PlayerPrefs.GetString(CURRENT_CLOTH_KEY, "")))
        {
            PlayerPrefs.SetString(CURRENT_CLOTH_KEY, "defaultCloth");
            PlayerPrefs.Save();
        }
        
        UpdateAllButtonStates();
    }

    void SetupShopItem(int index)
    {
        if (index >= shopItems.Length) return;
        
        SimpleShopItem item = shopItems[index];
        
        if (item.buyButton != null)
        {
            // Remove existing listeners and add new one
            item.buyButton.onClick.RemoveAllListeners();
            int itemIndex = index; // Capture for closure
            item.buyButton.onClick.AddListener(() => PurchaseItem(itemIndex));
        }
    }

    public void PurchaseItem(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= shopItems.Length) return;
        
        SimpleShopItem item = shopItems[itemIndex];
        
        // Check if already owned
        if (IsItemOwned(item.itemID))
        {
            Debug.Log($"Item {item.itemName} already owned!");
            return;
        }
        
        // Check if player has enough coins
        if (totalCoins < item.price)
        {
            Debug.Log($"Not enough coins! Need {item.price}, have {totalCoins}");
            return;
        }
        
        // Make the purchase
        totalCoins -= item.price;
        buySource.PlayOneShot(buySFX);

        // Mark item as owned
        PlayerPrefs.SetInt(OWNED_ITEMS_PREFIX + item.itemID, 1);
        
        // Set as current item based on type (automatically equip when purchased)
        if (item.itemType == ItemType.Keris)
        {
            PlayerPrefs.SetString(CURRENT_KERIS_KEY, item.itemID);
            Debug.Log($"Keris changed to: {item.itemName}");
        }
        else if (item.itemType == ItemType.Cloth)
        {
            PlayerPrefs.SetString(CURRENT_CLOTH_KEY, item.itemID);
            Debug.Log($"Cloth changed to: {item.itemName}");
        }
        
        // Save coins and data
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
        
        // Update UI
        UpdateCoinsUI();
        UpdateButtonState(itemIndex);
        
        Debug.Log($"Successfully purchased and equipped: {item.itemName}");    
    }

    void UpdateAllButtonStates()
    {
        for (int i = 0; i < shopItems.Length; i++)
        {
            UpdateButtonState(i);
        }
    }

    void UpdateButtonState(int itemIndex)
    {
        if (itemIndex >= shopItems.Length) return;

        SimpleShopItem item = shopItems[itemIndex];

        if (item.buyButton != null)
        {
            bool isOwned = IsItemOwned(item.itemID);
            bool canAfford = totalCoins >= item.price;

            // Button is interactable only if not owned and can afford
            item.buyButton.interactable = !isOwned && canAfford;

            // Optional: Change button text based on state
            Text buttonText = item.buyButton.GetComponentInChildren<Text>();
            TextMeshProUGUI buttonTextTMP = item.buyButton.GetComponentInChildren<TextMeshProUGUI>();

            string displayText = "Buy";
            if (isOwned)
            {
                displayText = "Owned";
            }
            else if (!canAfford)
            {
                displayText = "Buy"; // Keep as "Buy" but non-interactable
            }

            if (buttonText != null)
                buttonText.text = displayText;
            if (buttonTextTMP != null)
                buttonTextTMP.text = displayText;
        }
    }

    void UpdateCoinsUI()
    {
        if (coinsText != null)
        {
            coinsText.text = "Coins: " + totalCoins.ToString();
        }
    }

    // Helper methods
    bool IsItemOwned(string itemID)
    {
        return PlayerPrefs.GetInt(OWNED_ITEMS_PREFIX + itemID, 0) == 1;
    }

    // UI Methods
    public void ShowShopPanel()
    {
        if (ShopUI != null)
        {
            ShopUI.SetActive(true);

            // Refresh data when showing shop
            totalCoins = PlayerPrefs.GetInt(COINS_KEY, 0);
            UpdateCoinsUI();
            UpdateAllButtonStates();
        }
    }

    public void HideShopPanel()
    {
        if (ShopUI != null)
        {
            ShopUI.SetActive(false);
        }
    }

    // Method to add coins (for testing or game rewards)
    public void AddCoins(int amount)
    {
        totalCoins += amount;
        PlayerPrefs.SetInt(COINS_KEY, totalCoins);
        PlayerPrefs.Save();
        UpdateCoinsUI();
        UpdateAllButtonStates();
    }
}
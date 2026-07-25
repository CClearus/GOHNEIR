using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Standard TextMeshPro namespace

public class ShopCanvasManager : MonoBehaviour
{
    // 1. Static Gold variable (accessible anywhere in your game)
    public static int gold = 500;

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public int price;
    }

    [Header("Item Data (Add 6 items here in Inspector)")]
    public List<ShopItem> allItems = new List<ShopItem>();

    [Header("UI References - Gold")]
    [SerializeField] private TextMeshProUGUI goldDisplayText;

    [Header("UI References - Item Display 1")]
    [SerializeField] private TextMeshProUGUI item1NameText;
    [SerializeField] private TextMeshProUGUI item1PriceText;
    [SerializeField] private Button item1Button;

    [Header("UI References - Item Display 2")]
    [SerializeField] private TextMeshProUGUI item2NameText;
    [SerializeField] private TextMeshProUGUI item2PriceText;
    [SerializeField] private Button item2Button;

    // References to the 2 chosen items for this shop instance
    private ShopItem selectedItem1;
    private ShopItem selectedItem2;

    private void Start()
    {
        PickTwoRandomItems();
        SetupButtonListeners();
        UpdateUI();
    }

    /// <summary>
    /// Picks 2 unique items out of the list of 6 possible items.
    /// </summary>
    private void PickTwoRandomItems()
    {
        if (allItems.Count < 2)
        {
            Debug.LogError("ShopCanvasManager: You need at least 2 items in the All Items list!");
            return;
        }

        // Pick first item randomly
        int index1 = Random.Range(0, allItems.Count);
        selectedItem1 = allItems[index1];

        // Pick second item randomly (making sure it's not a duplicate)
        int index2;
        do
        {
            index2 = Random.Range(0, allItems.Count);
        } while (index2 == index1);

        selectedItem2 = allItems[index2];
    }

    /// <summary>
    /// Connects the UI buttons to buy logic dynamically.
    /// </summary>
    private void SetupButtonListeners()
    {
        item1Button.onClick.AddListener(() => TryBuyItem(selectedItem1));
        item2Button.onClick.AddListener(() => TryBuyItem(selectedItem2));
    }

    /// <summary>
    /// Checks gold and subtracts price if affordable.
    /// </summary>
    public void TryBuyItem(ShopItem item)
    {
        if (item == null) return;

        if (gold >= item.price)
        {
            gold -= item.price; // Subtract gold from static variable
            Debug.Log($"Bought {item.itemName} for {item.price} gold. Remaining gold: {gold}");
            UpdateUI();
        }
        else
        {
            Debug.Log($"Not enough gold to buy {item.itemName}!");
        }
    }

    /// <summary>
    /// Refreshes all text elements and button interactability on the Canvas.
    /// </summary>
    private void UpdateUI()
    {
        // Update main gold counter
        if (goldDisplayText != null)
            goldDisplayText.text = $"Gold: {gold}";

        // Update Slot 1 UI
        if (selectedItem1 != null)
        {
            item1NameText.text = selectedItem1.itemName;
            item1PriceText.text = $"${selectedItem1.price}";
            item1Button.interactable = (gold >= selectedItem1.price); // Greys out button if poor
        }

        // Update Slot 2 UI
        if (selectedItem2 != null)
        {
            item2NameText.text = selectedItem2.itemName;
            item2PriceText.text = $"${selectedItem2.price}";
            item2Button.interactable = (gold >= selectedItem2.price); // Greys out button if poor
        }
    }
}
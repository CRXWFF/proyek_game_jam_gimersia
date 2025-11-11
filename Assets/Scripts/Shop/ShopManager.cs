using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("References")]
    public Transform cardShopContainer; // parent to spawn shop cards
    public CardView cardPrefab;
    public GameObject shopPopup;
    // optional TMP in the shop popup that shows player's current coins (matches HUD CoinText)
    public TMP_Text shopCoinText;
    [Header("Reroll")]
    public Button rerollButton;
    public TMP_Text rerollValueText;
    public int rerollPrice = 5;

    [Header("Generation")]
    public int minItems = 1;
    public int maxItems = 4;

    [Header("Pricing")]
    public int priceWordCharm = 10;
    public int priceLexiconVault = 8;
    public int priceGrandVerse = 15;
    public int priceFreshMuse = 12;

    List<GameObject> spawned = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;
    }

    public void OpenShop()
    {
        if (cardShopContainer == null || cardPrefab == null)
        {
            Debug.LogWarning("ShopManager: missing references (cardShopContainer/cardPrefab)");
            return;
        }

        if (shopPopup != null)
            shopPopup.SetActive(true);

        // update shop coin display to match player's HUD
        UpdateShopCoinDisplay();

        ClearShop();

        int count = Random.Range(minItems, maxItems + 1);
        var values = System.Enum.GetValues(typeof(CardModel.PowerType));

        // collect slot transforms under cardShopContainer; if none found, fall back to using the container itself
        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < cardShopContainer.childCount; i++)
        {
            var t = cardShopContainer.GetChild(i);
            if (t == null) continue;
            slots.Add(t);
        }

        // cap generated items to number of slots (if slots exist)
        if (slots.Count > 0)
            count = Mathf.Min(count, slots.Count);

        // set reroll UI value
        if (rerollValueText != null)
            rerollValueText.text = "$" + rerollPrice.ToString();

        // attach reroll listener if button provided (safe to add multiple times as Unity will ignore duplicates)
        if (rerollButton != null)
        {
            rerollButton.onClick.RemoveListener(OnRerollButtonClicked);
            rerollButton.onClick.AddListener(OnRerollButtonClicked);
        }

        GenerateShopItems(slots, count, values);
    }

    public void ClearShop()
    {
        // destroy only spawned shop item gameObjects (do not destroy slot placeholders)
        foreach (var g in spawned)
        {
            if (g != null) Destroy(g);
        }
        spawned.Clear();
    }

    public void CloseShop()
    {
        if (shopPopup != null)
            shopPopup.SetActive(false);
        ClearShop();
    }

    // Regenerate shop items using given slots and available power values
    void GenerateShopItems(List<Transform> slots, int count, System.Array values)
    {
        for (int i = 0; i < count; i++)
        {
            // pick random power (exclude None)
            CardModel.PowerType choice;
            do
            {
                choice = (CardModel.PowerType)values.GetValue(Random.Range(0, values.Length));
            } while (choice == CardModel.PowerType.None);

            var model = new CardModel(choice);

            Transform parentForCard = (slots.Count > 0) ? slots[i] : cardShopContainer;

            var go = Instantiate(cardPrefab.gameObject, parentForCard);
            var cardView = go.GetComponent<CardView>();
            cardView.Setup(model);
            cardView.transform.localPosition = Vector3.zero;
            cardView.transform.localScale = Vector3.one;

            // add ShopItem component to handle price & buy
            var shopItem = go.AddComponent<ShopItem>();
            shopItem.manager = this;
            shopItem.card = cardView;
            shopItem.price = GetPriceForPower(choice);

            spawned.Add(go);
        }
        // update reroll button state based on player's coins
        UpdateShopCoinDisplay();
    }

    int GetPriceForPower(CardModel.PowerType p)
    {
        switch (p)
        {
            case CardModel.PowerType.WordCharm: return priceWordCharm;
            case CardModel.PowerType.LexiconVault: return priceLexiconVault;
            case CardModel.PowerType.GrandVerse: return priceGrandVerse;
            case CardModel.PowerType.FreshMuse: return priceFreshMuse;
            default: return 0;
        }
    }

    // Called by ShopItem when a purchase succeeds
    public void OnItemPurchased(ShopItem item)
    {
        if (item == null) return;
        // move purchased card into special area
        var p = item.card.Model.power;
        if (SpecialAreaManager.Instance != null)
        {
            bool ok = SpecialAreaManager.Instance.AddSpecificPower(p);
            if (!ok)
                Debug.LogWarning("ShopManager: unable to add purchased special to special area (no slot).");
        }

        // destroy shop UI item
        if (item.gameObject != null) Destroy(item.gameObject);

        // update shop coin display (GameManager.ModifyPlayerCoins already updated HUD)
        UpdateShopCoinDisplay();
    }

    // Called when the reroll button is clicked
    public void OnRerollButtonClicked()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ShopManager: GameManager missing for reroll.");
            return;
        }

        if (GameManager.Instance.PlayerCoins < rerollPrice)
        {
            Debug.Log("Not enough coins to reroll shop.");
            return;
        }

        // charge player
        GameManager.Instance.ModifyPlayerCoins(-rerollPrice);

        // regenerate the shop with a new random count and items
        ClearShop();

        int count = Random.Range(minItems, maxItems + 1);
        var values = System.Enum.GetValues(typeof(CardModel.PowerType));

        List<Transform> slots = new List<Transform>();
        for (int i = 0; i < cardShopContainer.childCount; i++)
        {
            var t = cardShopContainer.GetChild(i);
            if (t == null) continue;
            slots.Add(t);
        }

        if (slots.Count > 0)
            count = Mathf.Min(count, slots.Count);

        // update shop coin display and reroll UI
        UpdateShopCoinDisplay();

        if (rerollValueText != null)
            rerollValueText.text = "$" + rerollPrice.ToString();

        GenerateShopItems(slots, count, values);
    }

    void UpdateShopCoinDisplay()
    {
        if (shopCoinText == null) return;
        if (GameManager.Instance == null)
        {
            shopCoinText.text = "$0";
            return;
        }
        shopCoinText.text = "$" + GameManager.Instance.PlayerCoins.ToString();
    }
}

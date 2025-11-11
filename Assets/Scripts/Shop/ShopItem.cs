using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ShopItem : MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
    public ShopManager manager;
    public CardView card;
    public int price;

    // UI for price (optional) - tries to find a child TMP/Text automatically
    Text priceText;
    TMP_Text tmpPriceText;

    void Start()
    {
        // try to find a child Text or TMP and show price
        tmpPriceText = GetComponentInChildren<TMP_Text>(includeInactive: true);
        if (tmpPriceText != null)
        {
            tmpPriceText.text = "$" + price.ToString();
        }
        else
        {
            priceText = GetComponentInChildren<Text>(includeInactive: true);
            if (priceText != null)
                priceText.text = "$" + price.ToString();
        }
    }

    public void Buy()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("ShopItem: GameManager.Instance missing.");
            return;
        }

        if (GameManager.Instance.PlayerCoins >= price)
        {
            GameManager.Instance.ModifyPlayerCoins(-price);
            manager.OnItemPurchased(this);
        }
        else
        {
            Debug.Log("Not enough coins to buy this item.");
            // optional: visual feedback
        }
    }

    public void OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Buy();
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    [Header("References")]
    public DeckManager deck;
    public CardView cardPrefab;
    public Transform handArea;
    public Transform deckVisualPoint;

    [Header("Config")]
    public int handSize = 8;
    public Sprite[] cardSprites; // 52 sprite kartu (4 suit x 13 rank)

    public List<CardView> Cards { get; private set; } = new List<CardView>();

    void Awake()
    {
        if (deck == null)
            deck = DeckManager.Instance;
    }

    public void InitializeHand()
    {
        ResetHand();
        RefillHand();
    }

    public void RefillHand()
    {
        if (deck == null) deck = DeckManager.Instance;
        if (deck == null)
        {
            Debug.LogError("HandManager: DeckManager not assigned.");
            return;
        }

        while (Cards.Count < handSize)
        {
            var model = deck.Draw();
            var card = Instantiate(cardPrefab, handArea);

            var sprite = GetSpriteFor(model);
            card.Setup(model, sprite);

            Cards.Add(card);

            if (deckVisualPoint != null)
                card.DealFrom(deckVisualPoint.position);
        }
    }

    public List<CardView> GetSelectedCards()
    {
        return Cards.Where(c => c != null && c.IsSelected).ToList();
    }

    public void RemoveCards(IEnumerable<CardView> toRemove)
    {
        foreach (var c in toRemove.ToList())
        {
            Cards.Remove(c);
            if (c != null)
                Destroy(c.gameObject);
        }
    }

    public void ResetHand()
    {
        foreach (var c in Cards)
        {
            if (c != null)
                Destroy(c.gameObject);
        }
        Cards.Clear();
    }

    Sprite GetSpriteFor(CardModel m)
    {
        if (cardSprites == null || cardSprites.Length < 52)
            return null;

        int suitIndex = (int)m.suit;      // 0-3
        int rankIndex = m.rank - 2;       // 0-12
        int index = suitIndex * 13 + rankIndex;

        if (index < 0 || index >= cardSprites.Length)
            return null;

        return cardSprites[index];
    }
}

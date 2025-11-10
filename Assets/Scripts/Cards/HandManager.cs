using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandManager : MonoBehaviour
{
    public DeckManager deck;
    public PlayAreaManager playArea;
    public CardView cardPrefab;
    public Transform handArea;
    public int handSize = 5;

    public List<CardView> HandCards { get; private set; } = new List<CardView>();

    private void Awake()
    {
        if (deck == null) deck = DeckManager.Instance;
    }

    public void InitializeHand()
    {
        ClearHand();

        for (int i = 0; i < handSize; i++)
            DrawCardToHand();
    }

    public void DrawCardToHand()
    {
        if (deck == null) deck = DeckManager.Instance;
        if (deck == null) return;

        var model = deck.Draw();

        var card = Instantiate(cardPrefab, handArea);
        card.transform.localScale = Vector3.one;

        card.handManager = this;
        card.playAreaManager = playArea;
        card.currentSlot = null;
        card.CurrentArea = CardView.CardArea.Hand;
        card.Setup(model);

        HandCards.Add(card);
    }

    public void RemoveFromHand(CardView card)
    {
        HandCards.Remove(card);
    }

    public void ReturnCardToHand(CardView card)
    {
        if (card == null) return;

        if (playArea != null)
            playArea.RemoveCardReference(card);

        card.currentSlot = null;
        card.CurrentArea = CardView.CardArea.Hand;

        if (!HandCards.Contains(card))
            HandCards.Add(card);

        card.transform.SetParent(handArea, false);
        card.transform.DOKill();
        card.transform.localScale = Vector3.one;
    }

    public void ReplacePlayedCards(List<CardView> played)
    {
        int count = played.Count;

        foreach (var c in played)
        {
            if (c == null) continue;

            HandCards.Remove(c);
            if (playArea != null)
                playArea.RemoveCardReference(c);

            Destroy(c.gameObject);
        }

        for (int i = 0; i < count; i++)
            DrawCardToHand();
    }

    public void ClearHand()
    {
        foreach (var c in HandCards)
        {
            if (c != null)
                Destroy(c.gameObject);
        }

        HandCards.Clear();
    }
}

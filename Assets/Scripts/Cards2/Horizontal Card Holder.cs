using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using DG.Tweening;
using System.Linq;

public class HorizontalCardHolder : MonoBehaviour
{
    [SerializeField] private Card selectedCard;
    [SerializeReference] private Card hoveredCard;

    private RectTransform rect;

    [Header("Spawn Settings")]
    [SerializeField] private List<CardData> possibleCards;
    [SerializeField] private int cardsToSpawn = 7;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private GameObject cardPrefab;
    public List<Card> cards;

    [Header("Anim Settings")]
    [SerializeField] private float discardAnimDuration = 0.35f;
    [SerializeField] private Ease discardEase = Ease.InOutQuad;
    [SerializeField] private bool tweenCardReturn = true;

    bool isCrossing = false;
    // Pending swap target (store the Card reference instead of an index to avoid stale index issues)
    private int pendingSwapIndex = -1;
    private Card pendingSwapCard = null;
    private List<Card> selectedCards = new List<Card>();


    void Start()
    {
        cards = new List<Card>();
        SpawnRandomCards();

        // for (int i = 0; i < cardsToSpawn; i++)
        // {
        //     Instantiate(slotPrefab, transform);
        // }

        rect = GetComponent<RectTransform>();
        cards = GetComponentsInChildren<Card>().ToList();

        int cardCount = 0;

        foreach (Card card in cards)
        {
            card.PointerEnterEvent.AddListener(CardPointerEnter);
            card.PointerExitEvent.AddListener(CardPointerExit);
            card.BeginDragEvent.AddListener(BeginDrag);
            card.EndDragEvent.AddListener(EndDrag);
            card.SelectEvent.AddListener(OnCardSelected);
            card.name = cardCount.ToString();
            cardCount++;
        }

        StartCoroutine(Frame());

        IEnumerator Frame()
        {
            yield return new WaitForSecondsRealtime(.1f);
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].cardVisual != null)
                    cards[i].cardVisual.UpdateIndex(transform.childCount);
            }
        }
    }

    void SpawnRandomCards()
    {
        cards ??= new List<Card>();

        for (int i = 0; i < cardsToSpawn; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, transform);
            slotObj.name = $"CardSlot_{i}";

            GameObject cardObj = Instantiate(cardPrefab, slotObj.transform);
            Card card = cardObj.GetComponent<Card>();

            if (card == null)
            {
                Debug.LogError($"Slot {i}: Prefab card tidak punya komponen Card!");
                continue;
            }

            CardData randomData = possibleCards[UnityEngine.Random.Range(0, possibleCards.Count)];
            card.Initialize(randomData);

            cards.Add(card);
        }
    }

    private void BeginDrag(Card card)
    {
        selectedCard = card;
        // clear any previous pending swap when starting a new drag
        pendingSwapIndex = -1;
        pendingSwapCard = null;
    }


    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        // If a swap was scheduled during dragging, perform it now (on release)
        if (pendingSwapCard != null)
        {
            SwapCard(pendingSwapCard);
            pendingSwapCard = null;
            pendingSwapIndex = -1;
        }

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

        rect.sizeDelta += Vector2.right;
        rect.sizeDelta -= Vector2.right;

        selectedCard = null;

    }

    void CardPointerEnter(Card card)
    {
        hoveredCard = card;
    }

    void CardPointerExit(Card card)
    {
        hoveredCard = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            if (hoveredCard != null)
            {
                Destroy(hoveredCard.transform.parent.gameObject);
                cards.Remove(hoveredCard);

            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            foreach (Card card in cards)
            {
                card.Deselect();
            }
        }

        if (selectedCard == null)
            return;

        if (isCrossing)
            return;

        for (int i = 0; i < cards.Count; i++)
        {

            // Determine slot parents for selected and current card
            var selSlot = selectedCard.transform.parent;
            var otherSlot = cards[i].transform.parent;

            // Only consider swapping with cards that live under slot parents
            if (selSlot == null || otherSlot == null) continue;
            if (!selSlot.CompareTag("Slot") || !otherSlot.CompareTag("Slot")) continue;

            // Require both slot parents to be direct children of THIS holder transform. This prevents
            // accidental cross-container swaps (eg. assemble <-> hand) that can be triggered by
            // reparenting during drop events or by other holders.
            if (selSlot.parent != this.transform || otherSlot.parent != this.transform) continue;

            int selIndex = selSlot.GetSiblingIndex();
            int otherIndex = otherSlot.GetSiblingIndex();

            // If the selected card moved right past another card that sits to its right, schedule a swap on release
            if (selectedCard.transform.position.x > cards[i].transform.position.x && selIndex < otherIndex)
            {
                pendingSwapIndex = i;
                pendingSwapCard = cards[i];
                isCrossing = true; // prevent detecting further crossings until swap completes
                break;
            }

            // If the selected card moved left past another card that sits to its left, schedule a swap on release
            if (selectedCard.transform.position.x < cards[i].transform.position.x && selIndex > otherIndex)
            {
                pendingSwapIndex = i;
                pendingSwapCard = cards[i];
                isCrossing = true; // prevent detecting further crossings until swap completes
                break;
            }
        }
    }

    // Swap using direct Card reference to avoid index-out-of-range if the cards list changed while dragging
    void SwapCard(Card targetCard)
    {
        if (targetCard == null)
            return;

        // Find current index of the target in our cards list - if not found, abort safely
        int idx = cards.IndexOf(targetCard);
        if (idx < 0 || idx >= cards.Count)
            return;

        // Delegate to existing Swap(int) logic for the actual operation
        Swap(idx);
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard != null ? selectedCard.transform.parent : null; // slot transform
        Transform crossedParent = cards[index] != null ? cards[index].transform.parent : null; // slot transform

        // Safety: ensure both slot parents are still direct children of this holder. If not, abort.
        if (focusedParent == null || crossedParent == null) { isCrossing = false; pendingSwapCard = null; return; }
        if (focusedParent.parent != this.transform || crossedParent.parent != this.transform) { isCrossing = false; pendingSwapCard = null; return; }

        // Determine swap direction by comparing the slot parents' sibling indices when both are valid slot objects.
        // This avoids relying on Card.ParentIndex() and uses the actual slot ordering.
        bool swapIsRight = false;
        if (focusedParent != null && crossedParent != null && focusedParent.CompareTag("Slot") && crossedParent.CompareTag("Slot"))
        {
            int focusedIdx = focusedParent.GetSiblingIndex();
            int crossedIdx = crossedParent.GetSiblingIndex();
            swapIsRight = crossedIdx > focusedIdx;
        }
        else
        {
            // Fallback: if slots are not available, try previous parent-index based comparison if possible
            try
            {
                swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
            }
            catch
            {
                swapIsRight = false;
            }
        }

        // Swap parents so each card moves into the other's slot
        cards[index].transform.SetParent(focusedParent, false);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent, false);
        selectedCard.transform.localPosition = selectedCard.selected ? new Vector3(0, selectedCard.selectionOffset, 0) : Vector3.zero;

        // Refresh internal cards list to match scene order (important for future swaps)
        cards = GetComponentsInChildren<Card>().ToList();

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        // Use the swapIsRight computed above (based on slot sibling indices when available)
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
            if (card.cardVisual != null)
                card.cardVisual.UpdateIndex(transform.childCount);
        }
    }

    // public void DiscardSelectedCard()
    // {
    //     if (selectedCard == null)
    //     {
    //         Debug.LogWarning("Tidak ada kartu yang dipilih untuk dibuang!");
    //         return;
    //     }

    //     // Simpan parent slot sebelum menghapus
    //     Transform parentSlot = selectedCard.transform.parent;

    //     // Hapus kartu lama
    //     cards.Remove(selectedCard);
    //     // Sebelum Destroy(selectedCard.gameObject);
    //     selectedCard.transform
    //         .DOScale(0, 0.2f)
    //         .SetEase(Ease.InBack)
    //         .OnComplete(() =>
    //         {
    //             Destroy(selectedCard.gameObject);

    //             // Spawn kartu baru setelah animasi
    //             SpawnNewCard(parentSlot);
    //         });
    // }

    public void SpawnNewCard(Transform parentSlot)
    {
        if (possibleCards.Count == 0)
        {
            Debug.LogError("List possibleCards kosong!");
            return;
        }

        CardData newData = possibleCards[UnityEngine.Random.Range(0, possibleCards.Count)];
        GameObject newCardObj = Instantiate(cardPrefab, parentSlot);
        Card newCard = newCardObj.GetComponent<Card>();

        if (newCard == null)
        {
            Debug.LogError("Prefab card tidak memiliki komponen Card!");
            return;
        }

        newCard.Initialize(newData);
        cards.Add(newCard);

        // Tambahkan listener baru
        newCard.PointerEnterEvent.AddListener(CardPointerEnter);
        newCard.PointerExitEvent.AddListener(CardPointerExit);
        newCard.BeginDragEvent.AddListener(BeginDrag);
        newCard.EndDragEvent.AddListener(EndDrag);
        newCard.SelectEvent.AddListener(OnCardSelected);

        // Animasi masuk halus
        newCard.transform.localScale = Vector3.zero;
        newCard.transform.DOScale(1f, discardAnimDuration).SetEase(Ease.OutBack);
    }

    // Remove a specific card from this holder's tracking (called when a card is moved out)
    public void RemoveCard(Card c)
    {
        if (c == null) return;
        if (cards.Contains(c)) cards.Remove(c);
    }

    // Add a card into this holder's tracking (called when a card is moved in)
    public void AddCard(Card c)
    {
        if (c == null) return;
        if (!cards.Contains(c))
        {
            cards.Add(c);
            // ensure event listeners are present
            c.PointerEnterEvent.AddListener(CardPointerEnter);
            c.PointerExitEvent.AddListener(CardPointerExit);
            c.BeginDragEvent.AddListener(BeginDrag);
            c.EndDragEvent.AddListener(EndDrag);
            c.SelectEvent.AddListener(OnCardSelected);
        }
    }

    // ✅ Discard semua kartu yang sedang diseleksi
    public void DiscardSelectedCards()
    {
        if (selectedCards.Count == 0)
        {
            Debug.LogWarning("Tidak ada kartu yang dipilih untuk dibuang!");
            return;
        }

        StartCoroutine(DiscardRoutine());
    }

    private IEnumerator DiscardRoutine()
    {
        List<Transform> parentSlots = new List<Transform>(selectedCards.Select(c => c.transform.parent));
        List<Card> cardsToRemove = new List<Card>(selectedCards);

        // Animasi halus sebelum dihapus
        foreach (var card in cardsToRemove)
        {
            cards.Remove(card);
            card.transform.DOScale(0f, discardAnimDuration).SetEase(discardEase);
            card.transform.DOLocalMoveY(150f, discardAnimDuration).SetEase(discardEase);
            CanvasGroup cg = card.GetComponent<CanvasGroup>();
            if (cg)
                cg.DOFade(0f, discardAnimDuration);
        }

        yield return new WaitForSeconds(discardAnimDuration + 0.05f);

        // Destroy kartu lama
        foreach (var card in cardsToRemove)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        selectedCards.Clear();

        // Spawn kartu pengganti
        foreach (var parentSlot in parentSlots)
            SpawnNewCard(parentSlot);
    }

    // ✅ Bisa multi select
    public void OnCardSelected(Card card, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedCards.Contains(card))
                selectedCards.Add(card);
        }
        else
        {
            selectedCards.Remove(card);
        }
    }

}

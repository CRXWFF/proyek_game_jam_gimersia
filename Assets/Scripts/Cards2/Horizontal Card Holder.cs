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
    }


    void EndDrag(Card card)
    {
        if (selectedCard == null)
            return;

        selectedCard.transform.DOLocalMove(selectedCard.selected ? new Vector3(0,selectedCard.selectionOffset,0) : Vector3.zero, tweenCardReturn ? .15f : 0).SetEase(Ease.OutBack);

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

            if (selectedCard.transform.position.x > cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() < cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }

            if (selectedCard.transform.position.x < cards[i].transform.position.x)
            {
                if (selectedCard.ParentIndex() > cards[i].ParentIndex())
                {
                    Swap(i);
                    break;
                }
            }
        }
    }

    void Swap(int index)
    {
        isCrossing = true;

        Transform focusedParent = selectedCard.transform.parent;
        Transform crossedParent = cards[index].transform.parent;

        cards[index].transform.SetParent(focusedParent);
        cards[index].transform.localPosition = cards[index].selected ? new Vector3(0, cards[index].selectionOffset, 0) : Vector3.zero;
        selectedCard.transform.SetParent(crossedParent);

        isCrossing = false;

        if (cards[index].cardVisual == null)
            return;

        bool swapIsRight = cards[index].ParentIndex() > selectedCard.ParentIndex();
        cards[index].cardVisual.Swap(swapIsRight ? -1 : 1);

        //Updated Visual Indexes
        foreach (Card card in cards)
        {
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

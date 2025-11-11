using System.Collections.Generic;
using UnityEngine;

public class PlayAreaManager : MonoBehaviour
{
    public static PlayAreaManager Instance { get; private set; }
    public PlayAreaSlot[] slots;
    // public PlayAreaSlot slotPrefab;

    private void Reset()
    {
        AutoCollectSlots();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        Instance = this;

        if (slots == null || slots.Length == 0)
            AutoCollectSlots();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].index = i;
                slots[i].manager = this;
            }
        }
    }

    void AutoCollectSlots()
    {
        slots = GetComponentsInChildren<PlayAreaSlot>(includeInactive: true);
        // ensure slots are ready to receive drops
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null) continue;
            if (!s.gameObject.activeInHierarchy)
                s.gameObject.SetActive(true);

            var img = s.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                img.raycastTarget = true;
        }
        Debug.Log($"PlayAreaManager: found {slots.Length} slots");
    }

    public void AddSlot()
    {
        // if (slotPrefab == null)
        // {
        //     Debug.LogWarning("PlayAreaManager.AddSlot called but slotPrefab is not assigned.");
        //     return;
        // }

        // var go = Instantiate(slotPrefab, transform);
        // go.manager = this;
        // AutoCollectSlots();

        // reassign indices
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].index = i;
                slots[i].manager = this;
            }
        }
    }

    public List<CardView> GetPlayedCardsInOrder()
    {
        var result = new List<CardView>();
        int i = 0;
        foreach (var slot in slots)
        {
            if (slot != null && slot.currentCard != null)
            {
                Debug.Log($"Slot {i}: {slot.name} berisi {slot.currentCard.name}");
                result.Add(slot.currentCard);
            }
            i++;
        }
        return result;
    }

    public void ClearSlotsOnly()
    {
        foreach (var slot in slots)
        {
            if (slot != null)
                slot.currentCard = null;
        }
    }

    public void RemoveCardReference(CardView card)
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.currentCard == card)
            {
                slot.currentCard = null;
                return;
            }
        }
    }
}

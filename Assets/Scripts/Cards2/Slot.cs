using UnityEngine;
using UnityEngine.EventSystems;

// Simple slot that accepts Card drops. Attach to each assemble slot GameObject (UI element).
public class Slot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData == null || eventData.pointerDrag == null) return;

        var draggedGO = eventData.pointerDrag;
        var draggedCard = draggedGO.GetComponent<Card>();
        if (draggedCard == null) return;

        var originalParent = draggedGO.transform.parent;

        // If this slot already has a card, swap them
        Card existing = null;
        if (transform.childCount > 0)
        {
            var child = transform.GetChild(0);
            existing = child.GetComponent<Card>();
        }

        // Find holders for original parent and this slot (if any)
        var prevHolder = originalParent != null ? originalParent.GetComponentInParent<HorizontalCardHolder>() : null;
        var targetHolder = transform.GetComponentInParent<HorizontalCardHolder>();

        if (existing != null && existing != draggedCard)
        {
            // Move existing back to the original parent (where dragged came from)
            existing.transform.SetParent(originalParent, false);
            existing.transform.localPosition = Vector3.zero;
            existing.transform.localScale = Vector3.one;

            // Put dragged into this slot
            draggedGO.transform.SetParent(transform, false);
            draggedGO.transform.localPosition = Vector3.zero;
            draggedGO.transform.localScale = Vector3.one;
        }
        else
        {
            // Empty slot: just move dragged card here
            draggedGO.transform.SetParent(transform, false);
            draggedGO.transform.localPosition = Vector3.zero;
            draggedGO.transform.localScale = Vector3.one;
        }

        // --- Update holder lists robustly based on actual containment ---
        // Remove draggedCard from its previous holder (if any)
        if (prevHolder != null)
        {
            prevHolder.RemoveCard(draggedCard);
        }

        // Add draggedCard to target holder (if any)
        if (targetHolder != null && !targetHolder.cards.Contains(draggedCard))
        {
            targetHolder.AddCard(draggedCard);
        }

        // For the card we displaced (existing), ensure it's registered with the holder that now contains it
        if (existing != null)
        {
            var existingNewHolder = originalParent != null ? originalParent.GetComponentInParent<HorizontalCardHolder>() : null;
            if (existingNewHolder != null && !existingNewHolder.cards.Contains(existing))
                existingNewHolder.AddCard(existing);

            // Remove from the target holder if it incorrectly remains listed there
            if (targetHolder != null && targetHolder.cards.Contains(existing))
                targetHolder.RemoveCard(existing);
        }

        // Notify AssembleManager to re-evaluate
        var assemble = UnityEngine.Object.FindAnyObjectByType<AssembleManager>();
        if (assemble == null) assemble = UnityEngine.Object.FindFirstObjectByType<AssembleManager>();
        if (assemble != null)
        {
            try { assemble.CheckWord(); } catch { }
        }
    }
}

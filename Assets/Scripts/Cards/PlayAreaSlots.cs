using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PlayAreaSlot : MonoBehaviour, IDropHandler
{
    [HideInInspector] public int index;
    [HideInInspector] public CardView currentCard;
    [HideInInspector] public PlayAreaManager manager;

    private void Awake()
    {
        if (manager == null)
            manager = GetComponentInParent<PlayAreaManager>();

        // slot harus bisa kena raycast
        var img = GetComponent<Image>();
        if (img != null)
        {
            img.raycastTarget = true;
            // make sure image is visible at runtime
            img.enabled = true;
            var c = img.color;
            c.a = 1f;
            img.color = c;
        }

        // ensure transform scale is correct
        if (transform.localScale == Vector3.zero)
            transform.localScale = Vector3.one;

        Debug.Log($"PlayAreaSlot Awake: {name} (manager={(manager != null ? manager.name : "null")})");
    }

    private void OnEnable()
    {
        Debug.Log($"PlayAreaSlot Enabled: {name}");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        var card = eventData.pointerDrag.GetComponent<CardView>();
        if (card == null) return;

        // don't allow special/power cards to be dropped into play slots
        if (card.Model.IsPower)
        {
            Debug.Log("PlayAreaSlot: kartu-kartu spesial tidak bisa di-drop ke area susun.");
            // optional: play a small feedback animation or return card to origin (CardView handles this on end drag)
            return;
        }

        // kalau slot sudah diisi kartu lain (bukan dirinya), untuk sekarang abaikan
        if (currentCard != null && currentCard != card)
            return;

        card.AssignToSlot(this);
    }
}

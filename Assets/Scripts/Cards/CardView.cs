using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Card")]
    private Vector3 rotationDelta;

    [Header("UI")]
    public TMP_Text centerText;
    public TMP_Text cornerTopLeft;
    public TMP_Text cornerBottomRight;
    public Image background;

    [HideInInspector] public HandManager handManager;
    [HideInInspector] public PlayAreaManager playAreaManager;
    [HideInInspector] public PlayAreaSlot currentSlot;

    public CardModel Model { get; private set; }

    public enum CardArea { Hand, PlayArea }
    public CardArea CurrentArea { get; set; } = CardArea.Hand;

    Canvas rootCanvas;
    RectTransform rectTransform;
    CanvasGroup canvasGroup;

    // state drag
    Transform originalParent;
    Vector3 originalPosition;
    CardArea originArea;
    PlayAreaSlot originSlot;
    bool droppedOnSlot;
    Vector2 pointerOffset;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // pastikan default raycast pada canvas group aktif
        canvasGroup.blocksRaycasts = true;

        rootCanvas = GetComponentInParent<Canvas>();

        // pastikan kartu bisa di-klik & di-drag
        if (background == null)
        {
            // coba fallback ke Image pada object atau child supaya raycast tidak hilang jika inspector belum diisi
            background = GetComponent<Image>() ?? GetComponentInChildren<Image>();
        }

        if (background != null)
            background.raycastTarget = true;
    }

    public void Setup(CardModel model)
    {
        Model = model;
        string t = model.text;

        // If this is a power card, show a shorter friendly label
        if (Model.IsPower)
        {
            // Show first letters or the power name
            t = Model.power.ToString();
        }

        if (centerText != null) centerText.text = t;
        if (cornerTopLeft != null) cornerTopLeft.text = t;
        if (cornerBottomRight != null) cornerBottomRight.text = t;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // If this is a special/power card in special area, delegate to SpecialAreaManager
        if (Model.IsPower && SpecialAreaManager.Instance != null)
        {
            SpecialAreaManager.Instance.OnSpecialCardClicked(this);
        }
    }

    // ===== DRAG =====

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>() ?? Object.FindAnyObjectByType<Canvas>();

        droppedOnSlot = false;

        originalParent = transform.parent;
        originalPosition = transform.position;
        originArea = CurrentArea;
        originSlot = currentSlot;

        // biar raycast bisa "tembus" ke slot saat drag
        canvasGroup.blocksRaycasts = false;

        // pindah ke atas canvas supaya nggak ketutupan layout
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform, true);
        }
        else
        {
            Debug.LogWarning($"CardView.OnBeginDrag: no Canvas found for '{name}'. Drag will not move visually until a Canvas exists.");
        }

        Debug.Log($"OnBeginDrag: {name} rootCanvas={(rootCanvas != null ? rootCanvas.name : "null")}");

        transform.DOKill();
        transform.DOScale(1.05f, 0.1f);

        // Hitung offset antara posisi kartu dan pointer agar kartu tidak melompat jauh saat drag
        if (rectTransform != null && rootCanvas != null)
        {
            Vector2 localPointer;
            RectTransform canvasRect = rootCanvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                eventData.position,
                rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
                out localPointer
            );

            // anchoredPosition dari rectTransform relatif ke canvasRect
            pointerOffset = rectTransform.anchoredPosition - localPointer;
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null || rootCanvas == null) return;

        Vector2 localPointer;
        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPointer
        );

        // Set posisi kartu sehingga pointer tetap berada pada titik relatif yang sama di kartu
        rectTransform.anchoredPosition = localPointer + pointerOffset;

        // transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Clamp(rotationDelta.x, -60, 60));

        // sedikit debug jika drag tidak terasa responsif
        // Debug.Log($"OnDrag {name} anchored={rectTransform.anchoredPosition}");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        transform.DOScale(1f, 0.1f);

        Debug.Log($"OnEndDrag: {name} droppedOnSlot={droppedOnSlot}");

        if (!droppedOnSlot)
        {
            // balikin ke posisi awal
            transform.SetParent(originalParent, true);
            transform.position = originalPosition;
            CurrentArea = originArea;
            currentSlot = originSlot;
        }
    }

    // ===== DIPANGGIL DARI SLOT =====

    public void AssignToSlot(PlayAreaSlot slot)
    {
        if (slot == null) return;

        droppedOnSlot = true;

        // hapus referensi lama di play area
        if (playAreaManager != null)
            playAreaManager.RemoveCardReference(this);

        if (currentSlot != null && currentSlot.currentCard == this)
            currentSlot.currentCard = null;

        // keluarkan dari hand
        if (handManager != null)
            handManager.RemoveFromHand(this);

        // set data baru
        currentSlot = slot;
        playAreaManager = slot.manager;
        slot.currentCard = this;
        CurrentArea = CardArea.PlayArea;

        // pindahkan ke slot
        transform.SetParent(slot.transform, true);
        transform.DOKill();
        transform.DOMove(slot.transform.position, 0.15f).SetEase(Ease.OutCubic);
        transform.localScale = Vector3.one;

        if (GameManager.Instance != null)
            GameManager.Instance.UpdateWordPreview();
    
    }
}

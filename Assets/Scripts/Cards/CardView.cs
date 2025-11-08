using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CardView : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image art;
    public TMP_Text rankLabel;
    public TMP_Text suitLabel;
    public Image selectionHighlight;

    [Header("Config")]
    public float selectMoveOffset = 30f;

    public CardModel Model { get; private set; }
    public bool IsSelected { get; private set; }

    Vector3 baseLocalPos;
    bool initialized;

    void Start()
    {
        CacheBasePosition();
    }

    void CacheBasePosition()
    {
        if (initialized) return;
        initialized = true;
        baseLocalPos = transform.localPosition;
    }

    public void Setup(CardModel model, Sprite spriteForCard)
    {
        Model = model;

        if (art != null && spriteForCard != null)
            art.sprite = spriteForCard;

        if (rankLabel != null)
            rankLabel.text = CardModel.RankToString(model.rank);

        if (suitLabel != null)
            suitLabel.text = SuitToSymbol(model.suit);

        if (selectionHighlight != null)
            selectionHighlight.enabled = false;
    }

    public void DealFrom(Vector3 fromWorldPos, float duration = 0.25f)
    {
        CacheBasePosition();

        transform.position = fromWorldPos;
        transform.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.Append(transform.DOLocalMove(baseLocalPos, duration).SetEase(Ease.OutCubic));
        s.Join(transform.DOScale(1f, duration).SetEase(Ease.OutBack));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToggleSelect();
    }

    public void ToggleSelect()
    {
        CacheBasePosition();
        IsSelected = !IsSelected;

        if (selectionHighlight != null)
            selectionHighlight.enabled = IsSelected;

        float targetY = IsSelected ? baseLocalPos.y + selectMoveOffset : baseLocalPos.y;

        transform.DOLocalMoveY(targetY, 0.15f).SetEase(Ease.OutQuad);
        TweenPresets.CardSelectPunch(transform);
    }

    string SuitToSymbol(Suit suit)
    {
        return suit switch
        {
            Suit.Clubs => "♣",
            Suit.Diamonds => "♦",
            Suit.Hearts => "♥",
            Suit.Spades => "♠",
            _ => "?"
        };
    }
}

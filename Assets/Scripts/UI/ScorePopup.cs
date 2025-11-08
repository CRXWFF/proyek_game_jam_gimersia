using TMPro;
using UnityEngine;
using DG.Tweening;

public class ScorePopup : MonoBehaviour
{
    public TMP_Text text;
    public float moveUpDistance = 60f;
    public float duration = 0.6f;

    void Reset()
    {
        text = GetComponentInChildren<TMP_Text>();
    }

    public void Show(string message)
    {
        if (text == null)
            text = GetComponentInChildren<TMP_Text>();

        text.text = message;

        var cg = GetComponent<CanvasGroup>();
        if (cg == null)
            cg = gameObject.AddComponent<CanvasGroup>();

        cg.alpha = 0f;

        transform.localScale = Vector3.one;

        Sequence s = DOTween.Sequence();
        s.Append(cg.DOFade(1f, 0.1f));
        s.Join(transform.DOLocalMoveY(transform.localPosition.y + moveUpDistance, duration));
        s.AppendInterval(0.1f);
        s.Append(cg.DOFade(0f, 0.2f));
        s.OnComplete(() => Destroy(gameObject));
    }
}

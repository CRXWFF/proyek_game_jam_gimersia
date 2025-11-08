using UnityEngine;
using DG.Tweening;

public static class TweenPresets
{
    public static void CardSelectPunch(Transform t)
    {
        if (t == null) return;
        t.DOPunchScale(new Vector3(0.05f, 0.05f, 0f), 0.15f, 8, 0.8f);
    }

    public static void ButtonClick(Transform t)
    {
        if (t == null) return;
        Sequence s = DOTween.Sequence();
        s.Append(t.DOScale(0.95f, 0.05f));
        s.Append(t.DOScale(1.05f, 0.05f));
        s.Append(t.DOScale(1f, 0.05f));
    }

    public static void EmphasizeText(Transform t)
    {
        if (t == null) return;
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOScale(1.1f, 0.15f).SetLoops(2, LoopType.Yoyo);
    }
}

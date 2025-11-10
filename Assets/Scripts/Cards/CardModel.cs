using System;

[Serializable]
public struct CardModel
{
    public string text;     // 1-3 huruf
    public int basePoints;  // otomatis dari panjang
    public PowerType power;

    public enum PowerType { None = 0, WordCharm = 1, LexiconVault = 2, GrandVerse = 3, FreshMuse = 4 }

    public CardModel(string rawText)
    {
        text = (rawText ?? "").Trim().ToUpperInvariant();
        int len = text.Length;
        power = PowerType.None;

        if (len <= 0) basePoints = 0;
        else if (len == 1) basePoints = 2;
        else if (len == 2) basePoints = 4;
        else basePoints = 6; // 3+ huruf = 6 poin
    }

    public CardModel(PowerType p)
    {
        power = p;
        text = p.ToString().ToUpperInvariant();
        basePoints = 0;
    }

    public bool IsPower => power != PowerType.None;

    public override string ToString() => text;
}

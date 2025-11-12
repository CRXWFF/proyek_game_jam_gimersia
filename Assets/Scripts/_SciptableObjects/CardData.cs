using UnityEngine;

public enum CardType
{
    OneSyllable,
    TwoSyllable,
    ThreeSyllable,
    Special
}

[CreateAssetMenu(fileName = "NewCardData", menuName = "Cards/Card Data")]
public class CardData : ScriptableObject
{
    public Sprite sprite;        // gambar kartu
    public string sukuKata;      // contoh: "BA", "NA", "KA", "RA"
    public CardType type;        // tipe kartu
    public int basePoint;    // nilai poin opsional

    private void OnValidate()
    {
        switch (type)
        {
            case CardType.OneSyllable:
                basePoint = 2;
                break;
            case CardType.TwoSyllable:
                basePoint = 4;
                break;
            case CardType.ThreeSyllable:
                basePoint = 6;
                break;
            default:
                basePoint = 0; // kartu spesial bisa punya aturan poin sendiri
                break;
        }
    }

    public string specialEffectDescription;
}

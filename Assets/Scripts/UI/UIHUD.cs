using TMPro;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text handsText;
    public TMP_Text discardsText;
    public TMP_Text targetText;
    public TMP_Text lastHandText;

    public void SetScore(int value)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {value}";
    }

    public void SetHands(int value)
    {
        if (handsText != null)
            handsText.text = $"Hands: {value}";
    }

    public void SetDiscards(int value)
    {
        if (discardsText != null)
            discardsText.text = $"Discards: {value}";
    }

    public void SetTarget(int value)
    {
        if (targetText != null)
            targetText.text = $"Target: {value}";
    }

    public void SetLastHand(string msg)
    {
        if (lastHandText != null)
            lastHandText.text = msg;
    }
}

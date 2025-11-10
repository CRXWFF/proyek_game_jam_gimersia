using TMPro;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text targetText;
    public TMP_Text lastHandText;

    public void SetScore(int value)
    {
        if (scoreText != null)
            scoreText.text = $"Score\n\n{value}";
    }

    public void SetTarget(int value)
    {
        if (targetText != null)
            targetText.text = $"Target\n\n{value}";
    }

    public void SetLastHand(string msg)
    {
        if (lastHandText != null)
            lastHandText.text = msg;
    }
}

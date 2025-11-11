using TMPro;
using UnityEngine;

public class UIHUD : MonoBehaviour
{
    public TMP_Text scoreText;
    public TMP_Text targetText;
    public TMP_Text lastHandText;
    public TMP_Text discardsLeftText;
    public TMP_Text assemblesLeftText;
    public TMP_Text wordText;
    public TMP_Text basePointText;
    public TMP_Text baseMultText;

    public void SetScore(int value)
    {
        if (scoreText != null)
            scoreText.text = value.ToString();
    }

    public void SetTarget(int value)
    {
        if (targetText != null)
            targetText.text = value.ToString();
    }

    public void SetLastHand(string msg)
    {
        if (lastHandText != null)
            lastHandText.text = msg;
    }

    public void SetDiscardsleft(int value)
    {
        if (discardsLeftText != null)
            discardsLeftText.text = value.ToString();
    }

    public void SetAssemblesLeft(int value)
    {
        if (assemblesLeftText != null)
            assemblesLeftText.text = value.ToString();
    }

    public void setword(string word)
    {
        if (wordText != null)
            wordText.text = word;
    }

    public void SetBasePoint(int value)
    {
        if (basePointText != null)
            basePointText.text = value.ToString();
    }

    public void SetBaseMult(int value)
    {
        if (baseMultText != null)
            baseMultText.text = value.ToString();
    }
}

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class AssembleManager : MonoBehaviour
{
    [SerializeField] private List<string> validWords;
    [SerializeField] private Transform assembleSlotsParent;
    [SerializeField] private Text resultText;

    public void CheckWord()
    {
        string formedWord = "";

        foreach (Transform slot in assembleSlotsParent)
        {
            if (slot.childCount > 0)
            {
                Card card = slot.GetChild(0).GetComponent<Card>();
                if (card != null && card.cardData != null)
                    formedWord += card.cardData.sukuKata;
            }
        }

        if (validWords.Contains(formedWord))
        {
            resultText.text = $"✅ '{formedWord}' adalah kata valid!";
        }
        else
        {
            resultText.text = $"❌ '{formedWord}' tidak valid.";
        }

        Debug.Log($"[Assemble] Kata dibentuk: {formedWord}");
    }
}

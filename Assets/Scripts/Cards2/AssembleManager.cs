using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class AssembleManager : MonoBehaviour
{
    [SerializeField] private Transform assembleSlotsParent;
    [SerializeField] private Text resultText;

    public void CheckWord()
    {
        List<Card> cards = new List<Card>();
        string formedWord = "";

        foreach (Transform slot in assembleSlotsParent)
        {
            if (slot.childCount > 0)
            {
                Card card = slot.GetChild(0).GetComponent<Card>();
                if (card != null && card.cardData != null)
                {
                    cards.Add(card);
                    formedWord += card.cardData.sukuKata;
                }
            }
        }

        bool isValid = WordValidator.Instance != null && WordValidator.Instance.IsValid(formedWord);

        if (isValid)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ProcessAssemble(cards, formedWord);
            }
            resultText.text = $"✅ '{formedWord}' adalah kata valid!";
            // Clear slots after successful assemble
            foreach (Transform slot in assembleSlotsParent)
            {
                if (slot.childCount > 0)
                {
                    Destroy(slot.GetChild(0).gameObject);
                }
            }
        }
        else
        {
            resultText.text = $"❌ '{formedWord}' tidak valid.";
        }

        Debug.Log($"[Assemble] Kata dibentuk: {formedWord}");
    }
}

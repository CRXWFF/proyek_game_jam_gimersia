using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Kartu yang tersedia")]
    [Tooltip("Daftar teks kartu, misal: KA, A, RAN, dll (sesuai desain Kakartu Sekata)")]
    public List<string> cardDefinitions = new List<string> { "KA", "A", "RAN" };

    [Header("Auto Populate")]
    [Tooltip("If true, card definitions will be populated at runtime from WordValidator's syllable arrays when available.")]
    public bool autoPopulateDefinitions = true;

    [Tooltip("Berapa kopi tiap kartu dimasukkan ke deck")]
    public int copiesPerDefinition = 4;

    private Stack<CardModel> deck = new Stack<CardModel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Build the deck on Awake. BuildAndShuffle will attempt to use WordValidator if autoPopulateDefinitions is true.
        BuildAndShuffle();
    }

    public void BuildAndShuffle()
    {
        var list = new List<CardModel>();

        IEnumerable<string> definitionsToUse = cardDefinitions;

        if (autoPopulateDefinitions)
        {
            // Try to get WordValidator instance; if not set yet, try FindObjectOfType as a fallback
            // Try the singleton first, otherwise attempt to find any instance in the scene.
            var validator = WordValidator.Instance ?? UnityEngine.Object.FindAnyObjectByType<WordValidator>();
            if (validator != null)
            {
                var combined = new List<string>();
                if (validator.oneLetterSyllables != null) combined.AddRange(validator.oneLetterSyllables);
                if (validator.twoLetterSyllables != null) combined.AddRange(validator.twoLetterSyllables);
                if (validator.threeLetterSyllables != null) combined.AddRange(validator.threeLetterSyllables);

                // normalize and remove duplicates
                var normalized = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
                var finalList = new List<string>();
                foreach (var s in combined)
                {
                    if (string.IsNullOrWhiteSpace(s)) continue;
                    var t = s.Trim().ToUpperInvariant();
                    if (!normalized.Contains(t))
                    {
                        normalized.Add(t);
                        finalList.Add(t);
                    }
                }

                if (finalList.Count > 0)
                    definitionsToUse = finalList;
            }
        }

        foreach (var def in definitionsToUse)
        {
            if (string.IsNullOrWhiteSpace(def)) continue;

            for (int i = 0; i < copiesPerDefinition; i++)
            {
                list.Add(new CardModel(def));
            }
        }

        var rng = new System.Random();
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        deck.Clear();
        foreach (var c in list)
            deck.Push(c);
    }

    public bool HasCards => deck.Count > 0;

    public CardModel Draw()
    {
        if (deck.Count == 0)
            BuildAndShuffle();

        return deck.Pop();
    }
}

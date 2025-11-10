using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Kartu yang tersedia")]
    [Tooltip("Daftar teks kartu, misal: KA, A, RAN, dll (sesuai desain Kakartu Sekata)")]
    public List<string> cardDefinitions = new List<string> { "KA", "A", "RAN" };

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
        BuildAndShuffle();
    }

    public void BuildAndShuffle()
    {
        var list = new List<CardModel>();

        foreach (var def in cardDefinitions)
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

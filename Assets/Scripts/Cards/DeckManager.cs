using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool autoBuildOnStart = true;

    private Stack<CardModel> deck = new Stack<CardModel>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (autoBuildOnStart)
        {
            BuildAndShuffle();
        }
    }

    public void BuildAndShuffle()
    {
        var list = new List<CardModel>();

        foreach (Suit s in Enum.GetValues(typeof(Suit)))
        {
            for (int rank = 2; rank <= 14; rank++)
            {
                list.Add(new CardModel(rank, s));
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
        {
            deck.Push(c);
        }
    }

    public bool HasCards => deck.Count > 0;

    public CardModel Draw()
    {
        if (deck.Count == 0)
            BuildAndShuffle();

        return deck.Pop();
    }

    public int Count => deck.Count;
}

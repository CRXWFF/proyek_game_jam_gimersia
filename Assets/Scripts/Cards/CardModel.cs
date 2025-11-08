using System;
using UnityEngine;

[Serializable]
public struct CardModel
{
    [Range(2, 14)]
    public int rank; // 2-10, J=11, Q=12, K=13, A=14
    public Suit suit;

    public CardModel(int rank, Suit suit)
    {
        this.rank = rank;
        this.suit = suit;
    }

    public override string ToString()
    {
        return $"{RankToString(rank)} of {suit}";
    }

    public static string RankToString(int r)
    {
        return r switch
        {
            <= 10 => r.ToString(),
            11 => "J",
            12 => "Q",
            13 => "K",
            14 => "A",
            _ => "?"
        };
    }
}

public enum Suit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}

using System.Collections.Generic;
using System.Linq;

public enum HandRank
{
    HighCard,
    Pair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush
}

public static class PokerHandEvaluator
{
    public static (HandRank rank, int score) Evaluate(List<CardModel> cards)
    {
        if (cards == null || cards.Count != 5)
            return (HandRank.HighCard, 0);

        var ordered = cards.OrderBy(c => c.rank).ToList();

        bool isFlush = cards.All(c => c.suit == cards[0].suit);
        bool isStraight = IsStraight(ordered);

        var groups = cards
            .GroupBy(c => c.rank)
            .OrderByDescending(g => g.Count())
            .ThenByDescending(g => g.Key)
            .ToList();

        int[] counts = groups.Select(g => g.Count()).ToArray();
        bool four = counts[0] == 4;
        bool three = counts[0] == 3;
        int pairCount = counts.Count(c => c == 2);

        if (isStraight && isFlush) return (HandRank.StraightFlush, 100);
        if (four) return (HandRank.FourOfAKind, 80);
        if (three && pairCount == 1) return (HandRank.FullHouse, 60);
        if (isFlush) return (HandRank.Flush, 45);
        if (isStraight) return (HandRank.Straight, 35);
        if (three) return (HandRank.ThreeOfAKind, 25);
        if (pairCount == 2) return (HandRank.TwoPair, 15);
        if (pairCount == 1) return (HandRank.Pair, 8);

        return (HandRank.HighCard, 4);
    }

    private static bool IsStraight(List<CardModel> ordered)
    {
        // handle normal
        bool normal = true;
        for (int i = 0; i < ordered.Count - 1; i++)
        {
            if (ordered[i + 1].rank - ordered[i].rank != 1)
            {
                normal = false;
                break;
            }
        }

        // handle A-2-3-4-5
        bool wheel = false;
        if (!normal)
        {
            var ranks = ordered.Select(c => c.rank).ToList();
            // 14,2,3,4,5
            if (ranks.Contains(14) &&
                ranks.Contains(2) &&
                ranks.Contains(3) &&
                ranks.Contains(4) &&
                ranks.Contains(5))
            {
                wheel = true;
            }
        }

        return normal || wheel;
    }
}

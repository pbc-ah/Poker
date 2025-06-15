namespace Backend.Helpers;

public static class PokerHandEvaluator
{
    private const string RankOrder = "23456789TJQKA";

    private static readonly Dictionary<char, int> RankValue = RankOrder
        .Select((r, i) => new { r, i })
        .ToDictionary(x => x.r, x => x.i);

    public static int Evaluate(List<string> cards)
    {
        if (cards.Count < 5)
            throw new ArgumentException("should never happen, sth went very very wrong");

        return Combinations(cards, 5).Max(ScoreHand);
    }

    private static int ScoreHand(List<string> hand)
    {
        var suits = hand.Select(c => c[0]).ToList();
        var ranks = hand.Select(c => c[1]).ToList();
        var rankCounts = ranks.GroupBy(r => r).ToDictionary(g => g.Key, g => g.Count());

        var isFlush = suits.Distinct().Count() == 1;
        var rankIndices = ranks.Select(r => RankValue[r]).OrderBy(i => i).ToList();

        var isLowStraight = rankIndices.SequenceEqual([0, 1, 2, 3, 12]);
        var isStraight = isLowStraight || rankIndices.Zip(rankIndices.Skip(1), (a, b) => b - a).All(diff => diff == 1);

        var counts = rankCounts.Values.OrderByDescending(v => v).ToList();

        if (isFlush && isStraight && rankIndices.Max() == 12)
            return 9 << 20; // Royal Flush

        if (isFlush && isStraight)
            return 8 << 20 | rankIndices.Max() << 16; // Straight Flush

        if (counts[0] == 4)
        {
            var fourRank = rankCounts.First(kv => kv.Value == 4).Key;
            var kicker = rankCounts.First(kv => kv.Value == 1).Key;
            return 7 << 20 | RankValue[fourRank] << 16 | RankValue[kicker] << 12; // Four of a Kind
        }

        if (counts[0] == 3 && counts[1] == 2)
        {
            var threeRank = rankCounts.First(kv => kv.Value == 3).Key;
            var pairRank = rankCounts.First(kv => kv.Value == 2).Key;
            return 6 << 20 | RankValue[threeRank] << 16 | RankValue[pairRank] << 12; // Full House
        }

        if (isFlush)
            return 5 << 20 | rankIndices.OrderByDescending(i => i).Aggregate(0, (acc, val) => acc << 4 | val); // Flush

        if (isStraight)
            return 4 << 20 | rankIndices.Max() << 16; // Straight

        if (counts[0] == 3)
        {
            var threeRank = rankCounts.First(kv => kv.Value == 3).Key;
            var kickers = rankCounts.Where(kv => kv.Value == 1).Select(kv => RankValue[kv.Key]).OrderByDescending(i => i).ToList();
            return 3 << 20 | RankValue[threeRank] << 16 | kickers[0] << 12 | kickers[1] << 8; // Three of a Kind
        }

        if (counts[0] == 2 && counts[1] == 2)
        {
            var pairs = rankCounts.Where(kv => kv.Value == 2).Select(kv => RankValue[kv.Key]).OrderByDescending(i => i).ToList();
            var kicker = rankCounts.First(kv => kv.Value == 1).Key;
            return 2 << 20 | pairs[0] << 16 | pairs[1] << 12 | RankValue[kicker] << 8; // Two Pair
        }

        if (counts[0] == 2)
        {
            var pairRank = rankCounts.First(kv => kv.Value == 2).Key;
            var kickers = rankCounts.Where(kv => kv.Value == 1).Select(kv => RankValue[kv.Key]).OrderByDescending(i => i).ToList();
            return 1 << 20 | RankValue[pairRank] << 16 | kickers[0] << 12 | kickers[1] << 8 | kickers[2] << 4; // One Pair
        }

        // High card
        var sortedRanks = rankIndices.OrderByDescending(i => i).ToList();
        return sortedRanks.Aggregate(0, (acc, val) => acc << 4 | val);
    }

    private static IEnumerable<List<T>> Combinations<T>(List<T> list, int length)
    {
        if (length == 0)
            yield return new List<T>();
        else
            for (int i = 0; i <= list.Count - length; i++)
                foreach (var tail in Combinations(list.Skip(i + 1).ToList(), length - 1))
                {
                    List<T> result = [list[i]];
                    result.AddRange(tail);
                    yield return result;
                }
    }
}


using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public static class GameHelper
    {
        public static Random Rnd = new Random();

        public static void Shuffle<T>(List<T> list)
        {
            // Fisher-Yates / Knuth shuffle
            for (int i = 0; i < list.Count - 1; i++)
            {
                var j = i + Rnd.Next(list.Count - i);
                (list[j], list[i]) = (list[i], list[j]);
            }
        }

        public static Player SelectFromRandomOtherPlayer(AtomicGame game, Guid playerId)
        {
            var otherPlayers = game.Players.Where(x => x.Id != playerId && !x.IsGameOver()).ToList();
            if (!otherPlayers.Any()) return game.Players.First(); // Single player test
            var otherPlayersWithCards = otherPlayers.Where(x => x.Hand.Any()).ToList();
            if (!otherPlayersWithCards.Any()) return otherPlayers.First();
            return otherPlayersWithCards[Rnd.Next(otherPlayersWithCards.Count)];            
        }
        public static Card SelectRandomCard(CardCollection cards)
        {
            if (!cards.Any()) return null;
            return cards.ElementAt(Rnd.Next(cards.Count));
        }

        public static IOrderedEnumerable<Card> OrderByPriority(IEnumerable<Card> cards)
        {
            return cards.OrderByList(x => x.Type, GetCardTypePriorityList());
        }

        public static CardType[] GetCardTypePriorityList()
        {
            return new[]
            {
                CardType.DefuseCard,
                CardType.AttackCard,
                CardType.NopeCard,
                CardType.SkipCard,
                CardType.SeeTheFutureCard,
                CardType.ShuffleCard,
                CardType.FavorCard
            };
        }

    }

    public static class LinqExtensions
    {

        /// <summary>
        /// Orders collection by same order as a list of keys. Keys not matching list will be ordered last.
        /// </summary>
        public static IOrderedEnumerable<T> OrderByList<T, TKey>(this IEnumerable<T> source, Func<T, TKey> keySelector,
            IEnumerable<TKey> orderedKeys)
        {
            var keyOrderLookup = orderedKeys.Select((key, index) => new { key, index })
                .ToDictionary(x => x.key, y => y.index);
            return source.OrderBy(x =>
            {
                if (keyOrderLookup.TryGetValue(keySelector(x), out var index)) return index;
                return keyOrderLookup.Count + 1;
            }).ThenBy(keySelector);
        }

        /// <summary>
        /// Orders collection by same order as a list of items. Items not matching list will be ordered last.
        /// </summary>
        public static IEnumerable<T> OrderByList<T>(this IEnumerable<T> source, IEnumerable<T> orderedItems)
        {
            return OrderByList(source, x => x, orderedItems);
        }
    }
}
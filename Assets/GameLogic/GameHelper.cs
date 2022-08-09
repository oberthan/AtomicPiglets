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

        public static Player SelectRandomOtherPlayer(AtomicGame game, Guid playerId)
        {
            var otherPlayers = game.Players.Where(x => x.Id != playerId).ToList();
            if (!otherPlayers.Any()) return game.Players.First(); // Single player test
            return otherPlayers[Rnd.Next(otherPlayers.Count)];            
        }
        public static Card SelectRandomCard(CardCollection cards)
        {
            if (!cards.Any()) return null;
            return cards.ElementAt(Rnd.Next(cards.Count));
        }

    }
}
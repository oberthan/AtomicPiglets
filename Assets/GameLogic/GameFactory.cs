using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public static class GameFactory
    {
        public static AtomicGame CreateExplodingKittensLikeGame(int playerCount)
        {
            var players = new List<Player>();
            for (int i = 0; i < playerCount; i++)
                players.Add(new Player($"Player {i}"));
            return CreateExplodingKittensLikeGame(players);
        }

        public static AtomicGame CreateExplodingKittensLikeGame(IEnumerable<Player> players)
        {
            var playerList = players.ToList();
            var playerCount = playerList.Count;
            var deckCount = 1 + playerCount / 6;

            var defusers = new CardCollection();
            var totalDefusers = deckCount * 6;
            var defusersInGame = Math.Min(totalDefusers, playerCount + 2);
            defusers.AddNew(defusersInGame, () => new Card(CardType.DefuseCard));

            var dealPile = new CardCollection();

            // Add defusers to deck
            var dealPileDifuserCount = defusersInGame - playerCount;
            dealPile.AddMany(defusers.DrawTop(dealPileDifuserCount));


            // Add cards that will be initially delt to player hands
            dealPile.AddNew(4, () => new Card(CardType.ShuffleCard));
            dealPile.AddNew(4, () => new Card(CardType.SkipCard));
            dealPile.AddNew(5, () => new Card(CardType.NopeCard));
            dealPile.AddNew(5, () => new Card(CardType.SeeTheFutureCard));
            dealPile.AddNew(4, () => new Card(CardType.AttackCard));
            dealPile.AddNew(4, () => new Card(CardType.FavorCard));
            dealPile.AddNew(4, () => new Card(CardType.WatermelonCard));
            dealPile.AddNew(4, () => new Card(CardType.PotatoCard));
            dealPile.AddNew(4, () => new Card(CardType.BeirdCard));
            dealPile.AddNew(4, () => new Card(CardType.RainbowCard));
            dealPile.AddNew(4, () => new Card(CardType.TacoCard));

            // Duplicate deck for more than 5 players
            var finalDealPile = new CardCollection(dealPile);
            for (int i = 0; i < playerCount / 6; i++)
                finalDealPile.CloneNew(dealPile);
            dealPile = finalDealPile;

            // Shuffle
            dealPile.Shuffle();

            // Deal cards and add 1 defuser to each player hand
            foreach (var player in playerList)
            {
                player.Deal(dealPile.DrawTop(7));
                player.Hand.AddMany(defusers.DrawTop(1));
            }

            var deck = new CardDeck(dealPile);

            // Add nukes to deck
            deck.AddNew(playerCount - 1, () => new Card(CardType.AtomicPigletCard));


            // Shuffle deck
            deck.Shuffle();


            return new AtomicGame(deck, playerList);
        }
    }
}
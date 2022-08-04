using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public static class GameFactory
    {
        public static AtomicGame CreateExplodingKittensLikeGame(int playerCount)
        {
            var defusers = new CardCollection();
            defusers.AddNew(6, () => new DefuseCard());



            var players = new List<Player>();
            for (int i = 0; i < playerCount; i++)
                players.Add(new Player($"Player {i}"));

            var dealPile = new CardCollection();

            // Add defusers to deck
            var dealPileDifuserCount = 2;
            if (playerCount == 5) dealPileDifuserCount--;
            dealPile.AddMany(defusers.DrawTop(dealPileDifuserCount));


            // Add cards that will be initially delt to player hands
            dealPile.AddNew(4, () => new ShuffleCard());
            dealPile.AddNew(4, () => new SkipCard());
            dealPile.AddNew(5, () => new NopeCard());
            dealPile.AddNew(5, () => new SeeTheFutureCard());
            dealPile.AddNew(4, () => new AttackCard());
            dealPile.AddNew(4, () => new FavorCard());
            dealPile.AddNew(4, () => new WatermelonCard());
            dealPile.AddNew(4, () => new PotatoCard());
            dealPile.AddNew(4, () => new BeirdCard());
            dealPile.AddNew(4, () => new RainbowCard());
            dealPile.AddNew(4, () => new TacoCard());

            // Duplicate deck for more than 5 players
            var finalDealPile = new CardCollection(dealPile.All);
            for (int i = 0; i < playerCount / 6; i++)
                finalDealPile = new CardCollection(finalDealPile.All.Concat(dealPile.All));
            dealPile = finalDealPile;

            // Shuffle
            dealPile.Shuffle();

            // Deal cards and add 1 defuser to each player hand
            foreach (var player in players)
            {
                player.Deal(dealPile.DrawTop(7));
                player.Hand.AddMany(defusers.DrawTop(1));
            }

            var deck = new CardDeck(dealPile);

            // Add nukes to deck
            deck.AddNew(playerCount - 1, () => new AtomicPigletCard());


            // Shuffle deck
            deck.Shuffle();


            return new AtomicGame(deck, players);
        }
    }
}
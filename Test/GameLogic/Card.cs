using System;
using System.Collections.Generic;

namespace GameLogic
{
    public abstract class Card
    {
        public string Name { get; set; } = "Card";
    }

    public class AtomicPigletCard : Card
    {

    }
    public class DefuseCard : Card
    {

    }
    public class SkipCard : Card
    {

    }
    public class NopeCard : Card
    {

    }
    public class ShuffleCard : Card
    {

    }
    public class AttackCard : Card
    {

    }
    public class SeeTheFutureCard : Card
    {

    }
    public class FavorCard : Card
    {

    }
    public class CollectionCard : Card
    {

    }

    public class TacoCard : Card
    {

    }
    public class RainbowCard : Card
    {

    }
    public class PotatoCard : Card
    {

    }
    public class BeirdCard : Card
    {

    }
    public class WatermelonCard : Card
    {

    }

    public class CardCollection
    {
        private static Random rnd = new Random();
        private List<Card> cards = new List<Card>();

        public CardCollection()
        {
        }

        public CardCollection(IEnumerable<Card> cards)
        {
            this.cards = cards.ToList();
        }

        public int Count => cards.Count;

        public IEnumerable<Card> All => cards;

        public void Add(Card card)
        {
            cards.Add(card);
        }

        public void AddMany(CardCollection cards)
        {
            this.cards.AddRange(cards.cards);
        }

        public void AddNew(int count, Func<Card> createFunc)
        {
            for(int i = 0; i < count; i++)
            {
                cards.Add(createFunc());
            }
        }

        public void Shuffle()
        {
            // Fisher-Yates / Knuth shuffle
            for (int i = 0; i < cards.Count-1; i++)
            {
                var j = i + rnd.Next(cards.Count - i);
                (cards[j], cards[i]) = (cards[i], cards[j]);
            }
        }

        public CardCollection DrawTop(int count)
        {
            var maxTake = Math.Min(count, cards.Count);
            var top = cards.TakeLast(maxTake).ToList();
            cards.RemoveRange(cards.Count - maxTake, maxTake);
            return new CardCollection(top);
        }
    }

    /// <summary>
    /// Cards that a player holds
    /// </summary>
    public class PlayerHand : CardCollection
    {

    }

    /// <summary>
    /// Played cards
    /// </summary>
    public class DiscardPile : CardCollection
    {

    }

    public class CardDeck : CardCollection
    {
        public CardDeck()
        {
        }
        public CardDeck(CardCollection cards)
        {
            AddMany(cards);
        }

    }


    public class Player
    {
        private string name;

        public Player(string name)
        {
            this.name = name;
        }

        public CardCollection Hand { get; private set; } = new CardCollection();

        public void Deal(CardCollection hand)
        {
            Hand = hand;
        }

        internal void AddCard(Card card)
        {
            Hand.Add(card);
        }
    }

    public class AtomicGame
    {
        public AtomicGame(CardDeck deck, List<Player> players)
        {
            Deck = deck;
            Players = players;
        }

        public CardDeck Deck { get; }

        public List<Player> Players { get; }

        public static AtomicGame CreateExplodingKittensLikeGame(int playerCount)
        {
            var defusers = new CardCollection();
            defusers.AddNew(6, () => new DefuseCard());

            var players = new List<Player>();
            for (int i = 0; i < playerCount; i++)
                players.Add(new Player($"Player {i}"));

            var playerHandPile = new CardCollection();

            // Add cards that will be initially delt to player hands
            playerHandPile.AddNew(5, () => new ShuffleCard());
            playerHandPile.AddNew(5, () => new SkipCard());
            playerHandPile.AddNew(5, () => new NopeCard());
            playerHandPile.AddNew(5, () => new SeeTheFutureCard());

            // Shuffle
            playerHandPile.Shuffle();

            // Deal cards and add 1 defuser to each player hand
            foreach (var player in players)
            {
                player.Deal(playerHandPile.DrawTop(7));
                player.Hand.AddMany(defusers.DrawTop(1));
            }

            var deck = new CardDeck(playerHandPile);

            // Add nukes to deck
            deck.AddNew(playerCount - 1, () => new AtomicPigletCard());

            // Add defusers to deck
            deck.AddMany(defusers.DrawTop(2));


            // Shuffle deck
            deck.Shuffle();


            return new AtomicGame(deck, players);
        }

    }

}
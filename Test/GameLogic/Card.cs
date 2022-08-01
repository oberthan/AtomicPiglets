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
    public class DifuseCard : Card
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
        public void Add(Card card)
        {
            cards.Add(card);
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
            for (int i = 0; i < cards.Count; i++)
            {
                cards[i] = cards[i + rnd.Next(cards.Count - i)];
            }
        }
    }

    public class DiscardPile : CardCollection
    {

    }

    public class CardDeck
    {
        public static CardDeck CreateExplodingKittensLikeGame(int playerCount)
        {
            return new CardDeck();
        }
    }

}
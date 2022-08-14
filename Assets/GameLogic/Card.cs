using System;
using System.Collections.Generic;

namespace GameLogic
{
    public class Card
    {
        public int Id { get; set; }
        public CardType Type { get; set; }
        /// <summary>
        /// Deserialization constructor
        /// </summary>
        public Card() { }
        public Card(CardType cardType)
        {
            Type = cardType;
        }
        public string Name => Type.ToString().Replace("Card", "");


        public override string ToString()
        {
            return Name;
        }
    }

    public enum CardType
    {
        NoCard = 0,
        AtomicPigletCard = 1,
        DefuseCard = 2,
        SkipCard = 3,
        NopeCard = 4,
        ShuffleCard = 5,
        AttackCard = 6,
        SeeTheFutureCard = 7,
        FavorCard = 8,
        TacoCard = 9,
        RainbowCard = 10,
        PotatoCard = 11,
        BeirdCard = 12,
        WatermelonCard = 13
    }

    //public class AtomicPigletCard : Card
    //{
    //    public override string ToString()
    //    {
    //        return "##ATOMIC##";
    //    }

    //}
    //public class DefuseCard : Card
    //{
    //    public override string ToString()
    //    {
    //        return "*Defuse*";
    //    }
    //}
    //public class SkipCard : Card
    //{

    //}
    //public class NopeCard : Card
    //{

    //}
    //public class ShuffleCard : Card
    //{

    //}
    //public class AttackCard : Card
    //{

    //}
    //public class SeeTheFutureCard : Card
    //{

    //}
    //public class FavorCard : Card
    //{

    //}
    //public class CollectionCard : Card
    //{

    //}

    //public class TacoCard : CollectionCard
    //{

    //}
    //public class RainbowCard : CollectionCard
    //{

    //}
    //public class PotatoCard : CollectionCard
    //{

    //}
    //public class BeirdCard : CollectionCard
    //{

    //}
    //public class WatermelonCard : CollectionCard
    //{

    //}

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


}
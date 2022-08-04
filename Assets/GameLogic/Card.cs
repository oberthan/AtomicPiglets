﻿using System;
using System.Collections.Generic;

namespace GameLogic
{
    public abstract class Card
    {
        public string Name => GetType().Name;

        public override string ToString()
        {
            return Name;
        }
    }

    public class AtomicPigletCard : Card
    {
        public override string ToString()
        {
            return "##ATOMIC##";
        }

    }
    public class DefuseCard : Card
    {
        public override string ToString()
        {
            return "*Defuse*";
        }
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

    public class TacoCard : CollectionCard
    {

    }
    public class RainbowCard : CollectionCard
    {

    }
    public class PotatoCard : CollectionCard
    {

    }
    public class BeirdCard : CollectionCard
    {

    }
    public class WatermelonCard : CollectionCard
    {

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


}
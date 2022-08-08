using System;
using System.Collections.Generic;

namespace GameLogic
{
    public interface IGameAction
    {
        void Execute(AtomicGame game);

        string FormatShort();
    }
    public class DrawFromDeckAction : IGameAction
    {
        public Guid PlayerId;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private DrawFromDeckAction() { }
        public DrawFromDeckAction(Player player)
        {
            PlayerId = player.Id;
        }

        public void Execute(AtomicGame game)
        {
            var card = game.Deck.DrawTop();
            var player = game.GetPlayer(PlayerId);
            player.AddCard(card);
            game.PlayerTurns--;
        }

        public string FormatShort()
        {
            return "Draw from deck";
        }
    }

    public interface ICardAction : IGameAction
    {
        Guid PlayerId { get; }
        IEnumerable<Card> Cards { get; }
    }

    public class DefuseAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card defuseCard;
        private Card atomicPigletCard;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private DefuseAction() { }

        public DefuseAction(Player player, Card defuseCard, Card atomicPigletCard)
        {
            PlayerId = player.Id;
            this.defuseCard = defuseCard;
            this.atomicPigletCard = atomicPigletCard;
        }

        public IEnumerable<Card> Cards => new[] { defuseCard };

        public int AtomicPositionFromTop { get; set; }

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
            game.Deck.InsertFromTop(atomicPigletCard, AtomicPositionFromTop);
        }

        public string FormatShort()
        {
            return "Defuse";
        }
    }
    public class SkipAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private SkipAction() { }

        public SkipAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
        }

        public string FormatShort()
        {
            return "Skip";
        }
    }

    public class AttackAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;
        private AttackAction() { }
        public AttackAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.NextPlayer();
            if (game.PlayerTurns == 1)
            {
                game.PlayerTurns += 1;
            }
            else
            {
                game.PlayerTurns += 2;
            }
        }

        public string FormatShort()
        {
            return "Attack!";
        }

    }

    public class ShuffleAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;
        private ShuffleAction() { }
        public ShuffleAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.Deck.Shuffle();
        }

        public string FormatShort()
        {
            return "Shuffle";
        }

    }

    public class SeeTheFutureAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;
        private SeeTheFutureAction() { }
        public SeeTheFutureAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public CardCollection FutureCards { get; private set; }

        public void Execute(AtomicGame game)
        {
            FutureCards = game.Deck.PeekFromTop(3);
        }

        public string FormatShort()
        {
            return "See the future";
        }

    }
    public class FavorAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;
        private FavorAction() { }
        public FavorAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };


        public void Execute(AtomicGame game)
        {
            throw new NotImplementedException();
        }

        public string FormatShort()
        {
            return "Favor";
        }

    }

    public class NopeAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card card;
        private NopeAction() { }
        public NopeAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public CardCollection FutureCards { get; private set; }

        public void Execute(AtomicGame game)
        {
        }

        public string FormatShort()
        {
            return "Nope";
        }

    }

    public class DrawFromPlayerAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card[] cards;
        private DrawFromPlayerAction() { }
        public DrawFromPlayerAction(Player player, Card[] cards)
        {
            PlayerId = player.Id;
            this.cards = cards;
        }

        public IEnumerable<Card> Cards => cards;

        public void Execute(AtomicGame game)
        {
        }

        public string FormatShort()
        {
            return "Draw from player";
        }
    }

    public class DemandCardFromPlayerAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card[] cards;
        private DemandCardFromPlayerAction() { }
        public DemandCardFromPlayerAction(Player player, Card[] cards)
        {
            PlayerId = player.Id;
            this.cards = cards;
        }

        public IEnumerable<Card> Cards => cards;

        public void Execute(AtomicGame game)
        {
        }

        public string FormatShort()
        {
            return "Demand card";
        }
    }

    public class DrawFromDiscardPileAction : ICardAction
    {
        public Guid PlayerId { get; private set; }
        private Card[] cards;
        private DrawFromDiscardPileAction() { }
        public DrawFromDiscardPileAction(Player player, Card[] cards)
        {
            PlayerId = player.Id;
            this.cards = cards;
        }

        public IEnumerable<Card> Cards => cards;

        public void Execute(AtomicGame game)
        {
        }

        public string FormatShort()
        {
            return "Draw from discards";
        }
    }


    public class NextPlayerAction : IGameAction
    {
        public NextPlayerAction()
        {
        }

        public void Execute(AtomicGame game)
        {
            game.NextPlayer();
        }
        public string FormatShort()
        {
            return "Next";
        }

    }
    public class GameOverAction : IGameAction
    {
        private readonly Player player;

        public GameOverAction(Player player)
        {
            this.player = player;
        }

        public void Execute(AtomicGame game)
        {
            throw new NotImplementedException();
        }
        public string FormatShort()
        {
            return "Game over";
        }

    }
}
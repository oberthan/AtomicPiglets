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
        private readonly Player player;

        public DrawFromDeckAction(Player player)
        {
            this.player = player;
        }

        public void Execute(AtomicGame game)
        {
            var card = game.Deck.DrawTop();
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
        IEnumerable<Card> Cards { get; }
    }

    public class DefuseAction : ICardAction
    {
        private readonly Player player;
        private readonly Card defuseCard;
        private readonly Card atomicPigletCard;

        public DefuseAction(Player player, Card defuseCard, Card atomicPigletCard)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public SkipAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public AttackAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public ShuffleAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public SeeTheFutureAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public FavorAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card card;

        public NopeAction(Player player, Card card)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card[] cards;

        public DrawFromPlayerAction(Player player, Card[] cards)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card[] cards;

        public DemandCardFromPlayerAction(Player player, Card[] cards)
        {
            this.player = player;
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
        private readonly Player player;
        private readonly Card[] cards;

        public DrawFromDiscardPileAction(Player player, Card[] cards)
        {
            this.player = player;
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
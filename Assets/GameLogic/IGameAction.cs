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
        private readonly DefuseCard defuseCard;
        private readonly AtomicPigletCard atomicPigletCard;

        public DefuseAction(Player player, DefuseCard defuseCard, AtomicPigletCard atomicPigletCard)
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
        private readonly SkipCard card;

        public SkipAction(Player player, SkipCard card)
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
        private readonly AttackCard card;

        public AttackAction(Player player, AttackCard card)
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
        private readonly ShuffleCard card;

        public ShuffleAction(Player player, ShuffleCard card)
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
        private readonly SeeTheFutureCard card;

        public SeeTheFutureAction(Player player, SeeTheFutureCard card)
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
        private readonly FavorCard card;

        public FavorAction(Player player, FavorCard card)
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
        private readonly NopeCard card;

        public NopeAction(Player player, NopeCard card)
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
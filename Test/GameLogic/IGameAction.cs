namespace GameLogic
{
    public interface IGameAction
    {
        void Execute(AtomicGame game);
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
    }
    public class SkipAction : ICardAction
    {
        private readonly Player player;
        private readonly SkipCard skipCard;

        public SkipAction(Player player, SkipCard skipCard)
        {
            this.player = player;
            this.skipCard = skipCard;
        }

        public IEnumerable<Card> Cards => new[] { skipCard };

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
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
    }
}
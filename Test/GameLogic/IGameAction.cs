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

    public class DefuseAction : IGameAction
    {
        private readonly Player player;
        private readonly DefuseCard defuseCard;

        public DefuseAction(Player player, DefuseCard defuseCard)
        {
            this.player = player;
            this.defuseCard = defuseCard;
        }

        public void Execute(AtomicGame game)
        {
            throw new NotImplementedException();
        }
    }
    public class SkipAction : IGameAction
    {
        private readonly Player player;
        private readonly SkipCard skipCard;

        public SkipAction(Player player, SkipCard skipCard)
        {
            this.player = player;
            this.skipCard = skipCard;
        }

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
            throw new NotImplementedException();
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
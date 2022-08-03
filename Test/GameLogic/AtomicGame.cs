namespace GameLogic
{
    public class AtomicGame
    {
        public AtomicGame(CardCollection deck, List<Player> players)
        {
            Deck = deck;
            Players = players.ToList();
            GameHelper.Shuffle(Players);
            CurrentPlayer = players[0];
            PlayerTurns = 1;
        }

        public CardCollection Deck { get; }

        /// <summary>
        /// Cards played, but not yet executed.
        /// </summary>
        public List<ICardAction> PlayPile { get; } = new List<ICardAction>();

        /// <summary>
        /// Cards played and executed.
        /// </summary>
        public CardCollection DiscardPile { get; } = new CardCollection();


        public List<Player> Players { get; }

        public Player CurrentPlayer { get; private set; }

        public int PlayerTurns;

        /// <summary>
        /// Selects next player as current player cyclic.
        /// </summary>
        public void NextPlayer()
        {
            var playerCount = Players.Count;

            var currentPlayerIndex = Players.IndexOf(CurrentPlayer);
            currentPlayerIndex = (currentPlayerIndex + 1) % playerCount;
            CurrentPlayer = Players[currentPlayerIndex];
            
        }

        public void EndTurn()
        {
            PlayerTurns--;
            if (PlayerTurns == 0)
            {
                NextPlayer();
                PlayerTurns++;
            }
        }

        public void PlayCard(ICardAction cardAction)
        {
            PlayPile.Add(cardAction);
            // TODO: Reset execute timer
        }

        public void ExecutePlayedCards()
        {
            // Execute bottom played cards
            while (PlayPile.Any())
            {
                PlayPile.Last().Execute(this);
                PlayPile.RemoveAt(PlayPile.Count - 1);
            }
        }
    }

}
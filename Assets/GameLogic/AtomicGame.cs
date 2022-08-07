using System.Collections.Generic;
using System.Linq;

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
        public List<ICardAction> PlayPileActions { get; } = new List<ICardAction>();

        public CardCollection PlayPile => new CardCollection(PlayPileActions.SelectMany(x => x.Cards));

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
            PlayPileActions.Add(cardAction);
            // TODO: Reset execute timer
        }

        public void ExecutePlayedCards()
        {
            // Execute bottom played cards
            while (PlayPileActions.Any())
            {
                PlayPileActions.Last().Execute(this);
                PlayPileActions.RemoveAt(PlayPileActions.Count - 1);
            }
        }
    }

}
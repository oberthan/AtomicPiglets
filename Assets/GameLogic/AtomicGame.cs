using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public class AtomicGame
    {
        public IPlayTimer PlayTimer
        {
            get => _playTimer;
            set
            {
                _playTimer.TimerElapsed -= PlayTimerOnTimerElapsed;
                value.TimerElapsed += PlayTimerOnTimerElapsed;
                _playTimer = value;
            }
        }

        public AtomicGame(CardCollection deck, List<Player> players)
        {
            Deck = deck;
            Players = players.ToList();
            GameHelper.Shuffle(Players);
            CurrentPlayer = Players[0];
            PlayerTurns = 1;

            PlayTimer.TimerElapsed += PlayTimerOnTimerElapsed;
        }

        private void PlayTimerOnTimerElapsed(object sender, EventArgs e)
        {
            ExecutePlayedCards();
        }

        public CardCollection Deck { get; }

        internal Player GetPlayer(Guid playerId)
        {
            var player = Players.FirstOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new ArgumentException($"Cannot find a player with id {playerId}. Players: {string.Join(", ", Players)}");
            return Players.Single(x => x.Id == playerId);
        }

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
        private IPlayTimer _playTimer = new ImmediatePlayTimer();

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
            var player = GetPlayer(cardAction.PlayerId);
            player.Hand.RemoveAll(cardAction.Cards);
            PlayPileActions.Add(cardAction);
            PlayTimer.Start(4f);
        }

        public void ExecutePlayedCards()
        {
            // Execute bottom played cards
            while (PlayPileActions.Any())
            {
                var topAction = DiscardTopPlayCards();
                topAction.Execute(this);
            }
        }

        public ICardAction DiscardTopPlayCards()
        {
            var topAction = PlayPileActions.Last();
            var topCardCollection = new CardCollection(topAction.Cards);
            DiscardPile.AddMany(topCardCollection);
            PlayPileActions.RemoveAt(PlayPileActions.Count - 1);
            return topAction;
        }

        public void PlayAction(IGameAction action)
        {
            if (action is ICardAction cardAction)
            {
                PlayCard(cardAction);
            }
            else
            {
                action.Execute(this);
            }
            // Hide future cards
            CurrentPlayer.FutureCards = new CardCollection();
        }
    }

    public class ImmediatePlayTimer : IPlayTimer
    {
        public event EventHandler TimerElapsed;
        public void Start(float delay)
        {
            OnTimerElapsed();
        }

        private void OnTimerElapsed()
        {
            TimerElapsed?.Invoke(this, EventArgs.Empty);
        }
    }

    public interface IPlayTimer
    {
        void Start(float delay);
        event EventHandler TimerElapsed;
    }
}
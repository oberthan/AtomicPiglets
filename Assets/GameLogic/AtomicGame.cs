using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Dto;
using JetBrains.Annotations;
using Newtonsoft.Json;

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

        private List<GameEvent> _gameEvents = new List<GameEvent>();

        public AtomicGame(CardCollection deck, List<Player> players)
        {
            Deck = deck;
            Players = players.ToList();
            GameHelper.Shuffle(Players);
            CurrentPlayer = Players[0];
            PlayerTurns = 1;

            PlayTimer.TimerElapsed += PlayTimerOnTimerElapsed;

            _gameEvents.Add(new GameEvent {Type = GameEventType.NewGameStarted});
        }

        private void PlayTimerOnTimerElapsed(object sender, EventArgs e)
        {
            ExecutePlayedCards();
        }

        public CardCollection Deck { get; }

        public Player GetPlayer(Guid playerId)
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
        public string PublicMessage { get; set; } = "";

        public int PlayerTurns;
        private IPlayTimer _playTimer = new ImmediatePlayTimer();

        /// <summary>
        /// Selects next player as current player cyclic.
        /// </summary>
        public void NextPlayer()
        {
            var playerCount = Players.Count;

            if (Players.All(x => x.IsGameOver()))
            {
                throw new InvalidOperationException("Cannot select next player. All players are game over");
            }

            do
            {
                var currentPlayerIndex = Players.IndexOf(CurrentPlayer);
                currentPlayerIndex = (currentPlayerIndex + 1) % playerCount;
                CurrentPlayer = Players[currentPlayerIndex];
            } while (CurrentPlayer.IsGameOver());

        }

        public void EndTurn()
        {
            PlayerTurns--;
            if (PlayerTurns == 0)
            {
                NextPlayer();
                PlayerTurns = 1;
            }
        }

        public void PlayCard(ICardAction cardAction)
        {
            var player = GetPlayer(cardAction.PlayerId);
            player.Hand.RemoveAll(cardAction.Cards);
            PlayPileActions.Add(cardAction);
            PlayTimer.Start(cardAction.PlayDelay);
        }

        public void ExecutePlayedCards()
        {
            // Execute bottom played cards
            while (PlayPileActions.Any())
            {
                var topAction = DiscardTopPlayCards();
                topAction.Execute(this);

                var gameEvent = CreateGameEventFromAction(topAction);
                if (topAction is IDrawCardAction drawCardAction)
                    gameEvent.DrawCard = drawCardAction.DrawCard;

                _gameEvents.Add(gameEvent);
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

        public GameEvent PlayAction(IGameAction action)
        {
            var player = GetPlayer(action.PlayerId);
            PublicMessage = $"{player.Name} plays {action.FormatShort()}";

            var gameEvent = CreateGameEventFromAction(action);

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

            _gameEvents.Add(gameEvent);

            return gameEvent;
        }

        private GameEvent CreateGameEventFromAction(IGameAction action)
        {
            var gameEvent = new GameEvent
            {
                Type = GameEventType.ActionExecuted,
                ActionType = action.GetType().Name,
                Player = GetPlayer(action.PlayerId).GetPlayerInfo()
            };

            if (action is ICardAction cardAction)
            {
                gameEvent.Type = GameEventType.CardsPlayed;
                gameEvent.PlayCards = cardAction.Cards.ToArray();
                if (cardAction is ITargetGameAction targetGameAction)
                {
                    gameEvent.Target = GetPlayer(targetGameAction.TargetPlayerId).GetPlayerInfo();
                }
            }

            if (action is IDrawCardAction drawCardAction)
            {
                gameEvent.DrawCard = drawCardAction.DrawCard;
            }

            return gameEvent;
        }

        public IEnumerable<Player> GetOtherPlayers(Player player)
        {
            return Players.Where(x => x.Id != player.Id);
        }

        public CardCollection GetAllCards()
        {
            return new CardCollection(Deck.Concat(PlayPile).Concat(DiscardPile)
                .Concat(Players.SelectMany(x => x.Hand)));
        }

        public GameEvent GetLastGameEvent(Player player)
        {
            var lastGameEvent = _gameEvents.Last();

            // Hide information not visible to player
            var drawCard = lastGameEvent.DrawCard;
            if (drawCard != null && // Do not hide if no card was drawn.
                player.Id != lastGameEvent.Player.Id && // Do not hide if player is self.
                player.Id != lastGameEvent.Target?.Id &&  // Do not hide if player was target of draw card.
                drawCard.Type != CardType.AtomicPigletCard) // Do not hide if drawn card is Atomic Piglet. That must be announced.
                drawCard = new Card(CardType.Secret);

            return new GameEvent
            {
                Type = lastGameEvent.Type,
                ActionType = lastGameEvent.ActionType,
                Player = lastGameEvent.Player,
                Target = lastGameEvent.Target,
                PlayCards = lastGameEvent.PlayCards,
                DrawCard = drawCard
            };
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

    public class GameEvent
    {
        public GameEventType Type { get; set; }
        public PlayerInfo Player { get; set; }
        public PlayerInfo Target { get; set; }
        public Card[] PlayCards { get; set; }
        public Card DrawCard { get; set; }
        public string ActionType { get; set; }

        public string FormatShortMessage(Guid playerId)
        {
            switch (Type)
            {
                case GameEventType.NewGameStarted:
                    return "New game started";
                case GameEventType.GameOver:
                    return Player.PlayerName + " game over";
                case GameEventType.GameWon:
                    return Player.PlayerName + " won";
                case GameEventType.CardsPlayed:
                    return Player.PlayerName + " plays "+string.Join(", ",PlayCards.Select(x => x.Name));
                case GameEventType.ActionExecuted:
                    return Player.PlayerName + " executed " + ActionType;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    public enum GameEventType
    {
        NewGameStarted,
        GameOver,
        GameWon,
        CardsPlayed,
        ActionExecuted
    }
}
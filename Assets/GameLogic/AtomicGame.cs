using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Dto;
using UnityEngine;

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

        private readonly List<GameEvent> _gameEvents = new();

        public AtomicGame(CardCollection deck, IEnumerable<Player> players)
        {
            Deck = deck;
            Players = players.ToList();
            GameHelper.Shuffle(Players);
            CurrentPlayer = Players[0];
            PlayerTurns = 1;

            PlayTimer.TimerElapsed += PlayTimerOnTimerElapsed;

            AddGameEvent(new GameEvent {Type = GameEventType.NewGameStarted});
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
        public List<ICardAction> PlayPileActions { get; } = new();

        public CardCollection PlayPile => new(PlayPileActions.SelectMany(x => x.Cards));

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
            
            if (GetActivePlayers().Count() == 1)
                AddGameEvent(new GameEvent {Type = GameEventType.GameWon, Player = CurrentPlayer.GetPlayerInfo()});
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

                var gameEvent = CreateGameEventFromAction(topAction, GameEventType.ActionExecuted);
                Debug.Log("Game execute played cards: "+gameEvent.FormatShortMessage(CurrentPlayer.Id));

                AddGameEvent(gameEvent);
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

            var gameEventType = GameEventType.ActionExecuted;
            if (action is ICardAction cardAction)
            {
                PlayCard(cardAction);
                gameEventType = GameEventType.CardsPlayed;
            }
            else
            {
                action.Execute(this);
            }
            // Hide future cards
            CurrentPlayer.FutureCards = new CardCollection();

            var gameEvent = CreateGameEventFromAction(action, gameEventType);

            Debug.Log("Play action event: " + gameEvent.FormatShortMessage(action.PlayerId));
            AddGameEvent(gameEvent);

            return gameEvent;
        }

        private void AddGameEvent(GameEvent gameEvent)
        {
            gameEvent.EventIndex = _gameEvents.Count;
            gameEvent.GameState = PublicGameState.FromAtomicGame(this);
            _gameEvents.Add(gameEvent);
        }

        private GameEvent CreateGameEventFromAction(IGameAction action, GameEventType eventType)
        {
            var gameEvent = new GameEvent
            {
                Type = eventType,
                ActionType = action.GetType().Name,
                Player = GetPlayer(action.PlayerId).GetPlayerInfo()
            };

            if (action is ICardAction cardAction)
            {
                gameEvent.Type = eventType;
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

        /// <summary>
        /// Gets all game events after some index.
        /// </summary>
        public IEnumerable<GameEvent> GetLastGameEvents(Player player, int lastIndex)
        {
            var events = _gameEvents.AsEnumerable().Reverse().Where(x => x.EventIndex > lastIndex).Reverse().ToList();
            return events.Select(x => GetCensoredGameEvent(player, x));

        }

        public GameEvent GetLastGameEvent(Player player)
        {
            var lastGameEvent = _gameEvents.Last();
            return GetCensoredGameEvent(player, lastGameEvent);
        }

        private static GameEvent GetCensoredGameEvent(Player player, GameEvent gameEvent)
        {
            // Hide information not visible to player
            var drawCard = gameEvent.DrawCard;
            if (drawCard != null && // Do not hide if no card was drawn.
                player.Id != gameEvent.Player.Id && // Do not hide if player is self.
                player.Id != gameEvent.Target?.Id && // Do not hide if player was target of draw card.
                drawCard.Type !=
                CardType.AtomicPigletCard) // Do not hide if drawn card is Atomic Piglet. That must be announced.
                drawCard = new Card(CardType.Secret);

            return new GameEvent
            {
                EventIndex = gameEvent.EventIndex,
                Type = gameEvent.Type,
                ActionType = gameEvent.ActionType,
                Player = gameEvent.Player,
                Target = gameEvent.Target,
                PlayCards = gameEvent.PlayCards,
                GameState = gameEvent.GameState,
                DrawCard = drawCard,
            };
        }

        public IEnumerable<Player> GetActivePlayers()
        {
            return Players.Where(x => x.IsGameOver());
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
        public int EventIndex { get; set; }
        public GameEventType Type { get; set; }
        public PlayerInfo Player { get; set; }
        public PlayerInfo Target { get; set; }
        public Card[] PlayCards { get; set; }
        public Card DrawCard { get; set; }
        public string ActionType { get; set; }
        public PublicGameState GameState { get; set; }

        public string FormatShortMessage(Guid playerId)
        {
            switch (Type)
            {
                case GameEventType.NewGameStarted:
                    return "New game started";
                case GameEventType.GameOver:
                    return Player.PlayerName + " is game over";
                case GameEventType.GameWon:
                    return Player.PlayerName + " wins";
                case GameEventType.CardsPlayed:
                    var cardsPlayedMessage = Player.PlayerName + " plays "+string.Join(", ",PlayCards.Select(x => x.Name));
                    if (Target != null) cardsPlayedMessage += " against " + Target.PlayerName;
                    return cardsPlayedMessage;
                case GameEventType.ActionExecuted:
                    var actionExecutedMessage = Player.PlayerName + " executes " + ActionType;
                    if (DrawCard != null) actionExecutedMessage += " drew " + DrawCard.Name;
                    if (Target != null) actionExecutedMessage += " from " + Target.PlayerName;
                    return actionExecutedMessage;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsMyEvent(Guid playerId)
        {
            if (Player == null) return false;
            return playerId == Player.Id;
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
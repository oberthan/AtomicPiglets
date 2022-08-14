using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameLogic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Bots;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static System.String;


namespace Assets.Network
{
    public class GameServer : NetworkBehaviour
    {
        private AtomicGame _game;
        private AtomicPigletRules _rules;

        public int AtomicPigletPosition;
        public TMP_Text AtomicPigletPositionText;
        public GameObject PigletPositionControl;

        GameServerTimer _gameServerTimer;

        [SyncVar(SendRate = 0.1f)]
        public float ExecutePlayedCardsTimerMax;

        [SyncVar(SendRate = 0.1f)]
        public float ExecutePlayedCardsTimer;

        private Dictionary<Guid, NetworkConnection> _playerConnectionMap;
        private Dictionary<Guid, IAtomicPigletBot> _botMap;

        private void Awake()
        {
            _gameServerTimer = new GameServerTimer(this);
        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Game server started");
        }
        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Game server stopped");
        }

        void Update()
        {
            _gameServerTimer.Update();
            if (InstanceFinder.NetworkManager.IsClient)
            {
                PlayedCardsTimeLeftSlider.value = ExecutePlayedCardsTimer;
                PlayedCardsTimeLeftSlider.maxValue = ExecutePlayedCardsTimerMax;
            }
        }


        [Server]
        public void StartGame(SyncDictionary<NetworkConnection, PlayerInfo> netPlayers, IEnumerable<IAtomicPigletBot> bots)
        {
            if (!InstanceFinder.NetworkManager.IsServer)
            {
                Debug.LogWarning("Only servers can start a new game");
                return;
            }
            _playerConnectionMap = netPlayers.ToDictionary(x => x.Value.Id, x => x.Key);
            _botMap = bots.ToDictionary(x => x.PlayerInfo.Id);

            var playerInfos = netPlayers.Values.Concat(_botMap.Values.Select(x => x.PlayerInfo)).Select(PlayerFromPlayerInfo);
            StartNewGame(playerInfos);
        }

        private void StartNewGame(IEnumerable<Player> players)
        {
            _game = GameFactory.CreateExplodingKittensLikeGame(players);
            _game.PlayTimer = _gameServerTimer;
            _rules = new AtomicPigletRules(_game);

            Debug.Log(
                $"Starting new game with {_playerConnectionMap.Count} connected players. Players in game: {_game.Players.Count} with {_game.Players.Sum(x => x.Hand.Count)} cards dealt");

            foreach (var playerConnection in _playerConnectionMap)
            {
                Debug.Log(
                    $"Connection for player {playerConnection.Key} with client id {playerConnection.Value.ClientId} is {playerConnection.Value.IsValid}");
            }

            UpdateClients();
        }

        private Player PlayerFromPlayerInfo(PlayerInfo playerInfo)
        {
            return new Player(playerInfo.PlayerName, playerInfo.Id);
        }

        public void UpdateClients()
        {
            var publicState = PublicGameState.FromAtomicGame(_game);
            foreach (var player in _game.Players)
            {
                var playerState = PlayerGameState.FromAtomicGame(player, _rules);

                if (_playerConnectionMap.TryGetValue(player.Id, out var networkConnection))
                {
                    var connection = _playerConnectionMap[player.Id];
                    ClientUpdateGameState(connection, playerState, publicState);
                }

                if (_botMap.TryGetValue(player.Id, out var bot))
                {
                    var action = bot.GetAction(_rules, playerState, publicState);
                    PlayAction(action);
                }
            }
        }

        public TMP_Text PlayerHandText;
        public TMP_Text FutureCardsText;
        public TMP_Text PlayerTurnsLeft;
        public TMP_Text CurrentPlayerText;
        public TMP_Text AllPlayersText;

        private int _cardsLeft;
        public TMP_Text CardsLeft;

        public TMP_Text PlayedCardsText;
        public TMP_Text MessageText;

        public Slider PlayedCardsTimeLeftSlider;



        [TargetRpc]
        public void ClientUpdateGameState(NetworkConnection conn, PlayerGameState playerState, PublicGameState publicState)
        {
//            Debug.Log($"Client {playerState.PlayerInfo.PlayerName} game state hand: " + playerState.Hand);
//            Debug.Log($"Client update game state owner: {IsOwner}");

            var actionList = DeserializeActionListJson(playerState.ActionListJson);
            LegalActionsButtonList(actionList);

            var canDefuse = actionList.Any(x => x is DefuseAction);
            //var pigletPos = GameObject.Find( "PigletPos");
            //if (pigletPos != null)
            PigletPositionControl.SetActive(canDefuse);

            PlayerHandText.text =  FormatPlayerHand(playerState);
            FutureCardsText.text = Join("\n", playerState.FutureCards);

            PlayedCardsText.text =  Join("\n", publicState.PlayPile);
            PlayerTurnsLeft.text = publicState.TurnsLeft.ToString();
            CurrentPlayerText.text = publicState.CurrentPlayer.PlayerName;
            var allPlayerNames = Join("\n", publicState.AllPlayers.Select(FormatOtherPlayer));
            AllPlayersText.text = allPlayerNames;
            _cardsLeft = publicState.DeckCardsLeft;
            CardsLeft.text = publicState.DeckCardsLeft.ToString();

            MessageText.text = publicState.PublicMessage;

            Debug.Log("Actions: "+Join("\n", actionList.Select(x => x.FormatShort())));

            var deck = GameObject.Find("Deck");
            var deckScript = deck.GetComponent<CardDeckScript>();
            deckScript.SetCardCount(_cardsLeft);
        }

        private static string FormatPlayerHand(PlayerGameState playerState)
        {
            var hand = playerState.Hand;
            var highlightId = hand.HighlightedCard?.Id ?? -1;
            return Join("\n", GameHelper.OrderByPriority(hand).Select(x => x.Id == highlightId ? $"** {x} **" : x.ToString()));
        }

        private static string FormatOtherPlayer(PlayerInfo playerInfo)
        {
            if (playerInfo.IsGameOver) return playerInfo.PlayerName + " (out)";  /// Skull: " \U0001f480";
            return playerInfo.PlayerName + " " + playerInfo.CardsLeft;
        }

        public void PlayAction(IGameAction action)
        {
            if (action is NoAction) return;
            if (action is ICardAction cardAction)
            {
                Debug.Log("Playing action for player " + cardAction.PlayerId);
            }

            if (action is DefuseAction defuseAction)
            {
                if (AtomicPigletPosition > _cardsLeft)
                    AtomicPigletPosition = _cardsLeft;

                defuseAction.AtomicPositionFromTop = AtomicPigletPosition;
            }

            var actionJson = SerializeGameActionJson(action);
            ServerPlayAction(actionJson);
        }

        public void AtomicPigletPositionPlus()
        {
            AtomicPigletPosition++;
            UpdateAtomicPigletPositionText();
        }

        public void AtomicPigletPositionMinus()
        {
            AtomicPigletPosition--;
            UpdateAtomicPigletPositionText();
        }

        private void UpdateAtomicPigletPositionText()
        {
            if (AtomicPigletPosition < 0) AtomicPigletPosition = 0;
            if (AtomicPigletPosition > _cardsLeft) AtomicPigletPosition = _cardsLeft;
            AtomicPigletPositionText.text = AtomicPigletPosition.ToString();
        }

        [ServerRpc(RequireOwnership = false)]
        public void ServerPlayAction(string actionJson)
        {
            if (!InstanceFinder.NetworkManager.IsServer)
            {
                Debug.LogWarning("Only servers can play actions");
                return;
            }
            var action = DeserializeGameActionJson(actionJson);

            if (action is WinGameAction)
            {
                StartNewGame(_game.Players);
            }
            else
            {
                _game.PlayAction(action);
            }

            UpdateClients();
        }

        public static List<IGameAction> DeserializeActionListJson(string actionListJson)
        {
            return JsonConvert.DeserializeObject<List<IGameAction>>(actionListJson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }
        public static string SerializeActionListJson(List<IGameAction> actionList)
        {
            return JsonConvert.SerializeObject(actionList, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });
        }
        public static IGameAction DeserializeGameActionJson(string actionJson)
        {
            return JsonConvert.DeserializeObject<IGameAction>(actionJson, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
            });
        }
        public static string SerializeGameActionJson(IGameAction gameAction)
        {
            return JsonConvert.SerializeObject(gameAction, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
            });
        }

        [SerializeField] Transform LegalActionsList;
        [SerializeField] GameObject ButtonPrefab;         
        public void LegalActionsButtonList(List<IGameAction> availableActions)
        {
            Debug.Log($"Action owner: {IsOwner}");
            foreach (Transform child in LegalActionsList.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var action in availableActions)
            {
                if (action is NoAction) continue;

                var theAction = action;
                GameObject button = Instantiate(ButtonPrefab);
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                textComponent.text = action.FormatShort();
                Debug.Log($"Creating a button for the action: {theAction}");

                button.transform.SetParent(LegalActionsList.transform, false);
                button.transform.rotation = new Quaternion(0, 0, 0, 0);
                var buttonAction = action;
                button.GetComponent<Button>().onClick.AddListener(
                    () => { ActionPressed(buttonAction); }
                    );
               
            }
        }

        private void ActionPressed(IGameAction action)
        {
            if (action is NoAction) return;
            if (action is GameOverAction) return;
            Debug.Log(action.FormatShort() + " was chosen.");
            Debug.Log($"Action pressed owner: {IsOwner}");
            PlayAction(action);
        }
    }

    public class GameServerTimer : IPlayTimer
    {
        private readonly GameServer _gameServer;
        private float _elapseTime;
        private float _startTime;

        public GameServerTimer(GameServer gameServer)
        {
            _gameServer = gameServer;
        }

        public void Update()
        {
            if (_elapseTime == 0) return;

            var time = Time.time;
            var timeLeft = _elapseTime - time;
            if (timeLeft < 0) timeLeft = 0;
            _gameServer.ExecutePlayedCardsTimer = timeLeft;
            if (timeLeft <= 0)
            {
                Debug.Log("Game timer elapsed");
                OnTimerElapsed();
                _elapseTime = 0;
                _gameServer.UpdateClients();
            }
        }

        public void Start(float delay)
        {
            Debug.Log($"Game timer started with {delay} delay");
            _startTime = Time.time;
            _elapseTime = _startTime + delay;
            _gameServer.ExecutePlayedCardsTimerMax = delay;
            _gameServer.UpdateClients();
        }

        public event EventHandler TimerElapsed;

        private void OnTimerElapsed()
        {
            TimerElapsed?.Invoke(this, EventArgs.Empty);
        }
    }

    public class PlayerGameState
    {
        // Player info and hidden hand
        public PlayerInfo PlayerInfo;
        public CardCollection Hand;
        public CardCollection FutureCards;
        public string ActionListJson;

        internal static PlayerGameState FromAtomicGame(Player player, AtomicPigletRules rules)
        {
            var actionList = rules.GetLegalActionsForPlayer(player).ToList();

            return new PlayerGameState
            {
                PlayerInfo = PlayerInfoFromPlayer(player),
                Hand = player.Hand,
                FutureCards = player.FutureCards,
                ActionListJson = GameServer.SerializeActionListJson(actionList)
            };
        }


        private static string[] GetLegalActions(Player player, AtomicPigletRules rules)
        {
            var actions = rules.GetLegalActionsForPlayer(player);
            return actions.Select(x => x.FormatShort()).ToArray();
        }

        public static PlayerInfo PlayerInfoFromPlayer(Player player)
        {
            return new PlayerInfo { Id = player.Id, PlayerName = player.Name, IsReady = true, CardsLeft = player.Hand.Count, IsGameOver = player.IsGameOver()};
        }

        public static string[] SerializeCardCollection(CardCollection cards)
        {
            return cards.All.Select(x => x.GetType().Name).ToArray();
        }

    }

    public class PublicGameState
    {
        public PlayerInfo CurrentPlayer;
        public PlayerInfo[] AllPlayers;
        public CardCollection PlayPile;
        public CardCollection DiscardPile;
        public int DeckCardsLeft;
        public int TurnsLeft;
        public string PublicMessage;

        public static PublicGameState FromAtomicGame(AtomicGame game)
        {
            return new PublicGameState
            {
                CurrentPlayer = PlayerGameState.PlayerInfoFromPlayer(game.CurrentPlayer),
                AllPlayers = game.Players.Select(PlayerGameState.PlayerInfoFromPlayer).ToArray(),
                PlayPile = game.PlayPile,
                DiscardPile = game.DiscardPile,
                DeckCardsLeft = game.Deck.Count,
                TurnsLeft = game.PlayerTurns,
                PublicMessage = game.PublicMessage
            };
        }

    }
}

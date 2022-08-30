using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GameLogic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Bots;
using Assets.Dto;
using FishNet.Managing.Scened;
using FishNet.Transporting;
using TMPro;
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

        private Dictionary<Guid, PlayerConnection> _playerConnectionMap;
        private Dictionary<Guid, IAtomicPigletBot> _botMap;

        private class PlayerConnection
        {
            public Guid PlayerId;
            public NetworkConnection Connection;
            public int LastGameEventIndex = -1;
            public int Wins;
            public int ClientId => Connection.ClientId;
        }

        // ReSharper disable once UnusedMember.Local
        private void Awake()
        {
            _gameServerTimer = new GameServerTimer(this);
        }


        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Game server started");
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
            NetworkManager.ServerManager.OnServerConnectionState += ServerManagerOnOnServerConnectionState;
            NetworkManager.ClientManager.OnClientConnectionState += ClientManagerOnOnClientConnectionState;
        }

        private void ClientManagerOnOnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                Debug.LogWarning("Client disconnected");
            }
        }

        private void ServerManagerOnOnServerConnectionState(ServerConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                Debug.LogWarning("Stopped game host");
            }
        }

        private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args)
        {
            if (args.ConnectionState == RemoteConnectionState.Stopped)
            {
                var playerId = _playerConnectionMap.First(x => x.Value.ClientId == conn.ClientId).Key;
                var player = _game.GetPlayer(playerId);
                Debug.Log($"Player {player.Name} with conn id: {conn.ClientId} disconnected");
                player.Name += "(no net)";
            }
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Game server stopped");
            NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }


        // ReSharper disable once UnusedMember.Local
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
            _playerConnectionMap = netPlayers.ToDictionary(x => x.Value.Id,  x => new PlayerConnection
            {
                PlayerId = x.Value.Id,
                Connection = x.Key
            });

            _botMap = bots.ToDictionary(x => x.PlayerInfo.Id);

            var playerInfos = netPlayers.Values.Concat(_botMap.Values.Select(x => x.PlayerInfo)).Select(PlayerFromPlayerInfo);
            StartNewGame(playerInfos);
            UpdateClients();
        }

        [Server]
        private void StartNewGame(IEnumerable<Player> players)
        {
            foreach (var playerConnection in _playerConnectionMap.Values)
            {
                playerConnection.LastGameEventIndex = -1;
            }
            _game = GameFactory.CreateExplodingKittensLikeGame(players);
            _game.PlayTimer = _gameServerTimer;
            _rules = new AtomicPigletRules(_game);

            Debug.Log(
                $"Starting new game with {_playerConnectionMap.Count} connected players. Total players in game: {_game.Players.Count} with {_game.Players.Sum(x => x.Hand.Count)} cards dealt");

            foreach (var playerConnection in _playerConnectionMap)
            {
                Debug.Log(
                    $"Connection for player {playerConnection.Key} with client id {playerConnection.Value.ClientId} is {playerConnection.Value.Connection.IsValid}");
            }
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

//                Debug.Log($"Server side game event for {player}: " + gameEvent.FormatShortMessage(playerState.PlayerInfo.Id));

                if (_playerConnectionMap.TryGetValue(player.Id, out var playerConnection))
                {
                    var gameEvents = _game.GetLastGameEvents(player, playerConnection.LastGameEventIndex);
                    foreach (var gameEvent in gameEvents)
                    {
                        ClientUpdateGameState(playerConnection.Connection, gameEvent, playerState, publicState);
                        playerConnection.LastGameEventIndex = gameEvent.EventIndex;
                    }
                }

                if (_botMap.TryGetValue(player.Id, out var bot))
                {
                    var action = bot.GetAction(_rules, playerState, publicState);
                    StartCoroutine(DelayedPlayAction(action));
                }
            }
        }

        private readonly HashSet<Guid> _isPlayingAction = new();

        IEnumerator DelayedPlayAction(IGameAction action)
        {
            if (_isPlayingAction.Contains(action.PlayerId)) yield break;
            if (action is NoAction) yield break;
            if (action is GameOverAction) yield break;
            _isPlayingAction.Add(action.PlayerId);
            Debug.Log($"{_game.GetPlayer(action.PlayerId)} waiting to play {action}.");
            yield return new WaitForSeconds(2);
            Debug.Log($"{_game.GetPlayer(action.PlayerId)} plays {action}. {FormatCardAction(action)}");
            PlayAction(action);
            _isPlayingAction.Remove(action.PlayerId);
        }

        private string FormatCardAction(IGameAction action)
        {
            if (action is ICardAction cardAction)
            {
                return Join(", ", cardAction.Cards.Select(x => $"{x.Name}({x.Id})"));
            }

            return "";
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
        public void ClientUpdateGameState(NetworkConnection conn, GameEvent gameEvent, PlayerGameState playerState, PublicGameState publicState)
        {
            if (gameEvent.Type == GameEventType.NewGameStarted || (gameEvent.Type == GameEventType.ActionExecuted && gameEvent.ActionType == nameof(ShuffleAction)))
            {
                StartCoroutine(GetDeckScript().InitializeNewDeck(gameEvent.GameState.DeckCardsLeft));
            }

            var myPlayerId = playerState.PlayerInfo.Id;

            var isMyGameEvent = gameEvent.IsMyEvent(myPlayerId);
            if (isMyGameEvent && gameEvent.ActionType == nameof(DrawFromDeckAction))
                AnimateDrawMyCard(gameEvent.GameState.DeckCardsLeft);
            else if (!isMyGameEvent && gameEvent.ActionType == nameof(DrawFromDeckAction))
                AnimateOpponentDrawCard(gameEvent.GameState.DeckCardsLeft);

            Debug.Log($"Client side game event for {playerState.PlayerInfo.PlayerName}: " + gameEvent.FormatShortMessage(playerState.PlayerInfo.Id));
            var actionList = GameDataSerializer.DeserializeActionListJson(playerState.ActionListJson);
            LegalActionsButtonList(actionList);

            var canDefuse = actionList.Any(x => x is DefuseAction);
            PigletPositionControl.SetActive(canDefuse);

            PlayerHandText.text = FormatPlayerHand(playerState);
            FutureCardsText.text = Join("\n", playerState.FutureCards);

            PlayedCardsText.text = Join("\n", publicState.PlayPile);
            PlayerTurnsLeft.text = publicState.TurnsLeft.ToString();
            CurrentPlayerText.text = publicState.CurrentPlayer.PlayerName;
            var allPlayerNames = Join("\n", publicState.AllPlayers.Select(FormatOtherPlayer));
            AllPlayersText.text = allPlayerNames;
            _cardsLeft = publicState.DeckCardsLeft;
            CardsLeft.text = publicState.DeckCardsLeft.ToString();

            MessageText.text = publicState.PublicMessage;


            if (gameEvent.Type == GameEventType.GameOver)
            {
                if (!playerState.PlayerInfo.IsGameOver && publicState.AllPlayers
                        .Where(x => x.Id != playerState.PlayerInfo.Id).All(x => x.IsGameOver))
                    gameEvent = new GameEvent { Type = GameEventType.GameWon };
            }

            PlaySoundEffect(gameEvent);
        }

        private void AnimateDrawMyCard(int cardIndex)
        {
            GetDeckScript().DrawCardToBottom(cardIndex);
        }
        private void AnimateOpponentDrawCard(int cardIndex)
        {
            GetDeckScript().DrawCardToTop(cardIndex);
        }

        private static CardDeckScript GetDeckScript()
        {
            var deck = GameObject.Find("Deck");
            var deckScript = deck.GetComponent<CardDeckScript>();
            return deckScript;
        }

        private void PlaySoundEffect(GameEvent gameEvent)
        {
            var gameAudio = GameObject.Find("GameAudio");
            var audioScript = gameAudio.GetComponent<GameAudioScript>();
            audioScript.PlayGameEvent(gameEvent);
        }

        private static string FormatPlayerHand(PlayerGameState playerState)
        {
            var hand = playerState.Hand;
            var highlightId = hand.HighlightedCard?.Id ?? -1;
            return Join("\n", GameHelper.OrderByPriority(hand).Select(x => x.Id == highlightId ? $"** {x} **" : x.ToString()));
        }

        private static string FormatOtherPlayer(PlayerInfo playerInfo)
        {
            if (playerInfo.IsGameOver) return playerInfo.PlayerName + " (out)";  // Skull: " \U0001f480";
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

            var actionJson = GameDataSerializer.SerializeGameActionJson(action);
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
            var action = GameDataSerializer.DeserializeGameActionJson(actionJson);

            if (action is WinGameAction)
                StartNewGame(_game.Players);
            else
                _game.PlayAction(action);

            UpdateClients();
        }

        [SerializeField] Transform LegalActionsList;
        [SerializeField] GameObject ButtonPrefab;

        public void LegalActionsButtonList(List<IGameAction> availableActions)
        {
            foreach (Transform child in LegalActionsList.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var action in availableActions)
            {
                if (action is NoAction) continue;

                var button = Instantiate(ButtonPrefab);
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                textComponent.text = action.FormatShort();

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

        public void QuitToMenu()
        {
            var gameScene = new SceneLoadData("MenuScene")
            {
                ReplaceScenes = ReplaceOption.All
            };

            var networkConnections = _playerConnectionMap.Select(x => x.Value.Connection);
            var networkObjects = networkConnections.SelectMany(x => x.Objects);
            gameScene.MovedNetworkObjects = networkObjects.Where(x => !x.IsSceneObject).ToArray();

            SceneManager.LoadGlobalScenes(gameScene);
        }
    }
}

using FishNet;
using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Bots;
using Assets.Dto;
using UnityEngine;

namespace Assets.Network
{
    public class LobbyServer : NetworkBehaviour
    {
        /// <summary>
        /// Called when any player changes status.
        /// </summary>
        public static event Action<NetworkConnection, PlayerInfo> OnPlayerInfoChange;

        /// <summary>
        /// Collection of each player name for connections.
        /// </summary>
        [SyncObject]
        private readonly SyncDictionary<NetworkConnection, PlayerInfo> _playerInfos = new();

        public const int GameStartCountdownSeconds = 2;

        private const int MinimumNumberOfPlayers = 1;

        [SyncVar(SendRate = 0.1f)]
        public float GameStartTimer = GameStartCountdownSeconds;

        private bool _gameStartTimerIsRunning;
        private float _gameStartTimerStarted;
        private bool _isGameRunning;

        /// <summary>
        /// Singleton instance of this object.
        /// </summary>
        private static LobbyServer _instance;

        private int _botsCount;

        private void Awake()
        {
            Debug.Log("Lobby server awake");
            _instance = this;
            var hostGameObject = GameObject.Find("HostGame");
            if (hostGameObject != null)
            {
                var hostScript = hostGameObject.GetComponent<HostScript>();
                _botsCount = (int)hostScript.Bots.value;
            }

            _playerInfos.OnChange += _playerInfos_OnChange;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Lobby server started");
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
            SceneManager.OnClientPresenceChangeEnd += SceneManager_OnClientPresenceChangeEnd;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Lobby server stopped");
            NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        }

        internal static float GetStartTimer()
        {
            return _instance.GameStartTimer;
        }

        /// <summary>
        /// Called when a remote client connection state changes.
        /// </summary>
        private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs state)
        {
            if (state.ConnectionState == RemoteConnectionState.Started)
            {
                Debug.Log($"{conn.ClientId} remote connection started");
                conn.OnLoadedStartScenes += ConnOnOnLoadedStartScenes;
            }
            else // Started
            {
                _playerInfos.Remove(conn);
                conn.OnLoadedStartScenes -= ConnOnOnLoadedStartScenes;
            }
            RestartGameStartTimer();
        }

        private void ConnOnOnLoadedStartScenes(NetworkConnection conn, bool isServer)
        {
            Debug.Log($"Connection {conn.ClientId} loaded start scenes {FormatConnectionScenes(conn)}. IsServer: {isServer}");
        }

        private static string FormatConnectionScenes(NetworkConnection conn)
        {
            return string.Join(", ", conn.Scenes.Select(x => x.name));
        }

        private void RestartGameStartTimer()
        {
            GameStartTimer = GameStartCountdownSeconds;
            if (_playerInfos.Any() && _playerInfos.Values.All(x => x.IsReady))
            {
                _gameStartTimerIsRunning = true;
                _gameStartTimerStarted = Time.time;
            }
            else
            {
                _gameStartTimerIsRunning = false;
            }
        }

        void Update()
        {
            if (_gameStartTimerIsRunning && !_isGameRunning)
            {
                var deltaTime = Time.time - _gameStartTimerStarted;
                var startTimer = GameStartCountdownSeconds - deltaTime;
                if (startTimer < 0) startTimer = 0;
                GameStartTimer = startTimer;
                if (startTimer == 0)
                {
                    _gameStartTimerIsRunning = false;
                    StartServerGame();
                }
            }
        }

        private void StartServerGame()
        {
            if (!InstanceFinder.NetworkManager.IsServer) return;

            _isGameRunning = true;
            Debug.Log("Loading game scene");
            var gameScene = new SceneLoadData("GameScene")
            {
                ReplaceScenes = ReplaceOption.All
            };

            foreach (var playerInfo in _playerInfos)
            {
                var networkObjectsText = new StringBuilder();
                var connection = playerInfo.Key;
                networkObjectsText.AppendLine($"Player {playerInfo.Value.PlayerName} objects:");
                foreach (var obj in connection.Objects.Where(x => !x.IsSceneObject))
                {
                    networkObjectsText.AppendLine(obj.name);
                }
                Debug.Log(networkObjectsText.ToString());
            }

            var gameSceneMovedNetworkObjects = _playerInfos.Select(x => x.Key).SelectMany(x => x.Objects).Where(x => !x.IsSceneObject).ToArray();
            gameScene.MovedNetworkObjects = gameSceneMovedNetworkObjects;

            SceneManager.LoadGlobalScenes(gameScene);
        }

        private void SceneManager_OnClientPresenceChangeEnd(ClientPresenceChangeEventArgs obj)
        {
            Debug.Log($"Client presence changed for {obj.Connection.ClientId} for scene {obj.Scene.name}");
            Debug.Log($"All player scenes {string.Join(", ", _playerInfos.Keys.Select(x => $"{x.ClientId}:{FormatConnectionScenes(x)}"))}");
            if (_playerInfos.All(x => x.Key.Scenes.Any(y => y.name == "GameScene")))
            {
                var gameNetworking = GameObject.Find("GameNetworking");
                if (gameNetworking != null)
                {
                    var gameServer = gameNetworking.GetComponent<GameServer>();
                    Debug.Log("Making bots");
                    var bots = MakeBots(HostScript.BotsCount);
                    gameServer.StartGame(_playerInfos, bots);
                }
            }
        }

        private string FormatPlayers()
        {
            return string.Join(", ", _playerInfos.Select(x => $"{x.Value}. Conn: {x.Key.ClientId}"));
        }

        private IEnumerable<IAtomicPigletBot> MakeBots(int count)
        {
            for (int i = 0; i < count; i++)
            {
                IAtomicPigletBot bot = i % 2 == 0 ? new HorseBot() : new MonkeyBot();
                if (count > 2) bot.PlayerInfo.PlayerName += " " + (1 + i / 2);
                yield return bot;
            }
        }

        /// <summary>
        /// Optional callback when the playerNames collection changes.
        /// </summary>
        private void _playerInfos_OnChange(SyncDictionaryOperation op, NetworkConnection key, PlayerInfo value, bool asServer)
        {
//            if (op == SyncDictionaryOperation.Add || op == SyncDictionaryOperation.Set)
            OnPlayerInfoChange?.Invoke(key, value);
            RestartGameStartTimer();
        }

        /// <summary> 
        /// Gets a player name. Works on server or client.
        /// </summary>
        public static PlayerInfo GetPlayerInfo(NetworkConnection conn)
        {
            if (_instance._playerInfos.TryGetValue(conn, out PlayerInfo playerInfo))
                return playerInfo;
            else
                return null;
        }

        /// <summary>
        /// Lets clients set their name.
        /// </summary>

        [Client]
        public static void UpdatePlayerInfo(PlayerInfo playerInfo)
        {
            Debug.Log($"Client update player info for {playerInfo}");
            _instance.ServerUpdatePlayerInfo(playerInfo);
        }

        [Client]
        public static PlayerInfo[] GetPlayerInfos()
        {
            return _instance._playerInfos.Values.ToArray();
        }

        /// <summary>
        /// Sets name on server.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        private void ServerUpdatePlayerInfo(PlayerInfo playerInfo, NetworkConnection sender = null)
        {
            Debug.Log($"Server update player info for {playerInfo}. Conn: {sender?.ClientId}");
            if (sender == null) return;
            playerInfo.PlayerName = playerInfo.PlayerName.Substring(0, Math.Min(playerInfo.PlayerName.Length, 15));
            _playerInfos[sender] = playerInfo;
        }
    }
}

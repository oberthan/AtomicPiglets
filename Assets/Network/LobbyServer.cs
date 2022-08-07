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
using UnityEngine;
using UnityEngine.SceneManagement;

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
        private readonly SyncDictionary<NetworkConnection, PlayerInfo> _playerInfos = new SyncDictionary<NetworkConnection, PlayerInfo>();

        public const int GameStartCountdownSeconds = 2;

        private const int MinimumNumberOfPlayers = 1;

        [SyncVar(SendRate = 0.1f)]
        public float GameStartTimer = GameStartCountdownSeconds;

        private bool gameStartTimerIsRunning;
        private float gameStartTimerStarted;
        private bool isGameRunning;

        /// <summary>
        /// Singleton instance of this object.
        /// </summary>
        private static LobbyServer _instance;

        private void Awake()
        {
            Debug.Log("Lobby server awake");
            _instance = this;
            _playerInfos.OnChange += _playerInfos_OnChange;
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
            Debug.Log("Lobby server started");
            NetworkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
        }

        public override void OnStopServer()
        {
            base.OnStopServer();
            Debug.Log("Lobby server stopped");
            base.NetworkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
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
            if (state.ConnectionState != RemoteConnectionState.Started)
                _playerInfos.Remove(conn);
            RestartGameStartTimer();
        }

        private void RestartGameStartTimer()
        {
            GameStartTimer = GameStartCountdownSeconds;
            if (_playerInfos.Any() && _playerInfos.Values.All(x => x.IsReady))
            {
                gameStartTimerIsRunning = true;
                gameStartTimerStarted = Time.time;
            }
            else
            {
                gameStartTimerIsRunning = false;
            }
        }

        void Update()
        {
            if (gameStartTimerIsRunning && !isGameRunning)
            {
                var deltaTime = Time.time - gameStartTimerStarted;
                var startTimer = GameStartCountdownSeconds - deltaTime;
                if (startTimer < 0) startTimer = 0;
                GameStartTimer = startTimer;
                if (startTimer == 0)
                {
                    gameStartTimerIsRunning = false;
                    StartServerGame();
                }
            }
        }

        private void StartServerGame()
        {
            if (!InstanceFinder.NetworkManager.IsServer) return;

            isGameRunning = true;
            Debug.Log("Loading game scene");
            var gameScene = new SceneLoadData("GameScene");
            gameScene.ReplaceScenes = ReplaceOption.All;

            foreach (var playerInfo in _playerInfos)
            {
                var networkObjectsText = new StringBuilder();
                var connection = playerInfo.Key;
                networkObjectsText.AppendLine($"Player {playerInfo.Value.PlayerName} objects:");
                foreach (var obj in connection.Objects)
                {
                    networkObjectsText.AppendLine(obj.name);
                }
                Debug.Log(networkObjectsText.ToString());
            }

            gameScene.MovedNetworkObjects = _playerInfos.Select(x => x.Key).SelectMany(x => x.Objects).ToArray();
            //foreach (NetworkConnection item in InstanceFinder.ServerManager.Clients.Values)
            //{
            //    foreach (NetworkObject nob in item.Objects)
            //    {
            //        movedObjects.Add(nob);
            //    }
            //}

            

//            gameScene.MovedNetworkObjects = movedObjects;

            SceneManager.LoadGlobalScenes(gameScene);
            SceneManager.OnActiveSceneSet += SceneManager_OnActiveSceneSet;


            //Debug.Log("Unloading menu scene");
            //var menuScene = new FishNet.Managing.Scened.SceneUnloadData("MenuScene");
            //SceneManager.UnloadGlobalScenes(menuScene);


            //var gs1 = SceneManager.GetComponent<GameServer>();
            //if (gs1 == null) Debug.Log("Did NOT found GameServer with scene manager");

            //Debug.Log("Start client game");
            //var gameNetworking = GameObject.Find("GameNetworking");
            //if (gameNetworking == null)
            //{
            //    Debug.Log("Did not find game server");
            //}
            //else
            //{
            //    Debug.Log("Found game server");
            //    var gameServer = gameNetworking.GetComponent<GameServer>();
            //    gameServer.StartGame(_playerInfos);
            //}


            StartClientGame();
        }

        private void SceneManager_OnActiveSceneSet()
        {
            var gameNetworking = GameObject.Find("GameNetworking");
            if (gameNetworking == null)
            {
                Debug.Log("Did not find game server in OnActiveSceneSet");
            }
            else
            {
                Debug.Log("Did find game server in OnActiveSceneSet");
                var gameServer = gameNetworking.GetComponent<GameServer>();
                gameServer.StartGame(_playerInfos);
            }
        }

        [ObserversRpc]
        public void StartClientGame()
        {

            
            //var game = Resources.FindObjectsOfTypeAll<GameObject>().Single(x => x.name == "Game");
            //game.SetActive(true);
            //var menu = Resources.FindObjectsOfTypeAll<GameObject>().Single(x => x.name == "Menu");
            //menu.SetActive(false);
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
        /// <param name="name"></param>
        /// <param name="sender"></param>
        [ServerRpc(RequireOwnership = false)]
        private void ServerUpdatePlayerInfo(PlayerInfo playerInfo, NetworkConnection sender = null)
        {
            _playerInfos[sender] = playerInfo;
        }
    }

    public class PlayerInfo
    {
        public Guid Id;
        public string PlayerName;
        public bool IsReady;
        
        // In game only
        public int CardsLeft;
    }
}

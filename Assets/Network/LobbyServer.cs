﻿using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Linq;
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
        private readonly SyncDictionary<NetworkConnection, PlayerInfo> _playerInfos = new SyncDictionary<NetworkConnection, PlayerInfo>();

        public const int GameStartCountdownSeconds = 5;

        private const int MinimumNumberOfPlayers = 1;

        [SyncVar(SendRate = 1)]
        public int GameStartTimer = GameStartCountdownSeconds;

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

        internal static int GetStartTimer()
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
                var startTimer = GameStartCountdownSeconds - (int)deltaTime;
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
            isGameRunning = true;
            StartClientGame();
        }

        [ObserversRpc]
        public void StartClientGame()
        {
            var game = Resources.FindObjectsOfTypeAll<GameObject>().Single(x => x.name == "Game");
            game.SetActive(true);
            var menu = Resources.FindObjectsOfTypeAll<GameObject>().Single(x => x.name == "Menu");
            menu.SetActive(false);
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
        public string PlayerName;
        public bool IsReady;
    }
}

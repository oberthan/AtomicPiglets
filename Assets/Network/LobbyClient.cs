using FishNet.Connection;
using FishNet.Object;
using System;
using System.Linq;
using Assets.Dto;
using FishNet;
using FishNet.Discovery;
using FishNet.Transporting;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Network
{
    public class LobbyClient : NetworkBehaviour
    {
        public PlayerInfo Player = new() { PlayerName = "player", Id = Guid.NewGuid() };       

        public Toggle IsReady;
        public TMP_Text AllPlayers;
        public Slider GameStartCountdownSlider;
        public TMP_Text StatusText;

        private void Awake()
        {
            Debug.Log("Lobby client awake");
            IsReady.onValueChanged.AddListener((isReady) =>
            {
                Debug.Log("Is ready changed to "+isReady);
                Player.IsReady = isReady;
                ServerUpdatePlayerInfo();
            });

        }

        private void Start()
        {
            GameStartCountdownSlider.minValue = 0;
            GameStartCountdownSlider.maxValue = LobbyServer.GameStartCountdownSeconds;
            if (InstanceFinder.ServerManager.Started)
            {
                var statusText = "Hosting";
                var networkDiscovery = FindObjectOfType<NetworkDiscovery>();
                if (networkDiscovery != null)
                {
                    statusText += "\n" + (networkDiscovery.IsAdvertising ? "public" : "private");
                }

                StatusText.text = statusText;
            }
            else
            {
                var clientManager = InstanceFinder.ClientManager;
                if (clientManager.Started)
                {
                    var address = clientManager.NetworkManager.TransportManager.Transport.GetClientAddress();
                    var hostName = address;
                    var statusText = "Connected to\n" + hostName;
                    StatusText.text = statusText;
                    clientManager.OnClientConnectionState += ClientManagerOnOnClientConnectionState;
                }
            }
        }

        private void Stop()
        {
            InstanceFinder.ClientManager.OnClientConnectionState -= ClientManagerOnOnClientConnectionState;

        }

        private void ClientManagerOnOnClientConnectionState(ClientConnectionStateArgs args)
        {
            if (args.ConnectionState == LocalConnectionState.Stopped)
            {
                StatusText.text = "Disconnected";
            }
        }

        private void ServerUpdatePlayerInfo()
        {
            var playerGameObject = GameObject.Find("Player");
            var playerNameInfoComponent = playerGameObject.GetComponent<PlayerNameInfo>();
            var playerName = playerNameInfoComponent.PlayerName;
            Player.PlayerName = playerName;
            LobbyServer.UpdatePlayerInfo(Player);
        }

        public override void OnStartClient()
        {
            base.OnStartClient();
            Debug.Log("Lobby client started");
            LobbyServer.OnPlayerInfoChange += LobbyServer_OnPlayerInfoChange;
            GiveOwnership(this.LocalConnection);
        }

        public override void OnOwnershipClient(NetworkConnection prevOwner)
        {
            base.OnOwnershipClient(prevOwner);
            //            Debug.Log($"Is owner: {IsOwner}");
            ServerUpdatePlayerInfo();
            UpdatePlayerList();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            Debug.Log("Lobby client Stopped");
            LobbyServer.OnPlayerInfoChange -= LobbyServer_OnPlayerInfoChange;
            UpdatePlayerList();
        }

        public void Update()
        {
            GameStartCountdownSlider.value = LobbyServer.GetStartTimer();
        }

        private void LobbyServer_OnPlayerInfoChange(NetworkConnection conn, PlayerInfo arg2)
        {
            UpdatePlayerList();
        }

        private void UpdatePlayerList()
        {
            var text = "Not connected";
            if (Owner.IsValid)
            {
                var playerInfos = LobbyServer.GetPlayerInfos();
                if (playerInfos == null)
                {
                    text = "Not connected";
                }
                else if (!playerInfos.Any())
                {
                    text = "No players";
                }
                else
                {
                    text = string.Join("\n", playerInfos.Select(x => $"{x.PlayerName}" + (x.IsReady ? " (ready)" : "")));
                }
            }
            AllPlayers.text = text;
        }

        public void Leave()
        {
            InstanceFinder.ServerManager.StopConnection(true);
            InstanceFinder.ClientManager.StopConnection();
        }

    }
}

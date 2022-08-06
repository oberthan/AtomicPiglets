using FishNet.Connection;
using FishNet.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Network
{
    public class LobbyClient : NetworkBehaviour
    {
        public PlayerInfo Player = new PlayerInfo { PlayerName = "Player" };

        public Toggle IsReady;
        public TMP_Text AllPlayers;

        private void Awake()
        {
            Debug.Log("Lobby client awake");
            IsReady.onValueChanged.AddListener((isReady) => {
                Player.IsReady = isReady;
                LobbyServer.UpdatePlayerInfo(Player);
            });
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
            Debug.Log($"Is owner: {IsOwner}");
            LobbyServer.UpdatePlayerInfo(Player);
            UpdatePlayerList();
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            Debug.Log("Lobby client Stopped");
            LobbyServer.OnPlayerInfoChange -= LobbyServer_OnPlayerInfoChange;
            UpdatePlayerList();
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
                if (!playerInfos.Any())
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


    }
}

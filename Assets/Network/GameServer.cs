using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Assets.Network
{
    public class GameServer : NetworkBehaviour
    {
        private AtomicGame game;
        private AtomicPigletRules rules;

        private Dictionary<Guid, NetworkConnection> playerConnectionMap;

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

        [Server]
        public void StartGame(SyncDictionary<NetworkConnection, PlayerInfo> players)
        {
            if (!InstanceFinder.NetworkManager.IsServer)
            {
                Debug.LogWarning("Only servers can start a new game");
                return;
            }
            playerConnectionMap = players.ToDictionary(x => x.Value.Id, x => x.Key);

            game = GameFactory.CreateExplodingKittensLikeGame(players.Values.Select(PlayerFromPlayerInfo));
            rules = new AtomicPigletRules(game);

            Debug.Log($"Starting new game with {playerConnectionMap.Count} connected players. Players in game: {game.Players.Count} with {game.Players.Sum(x => x.Hand.Count)} cards dealt");

            foreach (var playerConnection in playerConnectionMap)
            {
                Debug.Log($"Connection for player {playerConnection.Key} with client id {playerConnection.Value.ClientId} is {playerConnection.Value.IsValid}");

            }

            UpdateClients();
        }

        private Player PlayerFromPlayerInfo(PlayerInfo playerInfo)
        {
            return new Player(playerInfo.PlayerName, playerInfo.Id);
        }

        private void UpdateClients()
        {
            var publicState = PublicGameState.FromAtomicGame(game);
            foreach (var player in game.Players)
            {
                Debug.Log($"{player.Name} hand: " + player.FormatHand());

                var connection = playerConnectionMap[player.Id];
                PlayerGameState playerState = PlayerGameState.FromAtomicGame(player, rules);

                Debug.Log($"Server {playerState.PlayerInfo.PlayerName} game state hand: " + playerState.Hand.ToString());

                ClientUpdateGameState(connection, playerState, publicState);
            }
        }

        private PlayerGameState myPlayerGameState;
        private PublicGameState publicGameState;

  //      public TMP_Text LegalActionsText;
        public TMP_Text PlayerHandText;

        public TMP_Dropdown LegalActionsDropdown;

        [TargetRpc]
        public void ClientUpdateGameState(NetworkConnection conn, PlayerGameState playerState, PublicGameState publicState)
        {
            Debug.Log($"Client {playerState.PlayerInfo.PlayerName} game state hand: " + playerState.Hand.ToString());
            myPlayerGameState = playerState;
            publicGameState = publicState;
            LegalActionsButtonList(playerState.AvailableActions);
            PlayerHandText.text =  string.Join("\n", playerState.Hand);

            //LegalActionsDropdown.ClearOptions();
            //LegalActionsDropdown.AddOptions(playerState.AvailableActions.ToList());

        }

        [SerializeField] Transform LegalActionsList;
        [SerializeField] GameObject ButtonPrefab;         
        public void LegalActionsButtonList(string[] availableActions)
        {
            foreach(var action in availableActions)
            {
                var TheAction = action;
                GameObject button = (GameObject)Instantiate(ButtonPrefab);
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                textComponent.text = action.ToString();
                Debug.Log($"Creating a button for the action: {TheAction}");


                button.transform.SetParent(LegalActionsList);
            }
        }
    }


    public class PlayerGameState
    {
        // Player info and hidden hand
        public PlayerInfo PlayerInfo;
        public string[] Hand;
        public string[] AvailableActions;

        internal static PlayerGameState FromAtomicGame(Player player, AtomicPigletRules rules)
        {
            return new PlayerGameState
            {
                PlayerInfo = PlayerInfoFromPlayer(player),
                Hand =  SerializeCardCollection(player.Hand),
                AvailableActions = GetLegalActions(player, rules)
            };
        }

        private static string[] GetLegalActions(Player player, AtomicPigletRules rules)
        {
            var actions = rules.GetLegalActionsForPlayer(player);
            return actions.Select(x => x.FormatShort()).ToArray();
        }

        public static PlayerInfo PlayerInfoFromPlayer(Player player)
        {
            return new PlayerInfo { Id = player.Id, PlayerName = player.Name, IsReady = true, CardsLeft = player.Hand.Count };
        }

        public static string[] SerializeCardCollection(CardCollection cards)
        {
            return cards.All.Select(x => x.Name).ToArray();
        }

    }

    public class PublicGameState
    {
        public PlayerInfo CurrentPlayer;
        public PlayerInfo[] AllPlayers;
        public string[] PlayPile;
        public string[] DiscardPile;
        public int DeckCardsLeft;
        public int TurnsLeft;

        public static PublicGameState FromAtomicGame(AtomicGame game)
        {
            return new PublicGameState
            {
                CurrentPlayer = PlayerGameState.PlayerInfoFromPlayer(game.CurrentPlayer),
                AllPlayers = game.Players.Select(PlayerGameState.PlayerInfoFromPlayer).ToArray(),
                PlayPile = PlayerGameState.SerializeCardCollection(game.PlayPile),
                DiscardPile = PlayerGameState.SerializeCardCollection(game.DiscardPile),
                DeckCardsLeft = game.Deck.Count,
                TurnsLeft = game.PlayerTurns
            };
        }
    }
}

using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Network
{
    public class GameServer : NetworkBehaviour
    {
        private AtomicGame game;
        private AtomicPigletRules rules;

        private Dictionary<Guid, NetworkConnection> playerConnectionMap;

        [Server]
        public void StartGame(SyncDictionary<NetworkConnection, PlayerInfo> players)
        {
            playerConnectionMap = players.ToDictionary(x => x.Value.Id, x => x.Key);

            game = GameFactory.CreateExplodingKittensLikeGame(players.Values.Select(PlayerFromPlayerInfo));
            rules = new AtomicPigletRules(game);
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
                var connection = playerConnectionMap[player.Id];
                PlayerGameState playerState = PlayerGameState.FromAtomicGame(player, rules);
                ClientUpdateGameState(connection, playerState, publicState);
            }
        }

        private PlayerGameState myPlayerGameState;
        private PublicGameState publicGameState;

        [TargetRpc]
        public void ClientUpdateGameState(NetworkConnection conn, PlayerGameState playerState, PublicGameState publicState)
        {
            myPlayerGameState = playerState;
            publicGameState = publicState;
        }
    }

    public class PlayerGameState
    {
        // Player info and hidden hand
        public PlayerInfo PlayerInfo;
        public CardCollection Hand;
        public string[] AvailableActions;

        internal static PlayerGameState FromAtomicGame(Player player, AtomicPigletRules rules)
        {
            return new PlayerGameState
            {
                PlayerInfo = PlayerInfoFromPlayer(player),
                Hand = player.Hand,
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

    }

    public class PublicGameState
    {
        public PlayerInfo CurrentPlayer;
        public PlayerInfo[] AllPlayers;
        public CardCollection PlayPile;
        public CardCollection DiscardPile;
        public int DeckCardsLeft;
        public int TurnsLeft;

        public static PublicGameState FromAtomicGame(AtomicGame game)
        {
            return new PublicGameState
            {
                CurrentPlayer = PlayerGameState.PlayerInfoFromPlayer(game.CurrentPlayer),
                AllPlayers = game.Players.Select(PlayerGameState.PlayerInfoFromPlayer).ToArray(),
                PlayPile = game.PlayPile,
                DiscardPile = game.DiscardPile,
                DeckCardsLeft = game.Deck.Count,
                TurnsLeft = game.PlayerTurns
            };
        }
    }
}

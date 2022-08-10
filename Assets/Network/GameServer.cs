using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using GameLogic;
using Newtonsoft.Json;
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
                Debug.Log("Getting legal actions from player " + player.Id);
                PlayerGameState playerState = PlayerGameState.FromAtomicGame(player, rules);

                Debug.Log($"Server {playerState.PlayerInfo.PlayerName} game state hand: " + playerState.Hand.ToString());

                ClientUpdateGameState(connection, playerState, publicState);
            }
        }

        private PlayerGameState myPlayerGameState;
        private PublicGameState publicGameState;

  //      public TMP_Text LegalActionsText;
        public TMP_Text PlayerHandText;
        public TMP_Text PlayerTurnsLeft;
        public TMP_Text CurrentPlayerText;

        

        [TargetRpc]
        public void ClientUpdateGameState(NetworkConnection conn, PlayerGameState playerState, PublicGameState publicState)
        {
            Debug.Log($"Client {playerState.PlayerInfo.PlayerName} game state hand: " + playerState.Hand.ToString());
            Debug.Log($"Client update game state owner: {IsOwner}");
            myPlayerGameState = playerState;
            publicGameState = publicState;
            var actionList = DeserializeActionListJson(playerState.ActionListJson);
            LegalActionsButtonList(actionList);
            PlayerHandText.text =  string.Join("\n", playerState.Hand.All.Select(x => x.Type));
            PlayerTurnsLeft.text = publicState.TurnsLeft.ToString();
            CurrentPlayerText.text = publicState.CurrentPlayer.PlayerName.ToString();
            Debug.Log("Actions: "+string.Join("\n", actionList.Select(x => x.FormatShort())));

        }

        public void PlayAction(IGameAction action)
        {
            if (action is ICardAction cardAction)
            {
                Debug.Log("Playing action for player " + cardAction.PlayerId);
            }
            var actionJson = SerializeGameActionJson(action);
            ServerPlayAction(actionJson);
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
            game.PlayAction(action);
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
            LegalActionsList.DetachChildren();
            foreach (var action in availableActions)
            {
                var TheAction = action;
                GameObject button = Instantiate(ButtonPrefab);
                var textComponent = button.GetComponentInChildren<TMP_Text>();
                textComponent.text = action.FormatShort();
                Debug.Log($"Creating a button for the action: {TheAction}");

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
            Debug.Log(action.FormatShort() + " was chosen.");
            Debug.Log($"Action pressed owner: {IsOwner}");
            SigHej("Muggi");
            PlayAction(action);
        }

        [ServerRpc(RequireOwnership = false)]
        public void SigHej(string besked)
        {
            Debug.Log("Der bliver sagt hej til server med besked "+besked);
        }
    }
    public class PlayerGameState
    {
        // Player info and hidden hand
        public PlayerInfo PlayerInfo;
        public CardCollection Hand;
        public string ActionListJson;

        internal static PlayerGameState FromAtomicGame(Player player, AtomicPigletRules rules)
        {
            var actionList = rules.GetLegalActionsForPlayer(player).ToList();

            return new PlayerGameState
            {
                PlayerInfo = PlayerInfoFromPlayer(player),
                Hand = player.Hand,
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
            return new PlayerInfo { Id = player.Id, PlayerName = player.Name, IsReady = true, CardsLeft = player.Hand.Count };
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

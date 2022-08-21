using System.Linq;
using Assets.Network;
using GameLogic;

namespace Assets.Dto
{
    public class PlayerGameState
    {
        // Player info and hidden hand
        public PlayerInfo PlayerInfo;
        public CardCollection Hand;
        public CardCollection FutureCards;
        public string ActionListJson;

        public static PlayerGameState FromAtomicGame(Player player, AtomicPigletRules rules)
        {
            var actionList = rules.GetLegalActionsForPlayer(player).ToList();

            return new PlayerGameState
            {
                PlayerInfo = player.GetPlayerInfo(),
                Hand = player.Hand,
                FutureCards = player.FutureCards,
                ActionListJson = GameDataSerializer.SerializeActionListJson(actionList)
            };
        }
    }
}
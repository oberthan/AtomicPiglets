using System;
using System.Linq;
using System.Xml.Serialization;
using Assets.Dto;
using GameLogic;

namespace Assets.Bots
{
    public class MonkeyBot : IAtomicPigletBot
    {
        private readonly Random _rnd = new();
        public PlayerInfo PlayerInfo { get; }

        public Guid PlayerId => PlayerInfo.Id;

        public MonkeyBot()
        {
            PlayerInfo = new PlayerInfo { PlayerName = "Monkey bot", Id = Guid.NewGuid() };
        }
        public IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState)
        {
            var player = rules.Game.GetPlayer(PlayerId);
            if (rules.Game.CurrentPlayer.Id != PlayerId)
            {
                return new NoAction(player);
            }
            var actions = rules.GetLegalActionsForPlayer(PlayerId).ToList();

            // Remove really stupid trades
            actions.RemoveAll(x => x is ISelectableCardsActions selectableCardsAction
                                   && selectableCardsAction.SelectedCards.Any(y => y.Type is CardType.DefuseCard or CardType.NopeCard or CardType.AttackCard));

            return actions[_rnd.Next(actions.Count)];
        }
    }
}
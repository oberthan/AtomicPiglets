using System;
using System.Linq;
using Assets.Dto;
using Assets.Network;
using GameLogic;

namespace Assets.Bots
{
    public interface IAtomicPigletBot
    {
        PlayerInfo PlayerInfo { get; }
        IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState);
    }

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
            if (!actions.Any()) return new NoAction(player);
            return actions[_rnd.Next(actions.Count)];
        }
    }
}

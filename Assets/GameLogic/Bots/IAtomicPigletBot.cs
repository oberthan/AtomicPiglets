using Assets.Dto;
using GameLogic;

namespace Assets.Bots
{
    public interface IAtomicPigletBot
    {
        PlayerInfo PlayerInfo { get; }
        IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState);
    }
}

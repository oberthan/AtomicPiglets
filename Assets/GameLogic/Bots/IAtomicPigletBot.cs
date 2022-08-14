using System;
using System.Collections.Generic;
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
            return actions[_rnd.Next(actions.Count)];
        }
    }

    public class DummyBot : IAtomicPigletBot
    {
        private readonly Random _rnd = new();
        public PlayerInfo PlayerInfo { get; }

        public Guid PlayerId => PlayerInfo.Id;

        private readonly CardType[] collectionCardTypes = new[]
        {
            CardType.BeardCard, CardType.PotatoCard, CardType.RainbowCard, CardType.WatermelonCard, CardType.TacoCard
        };

        public DummyBot()
        {
            PlayerInfo = new PlayerInfo { PlayerName = "Dummy bot", Id = Guid.NewGuid() };
        }

        public IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState)
        {
            var player = rules.Game.GetPlayer(PlayerId);
            if (rules.Game.CurrentPlayer.Id != PlayerId)
            {
                return new NoAction(player);
            }

            var actions = rules.GetLegalActionsForPlayer(PlayerId).ToList();

            if (TryGet<DrawFromDeckAction>(actions, out var drawFromDeck))
            {
                if (_rnd.Next(6) < 5) return drawFromDeck;
            }

            if (TryGet<DrawFromPlayerAction>(actions, out var drawFromPlayer))
            {
                var usableCard =
                    drawFromPlayer.SelectableCards.FirstOrDefault(x => collectionCardTypes.Contains(x.Type));
                if (usableCard != null)
                {
                    drawFromPlayer.SelectedCards = drawFromPlayer.SelectableCards.Where(x => x.Type == usableCard.Type).ToArray();
                    return drawFromPlayer;
                }

                actions.Remove(drawFromPlayer);
            }
        

            if (TryGet<DemandCardFromPlayerAction>(actions, out var demandCardFromPlayer))
            {
                var usableCard =
                    demandCardFromPlayer.SelectableCards.FirstOrDefault(x => collectionCardTypes.Contains(x.Type));
                if (usableCard != null)
                {
                    demandCardFromPlayer.SelectedCards = demandCardFromPlayer.SelectableCards
                        .Where(x => x.Type == usableCard.Type).ToArray();
                    return demandCardFromPlayer;
                }

                actions.Remove(demandCardFromPlayer);

            }

            actions.RemoveAll(x => x is DrawFromDiscardPileAction);

            if (TryGet<NopeAction>(actions, out var nope))
            {
                var topCard = rules.Game.PlayPile.PeekFromTop(1).First();

                // No nope nope
                if (topCard.Type == CardType.NopeCard)
                    actions.Remove(nope);
                else if (rules.Game.PlayPileActions.Last() is ITargetGameAction targetGameAction && targetGameAction.TargetPlayerId == PlayerId)
                {
                    return nope;
                }
            }

            return actions[_rnd.Next(actions.Count)];
        }

        private bool TryGet<T>(List<IGameAction> actions, out T action)
        {
            action = (T)actions.FirstOrDefault(x => x is T);
            return action != null;
        }
    }
}

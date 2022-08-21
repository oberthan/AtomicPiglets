using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;

namespace Assets.Bots
{
    public static class BotHelper
    {
        public static bool TryGet<T>(List<IGameAction> actions, out T action)
        {
            action = (T)actions.FirstOrDefault(x => x is T);
            return action != null;
        }

        public static bool IsTargeting(IGameAction gameAction, Guid playerId)
        {
            if (gameAction == null) return false;
            return gameAction is ITargetGameAction targetGameAction && targetGameAction.TargetPlayerId == playerId;
        }
        public static bool IsTargetingOther(IGameAction gameAction, Guid playerId)
        {
            if (gameAction == null) return false;
            return gameAction is ITargetGameAction targetGameAction && targetGameAction.TargetPlayerId != playerId;
        }
         
        public static readonly CardType[] CollectionCardTypes = {
            CardType.BeardCard, CardType.PotatoCard, CardType.RainbowCard, CardType.WatermelonCard, CardType.TacoCard
        };

        public static readonly CardType[] AvoidDrawCards = {
            CardType.AttackCard, CardType.SkipCard, CardType.ShuffleCard
        };

        public static IEnumerable<ICardAction> GetCardActions(List<IGameAction> actions, CardType[] cardTypes)
        {
            return actions.OfType<ICardAction>().Where(x => x.Cards.Any(y => cardTypes.Contains(y.Type)));
        }

        public static bool MeNext(AtomicGame game, Guid playerId)
        {
            var currentPlayerIndex = game.Players.IndexOf(game.CurrentPlayer);
            var meIndex = game.Players.IndexOf(game.GetPlayer(playerId));
            return (currentPlayerIndex + 1) % game.Players.Count == meIndex;
        }
    }
}
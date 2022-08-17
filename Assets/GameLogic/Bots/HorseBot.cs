using System;
using System.Linq;
using Assets.Dto;
using GameLogic;

namespace Assets.Bots
{
    public class HorseBot : IAtomicPigletBot
    {
        private readonly Random _rnd = new();
        public PlayerInfo PlayerInfo { get; }

        public Guid PlayerId => PlayerInfo.Id;

        public HorseBot()
        {
            PlayerInfo = new PlayerInfo { PlayerName = "Horse bot", Id = Guid.NewGuid() };
        }

        public IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState)
        {
            var game = rules.Game;

            var actions = rules.GetLegalActionsForPlayer(PlayerId).ToList();

            // See the future
            if (playerState.FutureCards.PeekFromTop(1).Any(x => x.Type == CardType.AtomicPigletCard))
            {
                var avoidActions = BotHelper.GetCardActions(actions, BotHelper.AvoidDrawCards).ToList();
                if (avoidActions.Any())
                    return avoidActions[_rnd.Next(avoidActions.Count)];
            }

            // Someone planted the bomb
            if (game.DiscardPile.PeekFromTop(3).Any(x => x.Type == CardType.DefuseCard))
            {
                var avoidActions = BotHelper.GetCardActions(actions, BotHelper.AvoidDrawCards.Concat(new[] {CardType.SeeTheFutureCard}).ToArray()).ToList();
                var selectIndex = _rnd.Next(avoidActions.Count + 1);
                if (selectIndex < avoidActions.Count)
                    return avoidActions[selectIndex];
            }


            if (BotHelper.TryGet<AttackAction>(actions, out var attackAction))
            {
                if (game.PlayPile.Contains(CardType.AttackCard) || game.PlayerTurns > 1)
                    return attackAction;
            }


            if (BotHelper.TryGet<DrawFromDeckAction>(actions, out var drawFromDeck))
            {
                if (_rnd.Next(6) < 5) return drawFromDeck;
            }

            if (BotHelper.TryGet<DrawFromPlayerAction>(actions, out var drawFromPlayer))
            {
                var usableCard =
                    drawFromPlayer.SelectableCards.FirstOrDefault(x => BotHelper.CollectionCardTypes.Contains(x.Type));
                if (usableCard != null)
                {
                    drawFromPlayer.SelectedCards = drawFromPlayer.SelectableCards.Where(x => x.Type == usableCard.Type).ToArray();
                    return drawFromPlayer;
                }

                actions.Remove(drawFromPlayer);
            }
        

            if (BotHelper.TryGet<DemandCardFromPlayerAction>(actions, out var demandCardFromPlayer))
            {
                var usableCard =
                    demandCardFromPlayer.SelectableCards.FirstOrDefault(x => BotHelper.CollectionCardTypes.Contains(x.Type));
                if (usableCard != null)
                {
                    demandCardFromPlayer.SelectedCards = demandCardFromPlayer.SelectableCards
                        .Where(x => x.Type == usableCard.Type).ToArray();
                    return demandCardFromPlayer;
                }

                actions.Remove(demandCardFromPlayer);

            }

            actions.RemoveAll(x => x is DrawFromDiscardPileAction);

            if (BotHelper.TryGet<NopeAction>(actions, out var nope))
            {
                var topCard = game.PlayPile.PeekFromTop(1).First();

                // No nope nope
                if (topCard.Type == CardType.NopeCard) actions.Remove(nope);
                else
                {
                    if (BotHelper.IsTargeting(game.PlayPileActions.Last(), PlayerId))
                        return nope;
                    if (BotHelper.MeNext(game, PlayerId) && game.PlayPile.Contains(CardType.AttackCard))
                        return nope;
                    if (BotHelper.MeNext(game, PlayerId) && game.PlayPile.Contains(CardType.SkipCard) && _rnd.Next(2)==0)
                        return nope;
                }
            }

            if (BotHelper.TryGet<DefuseAction>(actions, out var defuse))
            {
                // Oh oh
                var cardCount = game.Deck.Count;
                defuse.AtomicPositionFromTop = _rnd.Next(Math.Max(5, cardCount));
                return defuse;
            }

            if (!actions.Any()) return new NoAction(game.GetPlayer(PlayerId));
            return actions[_rnd.Next(actions.Count)];
        }
    }
}
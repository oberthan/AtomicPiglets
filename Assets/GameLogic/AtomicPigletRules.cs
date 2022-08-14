using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public class AtomicPigletRules
    {
        private readonly AtomicGame _game;

        public AtomicPigletRules(AtomicGame game)
        {
            _game = game;
        }
        public IEnumerable<IGameAction> GetLegalActionsForPlayer(Player player)
        {
            var hand = player.Hand;

            if (player.IsGameOver())
            {
                yield return new GameOverAction(player);
                yield break;
            }

            if (_game.PlayPile.Any())
            {
                var playPileHasDefuseCard = _game.PlayPile.PeekFromTop(CardType.DefuseCard) != null;
                var topCardIsNope = _game.PlayPile.PeekFromTop(1).First().Type == CardType.NopeCard;
                var hasNopeCard = hand.Contains(CardType.NopeCard);
                
                if (hasNopeCard && !playPileHasDefuseCard && (topCardIsNope || player != _game.CurrentPlayer))
                {
                    yield return new NopeAction(player, hand.PeekFromTop(CardType.NopeCard));
                }
                else
                {
                    yield return new NoAction(player);
                }
                yield break;
            }
            if (player == _game.CurrentPlayer)
            {

                // Players has drawn atomic piglet and must deal with it.
                if (hand.Contains(CardType.AtomicPigletCard))
                {
                    if (hand.Contains(CardType.DefuseCard))
                        yield return new DefuseAction(player, hand.PeekFromTop(CardType.DefuseCard), hand.DrawFromTop(CardType.AtomicPigletCard));
                    else
                        yield return new GameOverAction(player);
                }
                else // Player has NOT just drawn atomic piglet
                {
                    // Check if player has won
                    if (_game.GetOtherPlayers(player).All(x => x.IsGameOver()))
                    {
                        yield return new WinGameAction(player);
                        yield break;
                    }

                    // Player can end this turn by drawing from deck.
                    yield return new DrawFromDeckAction(player);
                    if (hand.Contains(CardType.SkipCard)) yield return new SkipAction(player, hand.PeekFromTop(CardType.SkipCard));
                    if (hand.Contains(CardType.AttackCard)) yield return new AttackAction(player, hand.PeekFromTop(CardType.AttackCard));
                    if (hand.Contains(CardType.ShuffleCard)) yield return new ShuffleAction(player, hand.PeekFromTop(CardType.ShuffleCard));
                    if (hand.Contains(CardType.SeeTheFutureCard)) yield return new SeeTheFutureAction(player, hand.PeekFromTop(CardType.SeeTheFutureCard));
                    if (hand.Contains(CardType.FavorCard)) yield return new FavorAction(player, hand.PeekFromTop(CardType.FavorCard), GameHelper.SelectRandomOtherPlayer(_game, player.Id));
                    var twoEqualCardGroups = (from card in hand
                                               group card by card.Type into collectionGroup
                                               where collectionGroup.Count() >= 2
                                               select collectionGroup.Take(2)).ToList();
                    if (twoEqualCardGroups.Any()) yield return new DrawFromPlayerAction(player, twoEqualCardGroups, GameHelper.SelectRandomOtherPlayer(_game, player.Id));

                    var threeEqualCardGroups = (from card in hand
                                             group card by card.Type into collectionGroup
                                             where collectionGroup.Count() >= 3
                                             select collectionGroup.Take(3)).ToList();
                    if (threeEqualCardGroups.Any()) yield return new DemandCardFromPlayerAction(player, threeEqualCardGroups, GameHelper.SelectRandomOtherPlayer(_game, player.Id));

                    var distinctCards = hand.All.Distinct(new GenericEqualityComparer<Card, CardType>(x => x.Type)).ToList();
                    if (distinctCards.Count >= 5) yield return new DrawFromDiscardPileAction(player, distinctCards.ToArray());
                }
            }
            else yield return new NoAction(player);
        }
    }

    public class GenericEqualityComparer<T, U> : IEqualityComparer<T>
    {
        private readonly Func<T, U> _selector;

        public GenericEqualityComparer(Func<T, U> selector)
        {
            this._selector = selector;
        }
        public bool Equals(T x, T y)
        {
            return EqualityComparer<U>.Default.Equals(_selector(x), _selector(y));
        }

        public int GetHashCode(T x)
        {
            return EqualityComparer<U>.Default.GetHashCode(_selector(x));
        }
    }
}
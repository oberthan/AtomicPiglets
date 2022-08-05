using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public class AtomicPigletRules
    {
        private readonly AtomicGame game;

        public AtomicPigletRules(AtomicGame game)
        {
            this.game = game;
        }
        public IEnumerable<IGameAction> GetLegalActionsForPlayer(Player player)
        {
            if (player == game.CurrentPlayer)
            {
                var hand = player.Hand;

                // Players has drawn atomic piglet and must deal with it.
                if (hand.Contains<AtomicPigletCard>())
                {
                    if (hand.Contains<DefuseCard>())
                        yield return new DefuseAction(player, hand.PeekFromTop<DefuseCard>(), hand.DrawFromTop<AtomicPigletCard>());
                    else
                        yield return new GameOverAction(player);
                }
                else // Player has NOT just drawn atomic piglet
                {
                    // Player can end this turn by drawing from deck.
                    yield return new DrawFromDeckAction(player);

                    if (hand.Contains<SkipCard>()) yield return new SkipAction(player, hand.PeekFromTop<SkipCard>());
                    if (hand.Contains<AttackCard>()) yield return new AttackAction(player, hand.PeekFromTop<AttackCard>());
                    if (hand.Contains<ShuffleCard>()) yield return new ShuffleAction(player, hand.PeekFromTop<ShuffleCard>());
                    if (hand.Contains<SeeTheFutureCard>()) yield return new SeeTheFutureAction(player, hand.PeekFromTop<SeeTheFutureCard>());
                    if (hand.Contains<FavorCard>()) yield return new FavorAction(player, hand.PeekFromTop<FavorCard>());
                    var twoEqualCardGroups = from card in hand.All
                                               group card by card.Name into collectionGroups
                                               where collectionGroups.Count() >= 2
                                               select collectionGroups.Take(2).ToList();

                    var threeEqualCardGroups = from card in hand.All
                                             group card by card.Name into collectionGroups
                                             where collectionGroups.Count() >= 3
                                             select collectionGroups.Take(3).ToList();
                    var distinctCards = hand.All.Distinct(new GenericEqualityComparer<Card, string>(x => x.Name)).ToList();
                }
            }
            yield break;
        }
    }

    public class GenericEqualityComparer<T, U> : IEqualityComparer<T>
    {
        private readonly Func<T, U> selector;

        public GenericEqualityComparer(Func<T, U> selector)
        {
            this.selector = selector;
        }
        public bool Equals(T x, T y)
        {
            return EqualityComparer<U>.Default.Equals(selector(x), selector(y));
        }

        public int GetHashCode(T x)
        {
            return EqualityComparer<U>.Default.GetHashCode(selector(x));
        }
    }
}
﻿using System;
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
                if (hand.Contains(CardType.AtomicPigletCard))
                {
                    if (hand.Contains(CardType.DefuseCard))
                        yield return new DefuseAction(player, hand.PeekFromTop(CardType.DefuseCard), hand.DrawFromTop(CardType.AtomicPigletCard));
                    else
                        yield return new GameOverAction(player);
                }
                else // Player has NOT just drawn atomic piglet
                {
                    // Player can end this turn by drawing from deck.
                    yield return new DrawFromDeckAction() { playerId = player.Id};
                    if (hand.Contains(CardType.SkipCard)) yield return new SkipAction(player, hand.PeekFromTop(CardType.SkipCard));
                    if (hand.Contains(CardType.AttackCard)) yield return new AttackAction(player, hand.PeekFromTop(CardType.AttackCard));
                    if (hand.Contains(CardType.ShuffleCard)) yield return new ShuffleAction(player, hand.PeekFromTop(CardType.ShuffleCard));
                    if (hand.Contains(CardType.SeeTheFutureCard)) yield return new SeeTheFutureAction(player, hand.PeekFromTop(CardType.SeeTheFutureCard));
                    if (hand.Contains(CardType.FavorCard)) yield return new FavorAction(player, hand.PeekFromTop(CardType.FavorCard));
                    var twoEqualCardGroups = from card in hand.All
                                               group card by card.GetType() into collectionGroups
                                               where collectionGroups.Count() >= 2
                                               select collectionGroups.Take(2).ToList();
                    if (twoEqualCardGroups.Any()) yield return new DrawFromPlayerAction(player, twoEqualCardGroups.SelectMany(x => x).ToArray());

                    var threeEqualCardGroups = from card in hand.All
                                             group card by card.GetType() into collectionGroups
                                             where collectionGroups.Count() >= 3
                                             select collectionGroups.Take(3).ToList();
                    if (threeEqualCardGroups.Any()) yield return new DemandCardFromPlayerAction(player, threeEqualCardGroups.SelectMany(x => x).ToArray());

                    var distinctCards = hand.All.Distinct(new GenericEqualityComparer<Card, Type>(x => x.GetType())).ToList();
                    if (distinctCards.Count >= 5) yield return new DrawFromDiscardPileAction(player, distinctCards.ToArray());
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
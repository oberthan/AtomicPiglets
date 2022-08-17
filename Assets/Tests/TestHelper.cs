using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic;

namespace Assets.Tests
{
    public static class TestHelper
    {
        public static void MakeTestHand(Player player, CardType[] cardTypes)
        {
            // Clear hand
            player.Hand.Clear();
            foreach (var cardType in cardTypes)
            {
                player.Hand.AddNew(cardType);
            }
        }

        /// <summary>
        /// Permutates a collection of items into all possible orders.
        /// </summary>
        /// <remarks>
        /// ABC ->
        ///  ABC
        ///  ACB
        ///  BAC
        ///  BCA
        ///  CAB
        ///  CBA 
        /// </remarks>
        public static IEnumerable<IEnumerable<T>> OrderPermutate<T>(IEnumerable<T> items)
        {
            var itemsList = items.ToList();
            foreach (var item in itemsList)
            {
                var restPermutations = OrderPermutate(itemsList.Where(x => !Equals(x, item)));
                foreach (var restPermutation in restPermutations)
                {
                    yield return new[] { item }.Concat(restPermutation);
                }
            }
        }

        /// <summary>
        /// Permutates collection into all possible pair sets.
        /// </summary>
        /// <remarks>
        /// ABC ->
        ///  AB
        ///  AC
        ///  BC
        /// </remarks>
        public static IEnumerable<(T, T)> PairPermutate<T>(this IEnumerable<T> items)
        {
            var itemsList = items.ToList();

            for (int i = 0; i < itemsList.Count; i++)
            {
                for (int j = i+1; j < itemsList.Count; j++)
                {
                    yield return (itemsList[i], itemsList[j]);
                }
            }
        }
    }

    class MockPlayTimer : IPlayTimer
    {
        private float _delay;

        public void Start(float delay)
        {
            _delay = delay;
        }

        public void Tick()
        {
            if (_delay > 0)
            {
                _delay = 0;
            }
            else if (_delay <= 0) Elapse();
        }

        public void Elapse()
        {
            OnTimerElapsed();
        }

        public event EventHandler TimerElapsed;

        protected virtual void OnTimerElapsed()
        {
            TimerElapsed?.Invoke(this, EventArgs.Empty);
        }
    }

}
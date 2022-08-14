using System;
using GameLogic;

namespace Assets.Tests
{
    public class TestFactory
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
    }

    class MockPlayTimer : IPlayTimer
    {
        public void Start(float delay)
        {
            // Do nothing
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
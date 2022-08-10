using System;
using NUnit.Framework;
using GameLogic;
using System.Linq;

namespace GameLogicTest
{
    [TestFixture]
    public class GameActionTest
    {
        [Test]
        public void DefuseActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;

            var defuseCard = new Card(CardType.DefuseCard);
            var atomicCard = new Card(CardType.AtomicPigletCard);
            var action = new DefuseAction(player, defuseCard, atomicCard);
            action.AtomicPositionFromTop = 2;

            action.Execute(game);

            // Defusing will end turn
            Assert.That(game.CurrentPlayer, Is.Not.EqualTo(player));

            // Atomic card should be hidden in deck at desired position.
            var deckCards = game.Deck.All.Reverse().ToList();
            Assert.That(deckCards[2], Is.EqualTo(atomicCard));
        }

        [Test]
        public void DefuseWithTurnsLeft()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            game.PlayerTurns = 2;
            var player = game.CurrentPlayer;

            var defuseCard = new Card(CardType.DefuseCard);
            var atomicCard = new Card(CardType.AtomicPigletCard);
            var action = new DefuseAction(player, defuseCard, atomicCard);
            action.AtomicPositionFromTop = 2;

            action.Execute(game);

            // Defusing will end turn, but not next player
            Assert.That(game.CurrentPlayer, Is.EqualTo(player));
            Assert.That(game.PlayerTurns, Is.EqualTo(1));

            // Atomic card should be hidden in deck at desired position.
            var deckCards = game.Deck.All.Reverse().ToList();
            Assert.That(deckCards[2], Is.EqualTo(atomicCard));
        }

        [Test]
        public void SkipActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;
            game.PlayerTurns = 2;
            var skipCard = new Card(CardType.SkipCard);
            var action = new SkipAction(player, skipCard);
            action.Execute(game);

            Assert.That(game.PlayerTurns, Is.EqualTo(1));
            Assert.That(game.CurrentPlayer, Is.EqualTo(player));
        }

        [Test]
        [TestCase(1, 2)]
        // Hvad gør man hvis første tur i en attack skippes og at der herefter trækkes op og attackes?
        // [TestCase(1, 3)] ???
        [TestCase(2, 4)]
        [TestCase(3, 5)]
        [TestCase(4, 6)]
        public void NewAttackActionWillGiveTwoMoreTurnsToNext(int turnsLeft, int nextPlayerTurns)
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;
            game.PlayerTurns = turnsLeft;

            var attackCard = player.Hand.AddNew(CardType.AttackCard);
            var action = new AttackAction(player, attackCard);

            action.Execute(game);

            // Player is next player
            Assert.That(game.CurrentPlayer, Is.Not.EqualTo(player));
            Assert.That(game.PlayerTurns, Is.EqualTo(nextPlayerTurns));
        }

        [Test]
        public void ShuffleActionShufflesDeck()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(20);
            var player = game.CurrentPlayer;

            var startDeck = game.Deck.All.ToList();

            var card = new Card(CardType.ShuffleCard);
            var action = new ShuffleAction(player, card);

            action.Execute(game);

            var currentDeck = game.Deck.All.ToList();

            Assert.That(currentDeck, Is.Not.EqualTo(startDeck));
        }

        [Test]
        public void SeeTheFuturePeeksAtThreeCards()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;

            var startDeckFromTop = game.Deck.All.Reverse().ToList();

            var card = new Card(CardType.SeeTheFutureCard);
            var action = new SeeTheFutureAction(player, card);

            Assert.That(action.FutureCards, Is.Null);

            action.Execute(game);

            Assert.That(action.FutureCards, Is.Not.Null);
            Assert.That(action.FutureCards.All, Is.EqualTo(startDeckFromTop.Take(3)));
        }

        [Test]
        public void NopeRemovesTopPlayedAction()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var gamePlayTimer = new MockPlayTimer();
            game.PlayTimer = gamePlayTimer;

            var opponent = game.CurrentPlayer;
            var attackCard = opponent.Hand.AddNew(CardType.AttackCard);

            // Other player played attack
            var attackAction = new AttackAction(opponent, attackCard);
            game.PlayCard(attackAction);
            game.EndTurn();

            Assume.That(game.PlayPileActions.Last(), Is.EqualTo(attackAction));

            var player = game.CurrentPlayer;
            var nopeCard = player.Hand.AddNew(CardType.NopeCard);

            // Current player nopes the attack
            var nopeAction = new NopeAction(player, nopeCard);
            
            game.PlayCard(nopeAction);
            Assert.That(game.PlayPileActions.Last(), Is.EqualTo(nopeAction));

            // Time is up, nope removes attack
            gamePlayTimer.Elapse();

            Assert.That(game.PlayPileActions, Is.Empty);
        }

        [Test]
        public void NopeNopeKeepsPlayedAction()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var gamePlayTimer = new MockPlayTimer();
            game.PlayTimer = gamePlayTimer;

            var playerA = game.CurrentPlayer;
            var playerB = game.CurrentPlayer == game.Players.First() ? game.Players.Last() : game.Players.First();
            var nopeBCard = playerB.Hand.AddNew(CardType.NopeCard);

            var attackCard = playerA.Hand.AddNew(CardType.AttackCard);
            var nopeACard = playerA.Hand.AddNew(CardType.NopeCard);

            // Player A plays attack
            var attackAction = new AttackAction(playerA, attackCard);
            game.PlayCard(attackAction);

            // Player B nopes
            var playerBNopeAction = new NopeAction(playerB, nopeBCard);
            game.PlayCard(playerBNopeAction);

            // Player A nopes the nope
            var playerANopeAction = new NopeAction(playerA, nopeACard);
            game.PlayCard(playerANopeAction);

            // Time is up, nope removes attack
            gamePlayTimer.Elapse();

            // Check that attack is executed
            Assert.That(game.PlayerTurns, Is.EqualTo(2));
            Assert.That(game.CurrentPlayer, Is.EqualTo(playerB));
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
}

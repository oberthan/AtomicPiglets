using GameLogic;
using NUnit.Framework;

namespace GameLogicTest
{
    [TestFixture]
    public class GameActionTest
    {
        [Test]
        public void SkipActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;
            game.PlayerTurns = 2;
            var skipCard = new SkipCard();
            var action = new SkipAction(player, skipCard);
            action.Execute(game);

            Assert.That(game.PlayerTurns, Is.EqualTo(1));
            Assert.That(game.CurrentPlayer, Is.EqualTo(player));
        }

        [Test]
        public void DefuseActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var player = game.CurrentPlayer;

            var defuseCard = new DefuseCard();
            var atomicCard = new AtomicPigletCard();
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

            var defuseCard = new DefuseCard();
            var atomicCard = new AtomicPigletCard();
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

            var attackCard = new AttackCard();
            var action = new AttackAction(player, attackCard);

            action.Execute(game);

            // Player is next player
            Assert.That(game.CurrentPlayer, Is.Not.EqualTo(player));
            Assert.That(game.PlayerTurns, Is.EqualTo(nextPlayerTurns));
        }
    }
}

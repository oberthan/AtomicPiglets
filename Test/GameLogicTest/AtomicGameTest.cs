using System.Linq;
using GameLogic;
using NUnit.Framework;

namespace GameLogicTest
{
    [TestFixture]
    public class AtomicGameTest
    {
        [Test]
        [TestCase(5, 56)] // 46 non-nuke/defuser
        [TestCase(4, 55)] // -1 nuke
        [TestCase(3, 53)] // - 2 nuke, -1 defuser
        [TestCase(2, 51)] // - 3 nuke, -2 defuser
        [TestCase(6, 2*46 + 5 + 8)] // 2*46 + nukes + defusers
        public void TotalCardsDependsOnPlayers(int playerCount, int expectedTotal)
        {
            var game = AtomicGame.CreateExplodingKittensLikeGame(playerCount);
            var deckCount = game.Deck.Count;
            var playerHandsCount = game.Players.Sum(x => x.Hand.Count);

            Assert.That(deckCount + playerHandsCount, Is.EqualTo(expectedTotal));
        }

        [Test]
        [TestCase(5, 4)]
        [TestCase(2, 1)]
        public void DeckContainsOneLessNukeThanPlayers(int players, int expectedNukes)
        {
            var game = AtomicGame.CreateExplodingKittensLikeGame(players);
            var nukeCount = game.Deck.All.OfType<AtomicPigletCard>().Count();

            Assert.That(nukeCount, Is.EqualTo(expectedNukes));
        }

        [Test]
        [TestCase(5, 6)]
        [TestCase(4, 6)]
        [TestCase(3, 5)]
        [TestCase(2, 4)]
        public void TotalDefusersAsExpected(int players, int expectedDefusers)
        {
            var game = AtomicGame.CreateExplodingKittensLikeGame(players);

            var allCards = game.Deck.All.Concat(game.Players.SelectMany(x => x.Hand.All)).ToList();

            var defuseCount = allCards.OfType<DefuseCard>().Count();

            Assert.That(defuseCount, Is.EqualTo(expectedDefusers));
        }

    }
}

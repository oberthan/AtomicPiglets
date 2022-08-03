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
            var game = GameFactory.CreateExplodingKittensLikeGame(playerCount);
            var deckCount = game.Deck.Count;
            var playerHandsCount = game.Players.Sum(x => x.Hand.Count);

            Assert.That(deckCount + playerHandsCount, Is.EqualTo(expectedTotal));
        }

        [Test]
        [TestCase(5, 4)]
        [TestCase(2, 1)]
        public void DeckContainsOneLessNukeThanPlayers(int players, int expectedNukes)
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(players);
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
            var game = GameFactory.CreateExplodingKittensLikeGame(players);

            var allCards = game.Deck.All.Concat(game.Players.SelectMany(x => x.Hand.All)).ToList();

            var defuseCount = allCards.OfType<DefuseCard>().Count();

            Assert.That(defuseCount, Is.EqualTo(expectedDefusers));
        }

        [Test]
        public void PlayerIsSelectedWhenGameIsCreated()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(5);
            Assert.That(game.CurrentPlayer, Is.Not.Null);
            Assert.That(game.PlayerTurns, Is.EqualTo(1));
        }

        [Test]
        [TestCase(2)]
        [TestCase(5)]
        public void NextPlayerCyclesCurrentPlayer(int playerCount)
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(playerCount);
            var players = game.Players.ToList();
            var currentPlayerIndex = players.IndexOf(game.CurrentPlayer);

            for (int i = 0; i < playerCount*2+1; i++)
            {
                game.NextPlayer();

                var nextPlayerIndex = players.IndexOf(game.CurrentPlayer);
                Assert.That(nextPlayerIndex, Is.EqualTo((currentPlayerIndex+1)%playerCount));
                currentPlayerIndex = nextPlayerIndex;
            }
        }

        [Test]
        public void EndTurnDecrementsPlayerTurns()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var startPlayer = game.CurrentPlayer;
            game.PlayerTurns = 3;
            game.EndTurn();

            // Current player is the same.
            Assert.That(startPlayer, Is.EqualTo(game.CurrentPlayer));

            // Turns is decremented
            Assert.That(game.PlayerTurns, Is.EqualTo(2));
        }

        [Test]
        public void EndTurnPassesToNextPlayerWhenTurnsIsOne()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var startPlayer = game.CurrentPlayer;
            game.PlayerTurns = 1;
            game.EndTurn();

            // Current player is not the same
            Assert.That(startPlayer, Is.Not.EqualTo(game.CurrentPlayer));

            // Turns are reset for new player.
            Assert.That(game.PlayerTurns, Is.EqualTo(1));
        }



        [Test]
        public void PrintGame()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(5);

            Console.WriteLine("-- Players --");
            foreach (var player in game.Players)
            {
                Console.WriteLine($"{player.Name}: "+player.FormatHand());
            }
            Console.WriteLine();
            Console.WriteLine("-- Deck --");
            Console.WriteLine($"");

        }

        [Test]
        public void ExecutePlayedCardsMoveCardsToDiscardPile()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(5);

            // play some cards
            // Execute played cards
            // Assert that played cards end on top of discard pile

        }
    }
}

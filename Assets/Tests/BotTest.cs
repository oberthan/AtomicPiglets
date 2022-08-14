using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Bots;
using Assets.Dto;
using GameLogic;
using NUnit.Framework;

namespace Assets.Tests
{
    [TestFixture]
    public class BotTest
    {
        [Test]
        public void MonkeyBattle()
        {
            var monkeyA = new MonkeyBot();
            var monkeyB = new MonkeyBot();
            var bots = new[] { monkeyA, monkeyB };
            var monkeyPlayers = new Player[] { new(monkeyA.PlayerInfo), new(monkeyB.PlayerInfo) };
            var game = GameFactory.CreateExplodingKittensLikeGame(monkeyPlayers);
            var rules = new AtomicPigletRules(game);

            for (int i = 0; i < 100; i++)
            {
                foreach (var bot in bots)
                {
                    var player = game.GetPlayer(bot.PlayerId);

                    if (player.IsGameOver()) continue;
                    var action = bot.GetAction(rules, PlayerGameState.FromAtomicGame(player, rules), PublicGameState.FromAtomicGame(game));

                    game.PlayAction(action);
                }
            }

            // Check for winner
            Assert.That(game.GetAllCards().Contains(CardType.AtomicPigletCard));
            Assert.That(game.Players.Count(x => x.IsGameOver()), Is.EqualTo(1));
        }

        [Test]
        public void DummyBattle()
        {
            var botA = new DummyBot();
            var botB = new DummyBot();
            var bots = new[] { botA, botB };
            var monkeyPlayers = new Player[] { new(botA.PlayerInfo), new(botB.PlayerInfo) };
            var game = GameFactory.CreateExplodingKittensLikeGame(monkeyPlayers);
            var rules = new AtomicPigletRules(game);

            for (int i = 0; i < 100; i++)
            {
                foreach (var bot in bots)
                {
                    var player = game.GetPlayer(bot.PlayerId);

                    if (player.IsGameOver()) continue;
                    var action = bot.GetAction(rules, PlayerGameState.FromAtomicGame(player, rules), PublicGameState.FromAtomicGame(game));

                    game.PlayAction(action);
                }
            }

            // Check for winner
            Assert.That(game.GetAllCards().Contains(CardType.AtomicPigletCard));
            Assert.That(game.Players.Count(x => x.IsGameOver()), Is.EqualTo(1));
        }

        [Test]
        public void BotBattle()
        {
            var botWins = new Dictionary<string, int>
            {
                { nameof(MonkeyBot), 0 },
                { nameof(DummyBot), 0 }
            };
            for (int i = 0; i < 100; i++)
            {
                var monkeyBot = new MonkeyBot();
                var dummyBot = new DummyBot();
                var bots = new IAtomicPigletBot[] { monkeyBot, dummyBot };
                var botPlayers = new Player[] { new(monkeyBot.PlayerInfo), new(dummyBot.PlayerInfo) };

                var winner = SimulateGame(botPlayers, bots);
                if (winner != null)
                {
                    var winnerBot = bots.First(x => x.PlayerInfo.Id == winner.Id);
                    botWins[winnerBot.GetType().Name] += 1;
                }
            }

            Assert.Fail($"{botWins.First().Key}: {botWins.First().Value}, {botWins.Last().Key}: {botWins.Last().Value}");
        }

        private static Player SimulateGame(IEnumerable<Player> players, IAtomicPigletBot[] bots)
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(players);
            var playTimer = new MockPlayTimer();
            game.PlayTimer = playTimer;
            var rules = new AtomicPigletRules(game);

            int round = 0;
            while (round < 200)
            {
                foreach (var bot in bots)
                {
                    var player = game.GetPlayer(bot.PlayerInfo.Id);

                    if (player.IsGameOver()) continue;
                    var action = bot.GetAction(rules, PlayerGameState.FromAtomicGame(player, rules),
                        PublicGameState.FromAtomicGame(game));
                    if (action is WinGameAction)
                    {
                        return player;
                    }

                    game.PlayAction(action);

                    // Give bots a chance to respond to plays with nope.
                    if (round % 2 == 1) playTimer.Elapse();
                    round++;
                }
            }

            return null;
        }
    }
}

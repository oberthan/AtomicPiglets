using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Bots;
using Assets.Dto;
using Assets.GameLogic.Bots;
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
        public void HorseBattle()
        {
            var botA = new HorseBot();
            var botB = new HorseBot();
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
            var botFactories = new Func<IAtomicPigletBot>[]
            {
                () => new MonkeyBot(),
                () => new HorseBot(),
                () => new FoxBot(),
            };

            var battlePairs = botFactories.PairPermutate();

            foreach (var (botFactoryA, botFactoryB) in battlePairs)
            {
                var protoA = botFactoryA();
                var protoB = botFactoryB();
                var botWins = new Dictionary<string, int>
                {
                    { protoA.PlayerInfo.PlayerName, 0 },
                    { protoB.PlayerInfo.PlayerName, 0 }
                };

                var fightsCount = 100;
                for (int i = 0; i < fightsCount; i++)
                {
                    var botA = botFactoryA();
                    var botB = botFactoryB();
                    var bots = new[] { botA, botB };

                    var botPlayers = new Player[] { new(botA.PlayerInfo), new(botB.PlayerInfo) };

                    var winner = SimulateGame(botPlayers, bots);
                    if (winner != null)
                    {
                        botWins[winner.Name] += 1;
                    }
                }

                TestContext.Out.WriteLine($"{fightsCount} {protoA.PlayerInfo.PlayerName} vs {protoB.PlayerInfo.PlayerName} fights completed");
                foreach (var (botName, wins) in botWins)
                {
                    TestContext.Out.WriteLine($"{botName}: {wins} wins ({100.0 * wins / fightsCount:F0}%)");
                }
            }

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

                    playTimer.Tick();

                    round++;
                }
            }

            return null;
        }
    }
}

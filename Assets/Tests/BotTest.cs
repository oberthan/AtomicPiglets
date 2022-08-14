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

            var allActions = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                foreach (var bot in bots)
                {
                    var player = game.GetPlayer(bot.PlayerId);

                    if (player.IsGameOver()) continue;
                    var action = bot.GetAction(rules, PlayerGameState.FromAtomicGame(player, rules), PublicGameState.FromAtomicGame(game));
                    allActions.AppendLine(action.ToString());

                    game.PlayAction(action);
                }
            }

            // Check for winner
            Assert.That(game.GetAllCards().Contains(CardType.AtomicPigletCard), allActions.ToString);
            Assert.That(game.Players.Count(x => x.IsGameOver()), Is.EqualTo(1));
        }
    }
}

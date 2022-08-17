using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameLogic;
using NUnit.Framework;

namespace Assets.Tests
{
    [TestFixture]
    public class AtomicPigletsRulesTest
    {
        [Test]
        public void MultiCardLegalActionsTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var rules = new AtomicPigletRules(game);

            var currentPlayer = game.CurrentPlayer;

            TestHelper.MakeTestHand(currentPlayer,
                new[]
                {
                    CardType.BeardCard, CardType.DefuseCard, CardType.BeardCard, CardType.WatermelonCard,
                    CardType.BeardCard, CardType.WatermelonCard
                });

            var legalActions = rules.GetLegalActionsForPlayer(currentPlayer).ToList();
            Assert.That(legalActions.Select(x => x.GetType().Name), Is.EquivalentTo(new [] { "DrawFromDeckAction", "DrawFromPlayerAction", "DemandCardFromPlayerAction" } ));

            var demandCardFromPlayerAction = legalActions.OfType<DemandCardFromPlayerAction>().Single();
            Assert.That(demandCardFromPlayerAction.SelectableCards.Count, Is.EqualTo(3));
            Assert.That(demandCardFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.BeardCard), Is.EqualTo(3), $"Selectable cards was {string.Join(", ", demandCardFromPlayerAction.SelectableCards.Select(x => x.Type))}");

            var drawFromPlayerAction = legalActions.OfType<DrawFromPlayerAction>().Single();
            Assert.That(drawFromPlayerAction.SelectableCards.Count, Is.EqualTo(4));
            Assert.That(drawFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.BeardCard), Is.EqualTo(2));
            Assert.That(drawFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.WatermelonCard), Is.EqualTo(2));
        }

        [Test]
        public void GameOverActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var rules = new AtomicPigletRules(game);

            var currentPlayer = game.CurrentPlayer;

            TestHelper.MakeTestHand(currentPlayer, new[] { CardType.BeardCard, CardType.BeardCard, CardType.SkipCard });
            game.Deck.Clear();
            game.Deck.AddNew(CardType.AtomicPigletCard);

            game.PlayAction(new DrawFromDeckAction(currentPlayer));

            var actions = rules.GetLegalActionsForPlayer(currentPlayer);

            Assert.That(actions.Single(), Is.TypeOf<GameOverAction>());
            Assert.That(currentPlayer.IsGameOver, Is.True);
        }

        [Test]
        public void WinActionTest()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(3);
            var rules = new AtomicPigletRules(game);

            var player0 = game.Players[0];
            var player1 = game.Players[1];
            var player2 = game.Players[2];

            TestHelper.MakeTestHand(player0, new[] { CardType.BeardCard, CardType.BeardCard, CardType.AtomicPigletCard });
            TestHelper.MakeTestHand(player2, new[] { CardType.BeardCard, CardType.BeardCard, CardType.AtomicPigletCard });

            player0.IsGameOver();

            game.Deck.Clear();
            game.Deck.AddNew(CardType.WatermelonCard);
            game.EndTurn();

            Assert.That(game.CurrentPlayer, Is.EqualTo(player1));

            var actions0 = rules.GetLegalActionsForPlayer(player0);
            var actions1 = rules.GetLegalActionsForPlayer(player1);
            var actions2 = rules.GetLegalActionsForPlayer(player2);

            Assert.That(actions0.Single(), Is.TypeOf<GameOverAction>());
            Assert.That(actions1.Single(), Is.TypeOf<WinGameAction>());
            Assert.That(actions2.Single(), Is.TypeOf<GameOverAction>());
        }

    }
}

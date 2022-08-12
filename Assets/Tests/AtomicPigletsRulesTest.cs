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
        public void LegalActionsPairs()
        {
            var game = GameFactory.CreateExplodingKittensLikeGame(2);
            var rules = new AtomicPigletRules(game);

            var currentPlayer = game.CurrentPlayer;

            MakeTestHand(currentPlayer,
                new[]
                {
                    CardType.BeirdCard, CardType.DefuseCard, CardType.BeirdCard, CardType.WatermelonCard,
                    CardType.BeirdCard, CardType.WatermelonCard
                });

            var legalActions = rules.GetLegalActionsForPlayer(currentPlayer).ToList();
            Assert.That(legalActions.Select(x => x.GetType().Name), Is.EquivalentTo(new [] { "DrawFromDeckAction", "DrawFromPlayerAction", "DemandCardFromPlayerAction" } ));

            var demandCardFromPlayerAction = legalActions.OfType<DemandCardFromPlayerAction>().Single();
            Assert.That(demandCardFromPlayerAction.SelectableCards.Count, Is.EqualTo(3));
            Assert.That(demandCardFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.BeirdCard), Is.EqualTo(3), $"Selectable cards was {string.Join(", ", demandCardFromPlayerAction.SelectableCards.Select(x => x.Type))}");

            var drawFromPlayerAction = legalActions.OfType<DrawFromPlayerAction>().Single();
            Assert.That(drawFromPlayerAction.SelectableCards.Count, Is.EqualTo(4));
            Assert.That(drawFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.BeirdCard), Is.EqualTo(2));
            Assert.That(drawFromPlayerAction.SelectableCards.Count(x => x.Type == CardType.WatermelonCard), Is.EqualTo(2));

        }

        private void MakeTestHand(Player player, CardType[] cardTypes)
        {
            // Clear hand
            player.Hand.RemoveAll(player.Hand);
            foreach (var cardType in cardTypes)
            {
                player.Hand.AddNew(cardType);
            }
        }
    }
}

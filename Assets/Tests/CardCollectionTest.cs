using NUnit.Framework;
using GameLogic;
using System.Linq;

namespace GameLogicTest
{
    [TestFixture]
    public class CardCollectionTest
    {
        [Test]
        [TestCase(0, 0, 5)]
        [TestCase(1, 1, 4)]
        [TestCase(2, 2, 3)]
        [TestCase(5, 5, 0)]
        [TestCase(6, 5, 0)]
        public void DrawTopRemovesExpectedNumberOfCards(int cardsRequested, int cardsDrawn, int cardsLeft)
        {
            var cards = new CardCollection();
            cards.AddNew(5, () => new Card(CardType.SkipCard));

            var drawnCards = cards.DrawTop(cardsRequested);

            Assert.That(drawnCards.Count, Is.EqualTo(cardsDrawn));
            Assert.That(cards.Count, Is.EqualTo(cardsLeft));
        }

        [Test]
        public void DrawTopDrawsInStackOrder()
        {
            var cards = new CardCollection();
            cards.Add(new Card(CardType.SkipCard));
            cards.Add(new Card(CardType.NopeCard));
            cards.Add(new Card(CardType.FavorCard));

            var drawnCards = cards.DrawTop(2);

            Assert.That(drawnCards.All.First().Type, Is.EqualTo(CardType.FavorCard));
            Assert.That(drawnCards.All.Last().Type, Is.EqualTo(CardType.NopeCard));

        }
    }

}
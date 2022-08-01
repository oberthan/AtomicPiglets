using GameLogic;
using NUnit.Framework;

namespace GameLogicTest
{
    [TestFixture]
    public class CardTest
    {
        [Test]
        public void TestMe()
        {
            Assert.That("42", Is.EqualTo("42"));
         
        }
    }

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
            cards.AddNew(5, () => new SkipCard());

            var drawnCards = cards.DrawTop(cardsRequested);

            Assert.That(drawnCards.Count, Is.EqualTo(cardsDrawn));
            Assert.That(cards.Count, Is.EqualTo(cardsLeft));
        }
    }

}
using System;
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

        [Test]
        public void RemoveAllRemovesById()
        {
            var handA = new CardCollection();
            var c1 = handA.AddNew(CardType.SkipCard);
            var c2 = handA.AddNew(CardType.NopeCard);
            var c3 = handA.AddNew(CardType.NopeCard);
            var c4 = handA.AddNew(CardType.FavorCard);

            var c2b = new Card(CardType.NopeCard) { Id = c2.Id };

            handA.RemoveAll(new[] { c1, c2b, c4 });
            Assert.That(handA, Is.EqualTo(new[] {c3}));
        }

        [Test]
        public void RemoveAllThrowsIfCardDoesNotExist()
        {
            var handA = new CardCollection();
            var c1 = handA.AddNew(CardType.SkipCard);
            var c2 = handA.AddNew(CardType.NopeCard);
            var c3 = handA.AddNew(CardType.NopeCard);
            var c4 = handA.AddNew(CardType.FavorCard);

            var c5 = new Card(CardType.NopeCard) { Id = 255 };

            Assert.Throws<ArgumentException>(() => handA.RemoveAll(new[] { c1, c4, c5 }));
        }

        [Test]
        public void TransferCardToMovesCardBetweenCollections()
        {
            var handA = new CardCollection();
            handA.AddNew(CardType.SkipCard);
            handA.AddNew(CardType.NopeCard);
            handA.AddNew(CardType.FavorCard);
            var nopeCard = handA.PeekFromTop(CardType.NopeCard);

            var handB = new CardCollection();
            handB.AddNew(CardType.AttackCard);
            handB.AddNew(CardType.DefuseCard);

            handA.TransferCardTo(nopeCard, handB);

            Assert.That(handA.Select(x => x.Type), Is.EqualTo(new [] { CardType.SkipCard, CardType.FavorCard}));
            Assert.That(handB.Select(x => x.Type), Is.EqualTo(new [] { CardType.AttackCard, CardType.DefuseCard, CardType.NopeCard}));
        }

        [Test]
        public void TransferNullCardDoesNothing()
        {
            var handA = new CardCollection();
            handA.AddNew(CardType.SkipCard);
            var nopeCard = handA.AddNew(CardType.NopeCard);
            handA.AddNew(CardType.FavorCard);

            var handB = new CardCollection();
            handB.AddNew(CardType.AttackCard);
            handB.AddNew(CardType.DefuseCard);

            handA.TransferCardTo(null, handB);

            Assert.That(handA.Select(x => x.Type), Is.EqualTo(new[] { CardType.SkipCard, CardType.NopeCard, CardType.FavorCard }));
            Assert.That(handB.Select(x => x.Type), Is.EqualTo(new[] { CardType.AttackCard, CardType.DefuseCard }));
        }

        [Test]
        public void TransferNonExistingCardThrows()
        {
            var handA = new CardCollection();
            handA.AddNew(CardType.SkipCard);
            handA.AddNew(CardType.NopeCard);
            handA.AddNew(CardType.FavorCard);

            var noCard = new Card(CardType.WatermelonCard) { Id = 255 };

            var handB = new CardCollection();
            handB.AddNew(CardType.AttackCard);
            handB.AddNew(CardType.DefuseCard);

            Assert.Throws<ArgumentException>(() => handA.TransferCardTo(noCard, handB));
        }
    }

}
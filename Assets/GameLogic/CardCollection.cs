using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public class CardCollection : IEnumerable<Card>
    {
        private static int nextId = 1;
        public List<Card> cards = new List<Card>();
        public Card HighlightedCard { get; set; }

        public CardCollection()
        {
        }

        public CardCollection(IEnumerable<Card> cards)
        {
            this.cards = cards.ToList();
        }

        public int Count => cards.Count;

        public IEnumerable<Card> All => cards;

        public void Add(Card card)
        {
            cards.Add(card);
            HighlightedCard = card;
        }

        public void AddMany(CardCollection cards)
        {
            this.cards.AddRange(cards.cards);
        }

        public Card AddNew(CardType cardType)
        {
            return AddNew(1, cardType).FirstOrDefault();
        }
        public IEnumerable<Card> AddNew(int count, CardType cardType)
        {
            return AddNew(count, () => new Card(cardType));
        }

        public IEnumerable<Card> AddNew(int count, Func<Card> createFunc)
        {
            var newCards = new List<Card>();
            for(int i = 0; i < count; i++)
            {
                var card = createFunc();
                card.Id = nextId++;
                newCards.Add(card);
            }
            cards.AddRange(newCards);
            return newCards;
        }

        public void InsertFromTop(Card card, int positionFromTop)
        {
            cards.Insert(cards.Count - positionFromTop, card);
        }

        public void Shuffle()
        {
            GameHelper.Shuffle(cards);
        }

        public Card DrawTop()
        {
            return DrawTop(1).Single();
        }

        public CardCollection DrawTop(int count)
        {
            var maxTake = Math.Min(count, cards.Count);
            var top = cards.TakeLast(maxTake).Reverse().ToList();
            cards.RemoveRange(cards.Count - maxTake, maxTake);
            UpdateHighlighted();
            return new CardCollection(top);
        }

        public Card PeekFromTop(CardType t)
        {
            return cards.LastOrDefault(x => x.Type == t);
        }

        public CardCollection PeekFromTop(int count)
        {
            var maxTake = Math.Min(count, cards.Count);
            var top = cards.TakeLast(maxTake).Reverse().ToList();
            return new CardCollection(top);
        }

        public Card DrawFromTop(CardType t)
        {
            for (int i = cards.Count-1; i >= 0; --i)
            {
                var card = cards[i];
                if (card.Type == t)
                {
                    cards.RemoveAt(i);
                    UpdateHighlighted();
                    return card;
                }
            }

            return null;
        }

        public void RemoveAll(IEnumerable<Card> cardsToRemove)
        {
            var cardsToRemoveList = cardsToRemove.ToList();
            var cardIds = new HashSet<int>(cardsToRemoveList.Select(card => card.Id));
            if (cardIds.Count != cardsToRemoveList.Count)
                throw new ArgumentException($"Cards to remove must have unique ids", nameof(cardsToRemove));
            int cardsRemoved = cards.RemoveAll(x => cardIds.Contains(x.Id));
            if (cardsRemoved != cardIds.Count)
            {
                throw new ArgumentException(
                    $"Could not remove all cards. Removed {cardsRemoved} cards. Expected {cardIds.Count}. Cards to remove {string.Join(", ", cardsToRemoveList)}. Collection {string.Join(", ", cards)}",
                    nameof(cardsToRemove));
            }

            UpdateHighlighted();
        }

        private void UpdateHighlighted()
        {
            if (HighlightedCard != null)
            {
                if (cards.All(x => x.Id != HighlightedCard.Id))
                    HighlightedCard = null;
            }
        }

        public void CloneNew(CardCollection dealPile)
        {
            foreach (Card card in dealPile)
            {
                AddNew(1, () => new Card(card.Type));
            }
        }

        public override string ToString()
        {
            return string.Join(", ", cards);
        }

        public bool Contains(CardType t) => cards.Any(x => x.Type == t);

        public IEnumerator<Card> GetEnumerator()
        {
            return ((IEnumerable<Card>)cards).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)cards).GetEnumerator();
        }

        public void TransferCardTo(Card card, CardCollection other)
        {
            if (card == null) return;
            RemoveAll(new[] { card });
            other.Add(card);
            HighlightedCard = card;
        }

        public void Clear()
        {
            cards.Clear();
        }
    }

}
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
        }

        public void AddMany(CardCollection cards)
        {
            this.cards.AddRange(cards.cards);
        }

        public void AddNew(int count, Func<Card> createFunc)
        {
            for(int i = 0; i < count; i++)
            {
                var card = createFunc();
                card.Id = nextId++;
                cards.Add(card);
            }
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
            return DrawTop(1).All.Single();
        }

        public CardCollection DrawTop(int count)
        {
            var maxTake = Math.Min(count, cards.Count);
            var top = cards.TakeLast(maxTake).Reverse().ToList();
            cards.RemoveRange(cards.Count - maxTake, maxTake);
            return new CardCollection(top);
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
                    return card;
                }
            }
            return null;
        }

        internal void RemoveAll(IEnumerable<Card> cardsToRemove)
        {
            var cardIds = new HashSet<int>(cardsToRemove.Select(card => card.Id));
            cards.RemoveAll(x => cardIds.Contains(x.Id));
        }

        public void CloneNew(CardCollection dealPile)
        {
            foreach (Card card in dealPile)
            {
                AddNew(1, () => new Card(card.Type));
            }
        }

        public Card PeekFromTop(CardType t)
        {
            return cards.Last(x => x.Type == t);
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

        internal void TransferCardTo(Card card, CardCollection other)
        {
            if (card == null) return;
            RemoveAll(new[] { card });
            other.Add(card);
        }
    }

}
namespace GameLogic
{
    public class CardCollection
    {
        private List<Card> cards = new List<Card>();

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
                cards.Add(createFunc());
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
        public T DrawFromTop<T>() where T : Card
        {
            for (int i = cards.Count-1; i >= 0; --i)
            {
                var card = cards[i];
                if (card is T)
                {
                    cards.RemoveAt(i);
                    return (T)card;
                }
            }
            throw new InvalidOperationException($"Could not find a card of type {typeof(T).Name}");
        }
        public T PeekFromTop<T>() where T : Card
        {
            return (T) cards.Last(x => x is T);
        }

        public override string ToString()
        {
            return string.Join(", ", cards);
        }

        public bool Contains<T>() => cards.OfType<T>().Any();

    }

}
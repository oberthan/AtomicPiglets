using System;

namespace GameLogic
{
    public class Player
    {
        public Player(string name) : this(name, Guid.NewGuid())
        {
        }
        public Player(string name, Guid id)
        {
            Name = name;
            Id = id;
        }

        public Guid Id;
        public string Name { get; set; }

        public bool IsGameOver;

        public CardCollection Hand { get; private set; } = new CardCollection();
        public CardCollection FutureCards { get; set; } = new CardCollection();

        public void Deal(CardCollection hand)
        {
            Hand = hand;
        }

        public void AddCard(Card card)
        {
            Hand.Add(card);
        }

        public string FormatHand()
        {
            return Hand.ToString();
        }

        public override string ToString()
        {
            return $"{Name} ({Id})";
        }
    }

}
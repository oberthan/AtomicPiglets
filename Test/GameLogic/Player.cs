namespace GameLogic
{
    public class Player
    {
        public Player(string name)
        {
            Name = name;
        }

        public CardCollection Hand { get; private set; } = new CardCollection();
        public string Name { get; set; }

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
            return Name;
        }
    }

}
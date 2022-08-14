using System;
using Assets.Dto;
using Newtonsoft.Json;

namespace GameLogic
{
    public class Player
    {
        private Player() {}
        public Player(string name) : this(name, Guid.NewGuid())
        {
        }
        public Player(PlayerInfo playerInfo) : this(playerInfo.PlayerName, playerInfo.Id)
        {}
        public Player(string name, Guid id)
        {
            Name = name;
            Id = id;
        }

        [JsonProperty]
        public Guid Id;

        [JsonProperty]
        public string Name { get; set; }

        public bool IsGameOver()
        {
            return Hand.Contains(CardType.AtomicPigletCard) && !Hand.Contains(CardType.DefuseCard);
        }

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
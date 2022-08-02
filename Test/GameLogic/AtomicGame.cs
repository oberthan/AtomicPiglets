﻿namespace GameLogic
{
    public class AtomicGame
    {
        public AtomicGame(CardDeck deck, List<Player> players)
        {
            Deck = deck;
            Players = players.ToList();
            GameHelper.Shuffle(Players);
            CurrentPlayer = players[0];
            PlayerRounds = 1;
        }

        public CardDeck Deck { get; }

        public List<Player> Players { get; }

        public Player CurrentPlayer { get; private set; }

        public int PlayerRounds;

        /// <summary>
        /// Selects next player as current player cyclic.
        /// </summary>
        public void NextPlayer()
        {
            var playerCount = Players.Count;

            var currentPlayerIndex = Players.IndexOf(CurrentPlayer);
            currentPlayerIndex = (currentPlayerIndex + 1) % playerCount;
            CurrentPlayer = Players[currentPlayerIndex];
            
        }
    }

}
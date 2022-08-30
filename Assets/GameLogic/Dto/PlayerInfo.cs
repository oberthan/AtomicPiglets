using System;

namespace Assets.Dto
{
    public class PlayerInfo
    {
        public Guid Id;
        public string PlayerName;
        public bool IsReady;
        
        // In game only
        public int CardsLeft;
        public bool IsGameOver;

        public override string ToString()
        {
            return $"{PlayerName}:{Id}";
        }
    }
}
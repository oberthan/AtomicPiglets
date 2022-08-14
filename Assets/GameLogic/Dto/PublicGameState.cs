using System.Linq;
using GameLogic;

namespace Assets.Dto
{
    public class PublicGameState
    {
        public PlayerInfo CurrentPlayer;
        public PlayerInfo[] AllPlayers;
        public CardCollection PlayPile;
        public CardCollection DiscardPile;
        public int DeckCardsLeft;
        public int TurnsLeft;
        public string PublicMessage;

        public static PublicGameState FromAtomicGame(AtomicGame game)
        {
            return new PublicGameState
            {
                CurrentPlayer = PlayerGameState.PlayerInfoFromPlayer(game.CurrentPlayer),
                AllPlayers = game.Players.Select(PlayerGameState.PlayerInfoFromPlayer).ToArray(),
                PlayPile = game.PlayPile,
                DiscardPile = game.DiscardPile,
                DeckCardsLeft = game.Deck.Count,
                TurnsLeft = game.PlayerTurns,
                PublicMessage = game.PublicMessage
            };
        }

    }
}
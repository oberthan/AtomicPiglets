using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Assets.Bots;
using Assets.Dto;
using GameLogic;

namespace Assets.GameLogic.Bots
{
    public class FoxBot : IAtomicPigletBot
    {
        private readonly Random _rnd = new();
        public PlayerInfo PlayerInfo { get; }

        public Guid PlayerId => PlayerInfo.Id;

        public FoxBot()
        {
            PlayerInfo = new PlayerInfo { PlayerName = "Fox bot", Id = Guid.NewGuid() };
        }

        private bool firstRound = true;
        public IGameAction GetAction(AtomicPigletRules rules, PlayerGameState playerState, PublicGameState publicState)
        {
            if (firstRound)
            {
                InitializeStatistics(rules.Game);
            }

            var player = rules.Game.GetPlayer(PlayerId);
            if (rules.Game.CurrentPlayer.Id != PlayerId)
            {
                return new NoAction(player);
            }
            var actions = rules.GetLegalActionsForPlayer(PlayerId).ToList();
            return actions[_rnd.Next(actions.Count)];
        }

        private void InitializeStatistics(AtomicGame game)
        {
            // Create duplicate of all cards game of same number of players.
            var playersCount = game.Players.Count;
            var allCards = GameFactory.CreateExplodingKittensLikeGame(playersCount).GetAllCards().Select(x => x.Type).ToList();

            var allCardTypeCounts = allCards.GroupBy(x => x).ToDictionary(x => x, x => x.Count());

            var myPlayer = GetMyPlayer(game);
            
            var ownCardTypes = myPlayer.Hand.Select(x => x.Type).ToList();


            // Unknowns: All -MyCards -DiscardCards -PlayedCards

            var deckCount = game.Deck.Count;
            var playCount = game.PlayPile.Count;
            var discardCount = game.DiscardPile.Count;
            var playersCardCount = game.Players.Sum(x => x.Hand.Count);

        }

        private Player GetMyPlayer(AtomicGame game)
        {
            return game.Players.First(x => x.Id == PlayerId);
        }

        public class CardProbability
        {
            public double P { get; }
            public CardType Type { get; }

            public CardProbability(double p, CardType type)
            {
                P = p;
                Type = type;
            }

            public static CardProbability Certain(CardType type)
            {
                return new CardProbability(1, type);
            }
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public interface IGameAction
    {
        void Execute(AtomicGame game);

        string FormatShort();
    }
    public class DrawFromDeckAction : IGameAction
    {
        public Guid PlayerId;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private DrawFromDeckAction() { }
        public DrawFromDeckAction(Player player)
        {
            PlayerId = player.Id;
        }

        public void Execute(AtomicGame game)
        {
            var card = game.Deck.DrawTop();
            var player = game.GetPlayer(PlayerId);
            player.AddCard(card);
            game.EndTurn();
        }

        public string FormatShort()
        {
            return "Draw from deck";
        }
    }

    public interface ICardAction : IGameAction
    {
        Guid PlayerId { get; }
        IEnumerable<Card> Cards { get; }
    }

    public class DefuseAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card defuseCard;
        [JsonProperty]
        private Card atomicPigletCard;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private DefuseAction() { }

        public DefuseAction(Player player, Card defuseCard, Card atomicPigletCard)
        {
            PlayerId = player.Id;
            this.defuseCard = defuseCard;
            this.atomicPigletCard = atomicPigletCard;
        }

        public IEnumerable<Card> Cards => new[] { defuseCard };

        public int AtomicPositionFromTop { get; set; }

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
            game.Deck.InsertFromTop(atomicPigletCard, AtomicPositionFromTop);
        }

        public string FormatShort()
        {
            return "Defuse";
        }
    }
    public class SkipAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card card;

        /// <summary>
        /// Deserialize constructor.
        /// </summary>
        private SkipAction() { }

        public SkipAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.EndTurn();
        }

        public string FormatShort()
        {
            return "Skip";
        }
    }

    public class AttackAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]

        private Card card;
        private AttackAction() { }
        public AttackAction(Player player, Card card)
        {
            if (card.Id == 0) throw new ArgumentException("Card should have an id");
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.NextPlayer();
            if (game.PlayerTurns == 1)
            {
                game.PlayerTurns += 1;
            }
            else
            {
                game.PlayerTurns += 2;
            }
        }

        public string FormatShort()
        {
            return "Attack!";
        }

    }

    public class ShuffleAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]

        private Card card;
        private ShuffleAction() { }
        public ShuffleAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
            game.Deck.Shuffle();
        }

        public string FormatShort()
        {
            return "Shuffle";
        }

    }

    public class SeeTheFutureAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]

        private Card card;
        private SeeTheFutureAction() { }
        public SeeTheFutureAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public CardCollection FutureCards { get; private set; }

        public void Execute(AtomicGame game)
        {
            FutureCards = game.Deck.PeekFromTop(3);
        }

        public string FormatShort()
        {
            return "See the future";
        }

    }
    public class FavorAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [JsonProperty]
        private Card card;

        private FavorAction() { }
        public FavorAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public Guid TargetPlayerId { get; set; }

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            var otherCard = GameHelper.SelectRandomCard(otherPlayer.Hand);

            otherPlayer.Hand.TransferCardTo(otherCard, player.Hand);
        }

        public string FormatShort()
        {
            return "Favor";
        }

    }

    public class NopeAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card card;
        private NopeAction() { }
        public NopeAction(Player player, Card card)
        {
            PlayerId = player.Id;
            this.card = card;
        }

        public IEnumerable<Card> Cards => new[] { card };

        public void Execute(AtomicGame game)
        {
        }

        public string FormatShort()
        {
            return "Nope";
        }

    }

    public class DrawFromPlayerAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card[] cards;
        private DrawFromPlayerAction() { }
        public DrawFromPlayerAction(Player player, List<IEnumerable<Card>> cards)
        {
            PlayerId = player.Id;
            this.cards = cards.First().ToArray();
        }

        public IEnumerable<Card> Cards => cards;

        public Guid TargetPlayerId { get; set; }

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            var card = GameHelper.SelectRandomCard(otherPlayer.Hand);

            otherPlayer.Hand.TransferCardTo(card, player.Hand);
        }

        public string FormatShort()
        {
            return "Draw from player";
        }
    }

    public class DemandCardFromPlayerAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card[] cards;
        private DemandCardFromPlayerAction() { }
        public DemandCardFromPlayerAction(Player player, List<IEnumerable<Card>> cards)
        {
            PlayerId = player.Id;
            this.cards = cards.First().ToArray();
        }

        public IEnumerable<Card> Cards => cards;
        public Guid TargetPlayerId { get; set; }
        public CardType CardType { get; set; }

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            var otherCard =
                CardType == CardType.NoCard // Default action
                    ? GameHelper.SelectRandomCard(otherPlayer.Hand)
                    : otherPlayer.Hand.DrawFromTop(CardType);

            otherPlayer.Hand.TransferCardTo(otherCard, player.Hand);
        }

        public string FormatShort()
        {
            return "Demand card";
        }
    }

    public class DrawFromDiscardPileAction : ICardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        [JsonProperty]
        private Card[] cards;
        private DrawFromDiscardPileAction() { }
        public DrawFromDiscardPileAction(Player player, Card[] cards)
        {
            PlayerId = player.Id;
            this.cards = cards.Take(5).ToArray();
        }

        public IEnumerable<Card> Cards => cards;
        public CardType CardType { get; set; }

        public void Execute(AtomicGame game)
        {
            if (!game.DiscardPile.Any()) return;

            var player = game.GetPlayer(PlayerId);
            
            var card =                 
                (CardType == CardType.NoCard) // Default action
                    ? GameHelper.SelectRandomCard(game.DiscardPile)
                    : game.DiscardPile.DrawFromTop(CardType);
            game.DiscardPile.TransferCardTo(card, player.Hand);
        }

        public string FormatShort()
        {
            return "Draw from discards";
        }
    }

    public class GameOverAction : IGameAction
    {
        private readonly Player player;

        public GameOverAction(Player player)
        {
            this.player = player;
        }

        public void Execute(AtomicGame game)
        {
           // throw new NotImplementedException();
        }
        public string FormatShort()
        {
            return "Game over";
        }

    }
}
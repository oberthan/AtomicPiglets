﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic
{
    public interface IGameAction
    {
        Guid PlayerId { get; }

        void Execute(AtomicGame game);

        string FormatShort();
    }

    public class DrawFromDeckAction : IGameAction, IDrawCardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

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
            DrawCard = game.Deck.DrawTop();
            var player = game.GetPlayer(PlayerId);
            player.AddCard(DrawCard);
            if (DrawCard.Type == CardType.AtomicPigletCard)
            {
                if (player.IsGameOver())
                {
                    game.PlayAction(new GameOverAction(player));
                    game.PlayerTurns = 1;
                    game.NextPlayer();
                }
            }
            else
                game.EndTurn();
        }

        public string FormatShort()
        {
            return "Draw from deck";
        }

        [JsonProperty] public Card DrawCard { get; private set; }
    }

    public interface ICardAction : IGameAction
    {
        IEnumerable<Card> Cards { get; }
        float PlayDelay { get; }
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
        public float PlayDelay => 0;

        public int AtomicPositionFromTop { get; set; }

        public void Execute(AtomicGame game)
        {
            game.GetPlayer(PlayerId).Hand.RemoveAll(new[] {atomicPigletCard});
            var position = AtomicPositionFromTop;
            if (position < 0) position = 0;
            if (position > game.Deck.Count) position = game.Deck.Count;
                game.Deck.InsertFromTop(atomicPigletCard, position);
            game.EndTurn();
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
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

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
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

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
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

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
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

        public void Execute(AtomicGame game)
        {
            game.CurrentPlayer.FutureCards = game.Deck.PeekFromTop(3);
        }

        public string FormatShort()
        {
            return "See the future";
        }

    }

    public interface ITargetGameAction
    {
        Guid TargetPlayerId { get; }
    }

    public interface IDrawCardAction
    {
        Card DrawCard { get; }
    }


    public class FavorAction : ICardAction, ITargetGameAction, IDrawCardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        [JsonProperty]
        private Card card;


        private FavorAction() { }
        public FavorAction(Player player, Card card, Player targetPlayer)
        {
            PlayerId = player.Id;
            this.card = card;
            TargetPlayer = targetPlayer;
        }

        public IEnumerable<Card> Cards => new[] { card };
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;
        [JsonProperty] public Player TargetPlayer;
        [JsonProperty] public Card DrawCard { get; private set; }

        public Guid TargetPlayerId => TargetPlayer.Id;

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectFromRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            DrawCard = GameHelper.OrderByPriority(otherPlayer.Hand).Reverse().FirstOrDefault();

            otherPlayer.Hand.TransferCardTo(DrawCard, player.Hand);
        }

        public string FormatShort()
        {
            return $"Favor from {TargetPlayer.Name}";
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
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

        public void Execute(AtomicGame game)
        {
            game.DiscardTopPlayCards();
        }

        public string FormatShort()
        {
            return "Nope";
        }

    }

    public interface ISelectableCardsActions
    {
        Card[] SelectedCards { get; set; }

        Card[] SelectableCards { get; }
    }

    public class DrawFromPlayerAction : ICardAction, ITargetGameAction, ISelectableCardsActions, IDrawCardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        /// <summary>
        /// Two cards of same type.
        /// </summary>
        [JsonProperty]
        public Card[] SelectedCards { get; set; }

        /// <summary>
        /// All pair-cards of same type in player hand.
        /// </summary>
        [JsonProperty] public Card[] SelectableCards { get; private set; }
        private DrawFromPlayerAction() { }
        public DrawFromPlayerAction(Player player, List<IEnumerable<Card>> cards, Player targetPlayer)
        {
            PlayerId = player.Id;
            SelectableCards = cards.SelectMany(x => x).ToArray();
            SelectedCards = cards.OrderByList(x => x.First().Type, GameHelper.GetCardTypePriorityList()).Last().ToArray();
            TargetPlayer = targetPlayer;
        }

        public IEnumerable<Card> Cards => SelectedCards;
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;

        [JsonProperty] public Player TargetPlayer;
        public Guid TargetPlayerId => TargetPlayer.Id;
        [JsonProperty] public Card DrawCard { get; private set; }

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectFromRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            DrawCard = GameHelper.SelectRandomCard(otherPlayer.Hand);

            otherPlayer.Hand.TransferCardTo(DrawCard, player.Hand);
        }

        public string FormatShort()
        {
            return $"Draw from {TargetPlayer.Name} with 2x {SelectedCards.First()}";
        }
    }

    public class DemandCardFromPlayerAction : ICardAction, ITargetGameAction, ISelectableCardsActions, IDrawCardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        /// <summary>
        /// Three cards of same type.
        /// </summary>
        [JsonProperty]
        public Card[] SelectedCards { get; set; }

        /// <summary>
        /// All triple-cards of same type in player hand.
        /// </summary>
        [JsonProperty] public Card[] SelectableCards { get; private set; }

        private DemandCardFromPlayerAction() { }
        public DemandCardFromPlayerAction(Player player, List<IEnumerable<Card>> cards, Player targetPlayer)
        {
            PlayerId = player.Id;
            SelectableCards = cards.SelectMany(x => x).ToArray();
            SelectedCards = cards.OrderByList(x => x.First().Type, GameHelper.GetCardTypePriorityList()).Last().ToArray();
            TargetPlayer = targetPlayer;
        }

        public IEnumerable<Card> Cards => SelectedCards;
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;
        [JsonProperty] public Player TargetPlayer;
        public Guid TargetPlayerId => TargetPlayer.Id;
        [JsonProperty] public Card DrawCard { get; private set; }
        public CardType CardType { get; set; } = CardType.DefuseCard;

        public void Execute(AtomicGame game)
        {
            var player = game.GetPlayer(PlayerId);

            var otherPlayer = TargetPlayerId == Guid.Empty
                ? GameHelper.SelectFromRandomOtherPlayer(game, PlayerId)
                : game.GetPlayer(TargetPlayerId);

            DrawCard =
                CardType == CardType.NoCard // Default action
                    ? GameHelper.SelectRandomCard(otherPlayer.Hand)
                    : otherPlayer.Hand.PeekFromTop(CardType);

            otherPlayer.Hand.TransferCardTo(DrawCard, player.Hand);
        }

        public string FormatShort()
        {
            return $"Demand from {TargetPlayer.Name} with 3x {SelectedCards.First().Type}";
        }
    }

    public class DrawFromDiscardPileAction : ICardAction, ISelectableCardsActions, IDrawCardAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }
        /// <summary>
        /// Three cards of same type.
        /// </summary>
        [JsonProperty]
        public Card[] SelectedCards { get; set; }

        /// <summary>
        /// All triple-cards of same type in player hand.
        /// </summary>
        [JsonProperty] public Card[] SelectableCards { get; private set; }

        private DrawFromDiscardPileAction() { }
        public DrawFromDiscardPileAction(Player player, Card[] cards)
        {
            PlayerId = player.Id;
            SelectableCards = cards.ToArray();
            SelectedCards = GameHelper.OrderByPriority(cards).Reverse().Take(5).ToArray();
        }

        public IEnumerable<Card> Cards => SelectedCards;
        public float PlayDelay => AtomicPigletRules.DefaultPlayDelay;
        public CardType CardType { get; set; }

        public void Execute(AtomicGame game)
        {
            if (!game.DiscardPile.Any()) return;

            var player = game.GetPlayer(PlayerId);
            
            DrawCard =                 
                (CardType == CardType.NoCard) // Default action
                    ? GameHelper.OrderByPriority(game.DiscardPile).FirstOrDefault()
                    : game.DiscardPile.DrawFromTop(CardType);
            game.DiscardPile.TransferCardTo(DrawCard, player.Hand);
        }

        public string FormatShort()
        {
            return "Draw from discards";
        }

        [JsonProperty] public Card DrawCard { get; private set; }
    }

    public class GameOverAction : IGameAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        private GameOverAction(){}
        public GameOverAction(Player player)
        {
            PlayerId = player.Id;
        }

        public void Execute(AtomicGame game)
        {
           // No action. Game over!
        }
        public string FormatShort()
        {
            return "Game over";
        }
    }
    public class WinGameAction : IGameAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        private WinGameAction() {}
        public WinGameAction(Player player)
        {
            PlayerId = player.Id;
        }

        public void Execute(AtomicGame game)
        {
        }
        public string FormatShort()
        {
            return "You won!";
        }
    }

    public class NoAction : IGameAction
    {
        [JsonProperty]
        public Guid PlayerId { get; private set; }

        private NoAction(){}

        public NoAction(Player player)
        {
            PlayerId = player.Id;
        }
        public void Execute(AtomicGame game)
        {
            // Do nothing
        }

        public string FormatShort()
        {
            return "No action";
        }
    }

}
namespace GameLogic
{
    public class AtomicPigletRules
    {
        private readonly AtomicGame game;

        public AtomicPigletRules(AtomicGame game)
        {
            this.game = game;
        }
        public IEnumerable<IGameAction> GetLegalActionsForPlayer(Player player)
        {
            if (player == game.CurrentPlayer)
            {
                var hand = player.Hand;

                // Players has drawn atomic piglet and must deal with it.
                if (hand.Contains<AtomicPigletCard>())
                {
                    if (hand.Contains<DefuseCard>())
                        yield return new DefuseAction(player, hand.PeekFromTop<DefuseCard>(), hand.DrawFromTop<AtomicPigletCard>());
                    else
                        yield return new GameOverAction(player);
                }
                else // Player has NOT just drawn atomic piglet
                {
                    // Player can end this turn by drawing from deck.
                    yield return new DrawFromDeckAction(player);

                    if (hand.Contains<SkipCard>())
                        yield return new SkipAction(player, hand.PeekFromTop<SkipCard>());

                }
            }
            yield break;
        }
    }

}
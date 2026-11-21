namespace WildernessGameplay
{
    // Negotiates initially, but leaves if the player counters its own counteroffer.
    public sealed class Impatient : Trader
    {
        public override TraderMood Mood => TraderMood.Impatient;
        public override TradeDecision Evaluate(Trade offer, Player player, System.Random random) =>
            !PlayerAccepted && IsCounterOffer ? TradeDecision.REJECT : base.Evaluate(offer, player, random);
    }
}

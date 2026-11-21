namespace WildernessGameplay
{
    // Has a fifty-percent chance to change the price whenever the player responds.
    public sealed class Indecisive : Trader
    {
        public override TraderMood Mood => TraderMood.Indecisive;
        public override bool HonorsAcceptanceImmediately => false;
        public override TradeDecision Evaluate(Trade offer, Player player, System.Random random)
        {
            if (random.NextDouble() < .5) return TradeDecision.COUNTER;
            return base.Evaluate(offer, player, random) == TradeDecision.ACCEPT ? TradeDecision.ACCEPT : TradeDecision.REJECT;
        }
        public override void CounterOffer(System.Random random)
        { CounterPrice = random.Next(1, OfferAmount * 2 + 2); IsCounterOffer = true; }
    }
}

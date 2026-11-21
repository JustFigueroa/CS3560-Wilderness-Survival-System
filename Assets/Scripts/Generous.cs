namespace WildernessGameplay
{
    // Offers twice the normal bundle for one gold and accepts any payable positive counteroffer.
    public sealed class Generous : Trader
    {
        public override TraderMood Mood => TraderMood.Generous;
        protected override int InitialAmount(DifficultyPreset rules, System.Random random) => rules.tradeAmount * 2;
        protected override int InitialPrice() => 1;
        protected override int MinimumPrice => 1;
    }
}

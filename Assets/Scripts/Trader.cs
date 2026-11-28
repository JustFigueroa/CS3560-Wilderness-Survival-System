using System;

namespace WildernessGameplay
{
    // Owns negotiation state. Subclasses specialize the price and response rules.
    public abstract class Trader : Item
    {
        public abstract TraderMood Mood { get; }
        // Pending presentation state keeps player and trader messages on separate Next actions.
        public TraderAction NextAction;
        public bool PlayerAccepted;
        public Trade PlayerOffer;
        public int PlayerPrice => PlayerOffer?.OfferedGold ?? 0;
        public int Negotiations, CounterPrice, OfferAmount;
        public bool OfferPending, IsCounterOffer;
        public SupplyKind CounterSupply;
        public virtual bool HonorsAcceptanceImmediately => true;
        // Entering a trader square schedules an offer, even if the player cannot use or afford it.
        public void Interact(Player player, int turn) { OfferPending = false; NextAction = TraderAction.Offer; }
        // Offer the more-needed supply. Normal food and water bundles each roll 3-6 per encounter.
        public void PresentOffer(Player player, DifficultyPreset rules, Random random)
        {
            NextAction = TraderAction.None;
            Negotiations = 0;
            IsCounterOffer = false;
            CounterSupply = player.currentFood / (double)player.maxFood <= player.currentWater / (double)player.maxWater ? SupplyKind.Food : SupplyKind.Water;
            OfferAmount = InitialAmount(rules, random);
            CounterPrice = InitialPrice();
            OfferPending = true;
        }
        protected virtual int InitialAmount(DifficultyPreset rules, Random random) => random.Next(3, 7);
        protected virtual int InitialPrice() => OfferAmount * 2;
        protected virtual int MinimumPrice => OfferAmount;
        // Accept a payable offer worth at least one gold per resource, or honor an already accepted asking price.
        public virtual TradeDecision Evaluate(Trade offer, Player player, Random random) =>
            offer.OfferedGold > 0 && offer.CanApply(player) && (PlayerAccepted || offer.OfferedGold >= MinimumPrice)
                ? TradeDecision.ACCEPT : TradeDecision.COUNTER;
        // Normal traders reduce the asking price by one; Indecisive overrides this with a new random price.
        public virtual void CounterOffer(Random random) { CounterPrice = Math.Max(1, CounterPrice - 1); IsCounterOffer = true; }
        public static Trader Create(TraderMood mood)
        {
            switch (mood)
            {
                case TraderMood.Patient: return new Patient();
                case TraderMood.Impatient: return new Impatient();
                case TraderMood.Generous: return new Generous();
                case TraderMood.Indecisive: return new Indecisive();
                default: throw new ArgumentOutOfRangeException(nameof(mood));
            }
        }
    }
}

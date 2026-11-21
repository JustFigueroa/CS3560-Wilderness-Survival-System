namespace WildernessGameplay
{
    // Keeps countering until a payable offer meets its minimum price or the player leaves.
    public sealed class Patient : Trader { public override TraderMood Mood => TraderMood.Patient; }
}

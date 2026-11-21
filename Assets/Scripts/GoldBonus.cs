namespace WildernessGameplay
{
    // Collectible gold; unlike food/water, the player has no gold capacity limit.
    public sealed class GoldBonus : Bonus
    {
        protected override int ApplyBonus(Player player)
        {
            int amount = Amount;
            player.currentGold += amount;
            return amount;
        }
    }
}

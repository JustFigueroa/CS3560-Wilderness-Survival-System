namespace WildernessGameplay
{
    // Collectible water; collection stops at the player's maximum water.
    public sealed class WaterBonus : Bonus
    {
        protected override int ApplyBonus(Player player)
        {
            int amount = System.Math.Min(Amount, player.maxWater - player.currentWater);
            player.currentWater += amount;
            return amount;
        }
    }
}

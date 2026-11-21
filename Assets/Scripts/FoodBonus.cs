namespace WildernessGameplay
{
    // Collectible food; collection stops at the player's maximum food.
    public sealed class FoodBonus : Bonus
    {
        protected override int ApplyBonus(Player player)
        {
            int amount = System.Math.Min(Amount, player.maxFood - player.currentFood);
            player.currentFood += amount;
            return amount;
        }
    }
}

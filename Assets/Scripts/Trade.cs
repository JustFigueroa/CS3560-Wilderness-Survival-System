namespace WildernessGameplay
{
    // Values are from the player's perspective. Current offers exchange gold for one supply.
    public sealed class Trade
    {
        public int OfferedGold, OfferedWater, OfferedFood;
        public int RequestedGold, RequestedWater, RequestedFood;
        public static Trade Purchase(SupplyKind supply, int amount, int price) => new Trade {
            OfferedGold = price,
            RequestedFood = supply == SupplyKind.Food ? amount : 0,
            RequestedWater = supply == SupplyKind.Water ? amount : 0
        };
        public bool CanApply(Player player) =>
            OfferedGold >= 0 && OfferedFood >= 0 && OfferedWater >= 0 &&
            RequestedGold >= 0 && RequestedFood >= 0 && RequestedWater >= 0 &&
            player.currentGold >= OfferedGold && player.currentFood >= OfferedFood && player.currentWater >= OfferedWater &&
            player.currentFood - OfferedFood + RequestedFood <= player.maxFood &&
            player.currentWater - OfferedWater + RequestedWater <= player.maxWater;
    }
}

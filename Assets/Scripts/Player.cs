using System;

namespace WildernessGameplay
{
    // Owns current resources and resource limits; applies movement, rest, collection, and trades.
    public sealed class Player
    {
        // Grid position and the original diagram resource names. These values are runtime data, not Inspector settings.
        public int X, Y;
        public int currentStrength, currentFood, currentWater, currentGold;
        public int maxStrength, maxFood, maxWater;
        public Brain Brain;
        public Vision Vision;
        public Square OccupiedSquare { get; private set; }
        // Keep the occupied square and terrain-selected vision synchronized after an action.
        public void UpdateVision(Square square) { OccupiedSquare = square; Vision = square.TerrainType.CreateVision(); }
        // Check an adjacent move against available resources. Game also guards against acting after game over.
        public bool CanMove(Square tile) => tile != null &&
            Math.Abs(tile.X - X) <= 1 && Math.Abs(tile.Y - Y) <= 1 && (tile.X != X || tile.Y != Y) &&
            currentStrength >= tile.StrengthCost && currentFood >= tile.FoodCost && currentWater >= tile.WaterCost;
        // Apply a move already approved by Game; collection and turn accounting are coordinated separately.
        public void Move(Square tile)
        {
            currentStrength -= tile.StrengthCost;
            currentFood -= tile.FoodCost;
            currentWater -= tile.WaterCost;
            X = tile.X;
            Y = tile.Y;
            OccupiedSquare = tile;
        }
        // Restore up to the strength limit and return the actual recovered amount.
        public int Rest(int amount)
        {
            int recovered = Math.Min(maxStrength - currentStrength, amount);
            currentStrength += recovered;
            return recovered;
        }
        public void InteractWith(Item item, int turn) => item.Interact(this, turn);
        // Interact with the bonus items once on entry; remaining resources stay on the square.
        public void Collect(Square tile, int turn)
        {
            if (tile.Food == 0 && tile.Water == 0 && tile.Gold == 0) return;
            foreach (var item in tile.Items) if (item is Bonus) InteractWith(item, turn);
            // Preserve the existing shared pickup timer for this tile's three bonuses.
            tile.LastCollectedTurn = turn;
        }
        // Counter with one gold below the current price, capped at the player's available gold.
        public Trade ProposeTrade(Trader trader, bool accept) => Trade.Purchase(trader.CounterSupply,
            trader.OfferAmount, accept ? trader.CounterPrice : Math.Min(currentGold, Math.Max(1, trader.CounterPrice - 1)));
        // Preserve the current rule: after one counteroffer, any affordable offer may be accepted.
        public TradeDecision EvaluateTrade(Trader trader, DifficultyPreset rules)
        {
            int room = trader.CounterSupply == SupplyKind.Food ? maxFood - currentFood : maxWater - currentWater;
            // Negotiating is free. Check survival after receiving the offered supply and paying completion upkeep.
            int foodAfter = currentFood + (trader.CounterSupply == SupplyKind.Food ? trader.OfferAmount : 0) - rules.actionFoodCost;
            int waterAfter = currentWater + (trader.CounterSupply == SupplyKind.Water ? trader.OfferAmount : 0) - rules.actionWaterCost;
            if (room < trader.OfferAmount || currentGold == 0 || foodAfter <= 0 || waterAfter <= 0)
                return TradeDecision.REJECT;
            bool affordable = currentGold >= trader.CounterPrice;
            bool fair = trader.CounterPrice <= trader.OfferAmount;
            return affordable && (fair || trader.Negotiations > 0) ? TradeDecision.ACCEPT : TradeDecision.COUNTER;
        }
        // Validate player affordability and capacity before applying an exchange; traders have unlimited supplies.
        public bool ApplyTrade(Trade offer)
        {
            if (!offer.CanApply(this)) return false;
            currentGold += offer.RequestedGold - offer.OfferedGold;
            currentFood += offer.RequestedFood - offer.OfferedFood;
            currentWater += offer.RequestedWater - offer.OfferedWater;
            return true;
        }
    }
}

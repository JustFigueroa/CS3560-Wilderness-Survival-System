using System.Collections.Generic;

namespace WildernessGameplay
{
    // One map cell. Terrain owns its entry costs; bonus items and the trader own its contents.
    public sealed class Square
    {
        public int X, Y;
        public Terrain TerrainType = new Plains();
        public readonly FoodBonus FoodBonus = new FoodBonus();
        public readonly WaterBonus WaterBonus = new WaterBonus();
        public readonly GoldBonus GoldBonus = new GoldBonus();
        public readonly List<Item> Items;
        private Trader trader;
        public Square() { Items = new List<Item> { FoodBonus, WaterBonus, GoldBonus }; }
        public void AddItem(Item item) { if (item != null && !Items.Contains(item)) Items.Add(item); }
        public Trader Trader
        {
            get => trader;
            set { if (trader != null) Items.Remove(trader); trader = value; AddItem(value); }
        }
        // Convenience accessors keep rendering independent of the item hierarchy.
        public TerrainKind Terrain
        {
            get => TerrainType.Kind;
            set
            {
                var replacement = WildernessGameplay.Terrain.Create(value);
                replacement.MovementCost = StrengthCost;
                replacement.FoodCost = FoodCost;
                replacement.WaterCost = WaterCost;
                TerrainType = replacement;
            }
        }
        public int StrengthCost { get => TerrainType.MovementCost; set => TerrainType.MovementCost = value; }
        public int FoodCost { get => TerrainType.FoodCost; set => TerrainType.FoodCost = value; }
        public int WaterCost { get => TerrainType.WaterCost; set => TerrainType.WaterCost = value; }
        public int Food { get => FoodBonus.Amount; set => FoodBonus.Amount = value; }
        public int Water { get => WaterBonus.Amount; set => WaterBonus.Amount = value; }
        public int Gold { get => GoldBonus.Amount; set => GoldBonus.Amount = value; }
        public int InitialFood { get => FoodBonus.InitialAmount; set => FoodBonus.InitialAmount = value; }
        public int InitialWater { get => WaterBonus.InitialAmount; set => WaterBonus.InitialAmount = value; }
        public int InitialGold { get => GoldBonus.InitialAmount; set => GoldBonus.InitialAmount = value; }
        public int LastCollectedTurn
        {
            get => FoodBonus.LastCollectedTurn;
            set { FoodBonus.LastCollectedTurn = WaterBonus.LastCollectedTurn = GoldBonus.LastCollectedTurn = value; }
        }
    }
}

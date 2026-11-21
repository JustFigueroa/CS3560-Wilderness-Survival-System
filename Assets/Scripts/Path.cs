using System.Collections.Generic;

namespace WildernessGameplay
{
    // A shortest visible route. Steps are entered tiles; the starting tile is excluded.
    public sealed class Path
    {
        public readonly List<Square> Steps = new List<Square>();
        public readonly List<Direction> Moves = new List<Direction>();
        public Square Destination => Steps[Steps.Count - 1];
        public int MovementCost, FoodCost, WaterCost;
        public bool Survivable;
        public int FoodAfter, WaterAfter;
    }
}

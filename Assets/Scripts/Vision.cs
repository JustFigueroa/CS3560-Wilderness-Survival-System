using System;
using System.Collections.Generic;

namespace WildernessGameplay
{
    // Defines a visibility shape and exposes the single shared search for visible resource/trader paths.
    // The player keeps one vision type for the whole game; terrain only adjusts its range.
    public abstract class Vision
    {
        public const int MinimumRadius = 1;
        public abstract VisionMode Mode { get; }
        // Range before terrain is applied.
        public abstract int BaseRadius { get; }
        // Set from the occupied square's terrain each time vision refreshes.
        public int TerrainModifier { get; private set; }
        // Effective range; never drops below the adjacent squares.
        public int Radius => Math.Max(MinimumRadius, BaseRadius + TerrainModifier);
        protected abstract bool Contains(int dx, int dy);
        private VisiblePaths paths;
        public void ApplyTerrain(Terrain terrain) => TerrainModifier = terrain.VisionModifier;
        public static Vision Create(VisionMode mode)
        {
            switch (mode)
            {
                case VisionMode.KeenEyed: return new KeenEyed();
                case VisionMode.Cautious: return new Cautious();
                case VisionMode.Focused: return new Focused();
                case VisionMode.FarSight: return new FarSight();
                default: throw new ArgumentOutOfRangeException(nameof(mode));
            }
        }
        // Pick one of the vision types uniformly from the game's seeded random sequence.
        public static Vision CreateRandom(Random random)
        {
            var modes = (VisionMode[])Enum.GetValues(typeof(VisionMode));
            return Create(modes[random.Next(modes.Length)]);
        }
        // Yield only in-bounds squares inside this vision's shape.
        public IEnumerable<Square> GetVisibleSquares(Map map, Square location)
        {
            for (int x = Math.Max(0, location.X - Radius); x <= Math.Min(map.Width - 1, location.X + Radius); x++)
                for (int y = Math.Max(0, location.Y - Radius); y <= Math.Min(map.Height - 1, location.Y + Radius); y++)
                    if (Contains(x - location.X, y - location.Y)) yield return map.GetSquare(x, y);
        }
        // Refresh the cached paths before each brain decision. Closest queries return null when absent.
        public VisiblePaths FindPaths(Game game) => paths = new VisiblePaths(game);
        public Path ClosestFood() => paths?.ClosestFood;
        public Path SecondClosestFood() => paths?.SecondClosestFood;
        public Path ClosestWater() => paths?.ClosestWater;
        public Path SecondClosestWater() => paths?.SecondClosestWater;
        public Path ClosestGold() => paths?.ClosestGold;
        public Path SecondClosestGold() => paths?.SecondClosestGold;
        public Path ClosestTrader() => paths?.ClosestTrader;
        public Path SecondClosestTrader() => paths?.SecondClosestTrader;
        public Path EasiestPath(IEnumerable<Path> candidates) => VisiblePaths.Easiest(candidates);
    }
}

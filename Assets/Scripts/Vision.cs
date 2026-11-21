using System;
using System.Collections.Generic;

namespace WildernessGameplay
{
    // Defines a visibility shape and exposes the single shared search for visible resource/trader paths.
    public abstract class Vision
    {
        public abstract VisionMode Mode { get; }
        protected abstract int Radius { get; }
        protected abstract bool Contains(int dx, int dy);
        private VisiblePaths paths;
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

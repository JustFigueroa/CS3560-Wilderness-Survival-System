using System.Collections.Generic;

namespace WildernessGameplay
{
    public enum BrainAction { Move, Rest, NoAction }
    public struct BrainDecision
    {
        public BrainAction Action;
        public Square Destination;
    }
    // Shared visible-route selection. Subclasses change priorities; Game executes the returned action.
    public abstract class Brain
    {
        private const float LowSupplyFraction = .45f;
        public abstract BrainMode Mode { get; }
        protected abstract Path ChoosePath(Game game, VisiblePaths paths);
        // Recalculate paths from current vision and resources; return only the first move, rest, or no-action result.
        public BrainDecision MakeMove(Game game)
        {
            var paths = game.Player.Vision.FindPaths(game);
            var chosen = ChoosePath(game, paths) ?? Neighbor(game, paths);
            if (chosen == null) return new BrainDecision { Action = BrainAction.NoAction };
            var next = chosen.Steps[0];
            return new BrainDecision { Action = game.Player.currentStrength < next.StrengthCost ? BrainAction.Rest : BrainAction.Move, Destination = next };
        }
        public static Brain Create(BrainMode mode) => mode == BrainMode.ResourceAware ? (Brain)new ResourceAware() : new EastFirst();
        // EastFirst interrupts progress only for critical supplies; ResourceAware checks sooner.
        protected static bool CriticalSupplies(Game game) =>
            game.Player.currentFood <= game.Player.maxFood * .25f ||
            game.Player.currentWater <= game.Player.maxWater * .25f;

        // Seek the scarcer supply first, keeping the nearest-two target selection simple.
        protected static Path Supplies(Game game)
        {
            bool food = game.Player.currentFood <= game.Player.maxFood * LowSupplyFraction;
            bool water = game.Player.currentWater <= game.Player.maxWater * LowSupplyFraction;
            if (!food && !water) return null;
            bool foodFirst = (float)game.Player.currentFood / game.Player.maxFood <=
                (float)game.Player.currentWater / game.Player.maxWater;
            var first = SupplyPath(game, foodFirst, foodFirst ? food : water);
            if (first != null) return first;
            var second = SupplyPath(game, !foodFirst, foodFirst ? water : food);
            if (second != null) return second;
            if (game.Player.currentGold > 0)
                return game.Player.Vision.EasiestPath(new[] { game.Player.Vision.ClosestTrader(), game.Player.Vision.SecondClosestTrader() });
            return game.Player.Vision.EasiestPath(new[] { game.Player.Vision.ClosestGold(), game.Player.Vision.SecondClosestGold() });
        }

        private static Path SupplyPath(Game game, bool food, bool needed)
        {
            if (!needed) return null;
            var paths = food
                ? new[] { game.Player.Vision.ClosestFood(), game.Player.Vision.SecondClosestFood() }
                : new[] { game.Player.Vision.ClosestWater(), game.Player.Vision.SecondClosestWater() };
            var candidates = new List<Path>();
            float reserve = System.Math.Min((float)game.Player.currentFood / game.Player.maxFood,
                (float)game.Player.currentWater / game.Player.maxWater);
            foreach (var path in paths)
            {
                if (path == null || !path.Survivable) continue;
                float after = System.Math.Min((float)path.FoodAfter / game.Player.maxFood,
                    (float)path.WaterAfter / game.Player.maxWater);
                // Do not trade one shortage for a worse shortage of the other supply.
                if (after >= reserve && (food ? path.FoodAfter > game.Player.currentFood :
                    path.WaterAfter > game.Player.currentWater)) candidates.Add(path);
            }
            return game.Player.Vision.EasiestPath(candidates);
        }

        protected static Path East(Game game, VisiblePaths paths)
        {
            // Prefer an unvisited next step, then the furthest reachable visible column.
            for (int pass = 0; pass < 2; pass++)
            {
                var candidates = new List<Path>();
                int furthest = game.Player.X;
                foreach (var path in paths.All)
                {
                    var tile = path.Destination;
                    if (!path.Survivable || tile.X <= game.Player.X ||
                        (game.Visits[path.Steps[0].X, path.Steps[0].Y] > 0) != (pass == 1)) continue;
                    if (tile.X > furthest) { furthest = tile.X; candidates.Clear(); }
                    if (tile.X == furthest) candidates.Add(path);
                }
                var best = game.Player.Vision.EasiestPath(candidates);
                if (best != null) return best;
            }
            return null;
        }

        // Last fallback: a survivable adjacent square, preferring one not visited before.
        protected static Path Neighbor(Game game, VisiblePaths paths)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                var candidates = new List<Path>();
                foreach (var path in paths.All)
                    if (path.Steps.Count == 1 && (game.Visits[path.Destination.X, path.Destination.Y] > 0) == (pass == 1))
                        candidates.Add(path);
                var best = game.Player.Vision.EasiestPath(candidates);
                if (best != null) return best;
            }
            return null;
        }


    }
}

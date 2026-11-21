using System;
using System.Collections.Generic;

namespace WildernessGameplay
{
    // BFS finds one shortest visible route per destination. No global map knowledge or weighted search.
    public sealed class VisiblePaths
    {
        public Path ClosestFood, SecondClosestFood;
        public Path ClosestWater, SecondClosestWater;
        public Path ClosestGold, SecondClosestGold;
        public Path ClosestTrader, SecondClosestTrader;
        public readonly List<Path> All = new List<Path>();

        // Fixed order resolves equal-distance ties: E, NE, SE, N, S, NW, SW, W.
        private static readonly int[,] Directions = {
            {1, 0}, {1, 1}, {1, -1}, {0, 1}, {0, -1}, {-1, 1}, {-1, -1}, {-1, 0}
        };

        public VisiblePaths(Game game)
        {
            var visited = new HashSet<Square> { game.Current };
            var queue = new Queue<Path>();
            queue.Enqueue(new Path());
            while (queue.Count > 0)
            {
                var previous = queue.Dequeue();
                var from = previous.Steps.Count == 0 ? game.Current : previous.Destination;
                for (int i = 0; i < Directions.GetLength(0); i++)
                {
                    int x = from.X + Directions[i, 0], y = from.Y + Directions[i, 1];
                    if (!game.IsInside(x, y) || !game.Visible[x, y]) continue;
                    var tile = game.Tiles[x, y];
                    if (tile.StrengthCost > game.Player.maxStrength || !visited.Add(tile)) continue;
                    var path = new Path {
                        MovementCost = previous.MovementCost + tile.StrengthCost,
                        FoodCost = previous.FoodCost + tile.FoodCost,
                        WaterCost = previous.WaterCost + tile.WaterCost
                    };
                    path.Steps.AddRange(previous.Steps);
                    path.Steps.Add(tile);
                    path.Moves.AddRange(previous.Moves);
                    int dx = tile.X - from.X, dy = tile.Y - from.Y;
                    path.Moves.Add(dx > 0 ? (dy > 0 ? Direction.NE : dy < 0 ? Direction.SE : Direction.E)
                        : dx < 0 ? (dy > 0 ? Direction.NW : dy < 0 ? Direction.SW : Direction.W)
                        : dy > 0 ? Direction.N : Direction.S);
                    CheckSurvival(game, path);
                    All.Add(path);
                    if (tile.X != game.Rules.boardWidth - 1) queue.Enqueue(path);
                    if (tile.Food > 0) Remember(path, ref ClosestFood, ref SecondClosestFood);
                    if (tile.Water > 0) Remember(path, ref ClosestWater, ref SecondClosestWater);
                    if (tile.Gold > 0) Remember(path, ref ClosestGold, ref SecondClosestGold);
                    if (tile.Trader != null && (tile.Trader.Food > 0 || tile.Trader.Water > 0))
                        Remember(path, ref ClosestTrader, ref SecondClosestTrader);
                }
            }
        }

        private static void Remember(Path path, ref Path first, ref Path second)
        {
            if (first == null) first = path;
            else if (second == null) second = path;
        }

        // Compare candidate routes, not every possible route to each destination.
        public static Path Easiest(IEnumerable<Path> candidates)
        {
            Path best = null;
            foreach (var path in candidates)
            {
                if (path == null || !path.Survivable) continue;
                if (best == null || path.MovementCost < best.MovementCost ||
                    (path.MovementCost == best.MovementCost && path.Steps.Count < best.Steps.Count)) best = path;
            }
            return best;
        }

        // Simulate costs, rests, and pickups using local numbers; planning never changes the real player or tiles.
        private static void CheckSurvival(Game game, Path path)
        {
            int strength = game.Player.currentStrength, food = game.Player.currentFood, water = game.Player.currentWater;
            int turn = game.Turn;
            foreach (var tile in path.Steps)
            {
                while (strength < tile.StrengthCost)
                {
                    food -= game.Rules.actionFoodCost;
                    water -= game.Rules.actionWaterCost;
                    if (food <= 0 || water <= 0 || ++turn >= game.Rules.turnLimit) return;
                    strength = Math.Min(game.Player.maxStrength, strength + game.Rules.restStrength);
                }
                if (food < tile.FoodCost || water < tile.WaterCost) return;
                strength -= tile.StrengthCost;
                food = Math.Min(game.Player.maxFood, food - tile.FoodCost + tile.Food);
                water = Math.Min(game.Player.maxWater, water - tile.WaterCost + tile.Water);
                if (food <= 0 || water <= 0) return;
                turn++;
                if (tile.X == game.Rules.boardWidth - 1) break;
                if (turn >= game.Rules.turnLimit) return;
            }
            path.Survivable = true;
            path.FoodAfter = food;
            path.WaterAfter = water;
        }
    }
}

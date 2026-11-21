using System;

namespace WildernessGameplay
{
    // Generates and owns the squares, visibility, explored flags, and visit counts for one board.
    public sealed class Map
    {
        public Square[,] Tiles { get; }
        public bool[,] Visible { get; }
        public bool[,] Explored { get; }
        public int[,] Visits { get; }
        public int Width => Tiles.GetLength(0);
        public int Height => Tiles.GetLength(1);
        private readonly DifficultyPreset Rules;
        // Generate squares in x/y order using the supplied seed's random sequence.
        public Map(Difficulty difficulty, Random random)
        {
            Rules = difficulty.Settings;
            Tiles = new Square[Rules.boardWidth, Rules.boardHeight];
            Visible = new bool[Rules.boardWidth, Rules.boardHeight];
            Explored = new bool[Rules.boardWidth, Rules.boardHeight];
            Visits = new int[Rules.boardWidth, Rules.boardHeight];
            int total = 0;
            foreach (var rule in Rules.terrainRules)
            {
                total += rule.weight;
            }

            for (int x = 0; x < Rules.boardWidth; x++)
            {
                for (int y = 0; y < Rules.boardHeight; y++)
                {
                    int roll = random.Next(total);
                    TerrainRule selected = null;
                    foreach (var rule in Rules.terrainRules)
                    {
                        roll -= rule.weight;
                        if (roll < 0)
                        {
                            selected = rule;
                            break;
                        }
                    }

                    var tile = new Square
                    {
                        X = x,
                        Y = y,
                        TerrainType = difficulty.CreateTerrain(selected)
                    };
                    tile.Food = tile.InitialFood = Spawn(random, Rules.foodSpawnChance, Rules.minimumResourceAmount, Rules.maximumResourceAmount);
                    tile.Water = tile.InitialWater = Spawn(random, Rules.waterSpawnChance, Rules.minimumResourceAmount, Rules.maximumResourceAmount);
                    tile.Gold = tile.InitialGold = Spawn(random, Rules.goldSpawnChance, Rules.minimumGoldAmount, Rules.maximumGoldAmount);
                    if (random.NextDouble() < Rules.traderSpawnChance)
                    {
                        tile.Trader = Trader.Create((TraderMood)random.Next(4));
                    }

                    tile.FoodBonus.IsRepeating = tile.WaterBonus.IsRepeating = tile.GoldBonus.IsRepeating = Rules.repeatingResources;
                    Tiles[x, y] = tile;
                }
            }

        }
        // Roll each resource independently, then choose an inclusive amount when it spawns.
        private static int Spawn(Random random, float chance, int minimum, int maximum) => random.NextDouble() < chance ? random.Next(minimum, maximum + 1) : 0;
        public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        public Square GetSquare(int x, int y) => IsInside(x, y) ? Tiles[x, y] : null;
        // Refill enabled repeating bonuses; all shipped difficulty presets leave repeating resources off.
        public void Respawn(int turn)
        {
            if (!Rules.repeatingResources) return;
            foreach (var tile in Tiles)
                foreach (var item in tile.Items)
                    if (item is Bonus bonus) bonus.Respawn(turn, Rules.resourceRespawnTurns);
        }
    }
}

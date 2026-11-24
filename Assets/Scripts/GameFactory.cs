using System;

namespace WildernessGameplay
{
    // Creates the difficulty, seeded map, player (with a random vision), brain, and game in a consistent order.
    public static class GameFactory
    {
        public static Game CreateSession(GameSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (!settings.difficultyValues.TryValidate(out string error)) throw new ArgumentException(error);
            var difficulty = Difficulty.Create(settings.difficultyValues);
            var rules = difficulty.Settings;
            var random = new Random(settings.seed);
            var map = new Map(difficulty, random);
            var player = new Player {
                X = 0, Y = random.Next(rules.boardHeight),
                currentStrength = rules.startingStrength, currentFood = rules.startingFood, currentWater = rules.startingWater, currentGold = rules.startingGold,
                maxStrength = rules.maximumStrength, maxFood = rules.maximumFood, maxWater = rules.maximumWater,
                Brain = Brain.Create(rules.brain),
                Vision = Vision.CreateRandom(random)
            };
            return new Game(difficulty, map, player, new Random(settings.seed ^ 0x51A7));
        }
    }
}

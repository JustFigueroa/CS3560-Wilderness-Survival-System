using System;

// Serialized only because GameManager stores these editable presets in the scene.
// Editable starting values and rules for one difficulty; never stores current player resources.
[Serializable]
public class DifficultyPreset
{
    public DifficultyLevel level;
    // Board dimensions.
    public int boardWidth = 10;
    public int boardHeight = 10;
    // Starting resources and maximum capacities.
    public int startingStrength = 10;
    public int startingFood = 20;
    public int startingWater = 20;
    public int startingGold = 10;
    public int maximumStrength = 20;
    public int maximumFood = 30;
    public int maximumWater = 30;
    // Independent per-tile spawn probabilities, from zero to one.
    public float foodSpawnChance = 0.5f;
    public float waterSpawnChance = 0.5f;
    public float goldSpawnChance = 0.3f;
    public float traderSpawnChance = .2f;
    // Inclusive food/water pickup quantities; gold has its own range.
    public int minimumResourceAmount = 2;
    public int maximumResourceAmount = 4;
    public int minimumGoldAmount = 3;
    public int maximumGoldAmount = 6;
    public bool repeatingResources;
    public int resourceRespawnTurns = 15;
    // Terrain cost multipliers; resulting entry costs are rounded up.
    public float movementCostMultiplier = 1f;
    public float foodCostMultiplier = 1f;
    public float waterCostMultiplier = 1f;
    // Costs and recovery for rest/trade actions, plus the turn limit.
    public int actionFoodCost = 1;
    public int actionWaterCost = 1;
    public int restStrength = 5;
    public int turnLimit = 500;
    public int tradeAmount = 3;
    // Autoplay selection and terrain generation weights.
    public BrainMode brain = BrainMode.ResourceAware;
    public bool rememberExploredTiles = true;
    public TerrainRule[] terrainRules = DefaultTerrainRules();
    public static TerrainRule[] DefaultTerrainRules() => new[]
    {
        new TerrainRule
        {
            terrain = TerrainKind.Plains,
            weight = 4,
            strengthCost = 1,
            foodCost = 1,
            waterCost = 1
        },
        new TerrainRule
        {
            terrain = TerrainKind.Forest,
            weight = 3,
            strengthCost = 3,
            foodCost = 2,
            waterCost = 1
        },
        new TerrainRule
        {
            terrain = TerrainKind.Desert,
            weight = 1,
            strengthCost = 2,
            foodCost = 1,
            waterCost = 3
        },
        new TerrainRule
        {
            terrain = TerrainKind.Swamp,
            weight = 1,
            strengthCost = 4,
            foodCost = 3,
            waterCost = 2
        },
        new TerrainRule
        {
            terrain = TerrainKind.Mountain,
            weight = 1,
            strengthCost = 5,
            foodCost = 2,
            waterCost = 2
        }
    };
    // Default values used when a scene preset is first created; Inspector values remain editable.
    public static DifficultyPreset Create(DifficultyLevel level)
    {
        var preset = new DifficultyPreset
        {
            level = level
        };
        if (level == DifficultyLevel.Medium)
        {
            preset.startingStrength = 10;
            preset.startingFood = 18;
            preset.startingWater = 18;
            preset.startingGold = 8;
            preset.minimumResourceAmount = 2;
            preset.maximumResourceAmount = 4;
            preset.restStrength = 5;
            preset.actionFoodCost = 1;
            preset.actionWaterCost = 1;
            preset.boardWidth = preset.boardHeight = 30;
            preset.terrainRules[0].weight = 22;
            preset.terrainRules[1].weight = 32;
            preset.terrainRules[2].weight = 12;
            preset.terrainRules[3].weight = 12;
            preset.terrainRules[4].weight = 22;
            preset.turnLimit = 1000;
        }
        else if (level == DifficultyLevel.Hard)
        {
            preset.startingStrength = 10;
            preset.startingFood = 15;
            preset.startingWater = 15;
            preset.startingGold = 8;
            preset.minimumResourceAmount = 2;
            preset.maximumResourceAmount = 4;
            preset.minimumGoldAmount = 3;
            preset.maximumGoldAmount = 6;
            preset.restStrength = 5;
            preset.actionFoodCost = 1;
            preset.actionWaterCost = 1;
            preset.boardWidth = preset.boardHeight = 50;
            preset.terrainRules[0].weight = 1;
            preset.terrainRules[2].weight = 2;
            preset.terrainRules[3].weight = 2;
            preset.terrainRules[4].weight = 2;
            preset.turnLimit = 2000;
        }

        return preset;
    }

    // Check configuration before starting a game; numeric limits keep resource arithmetic in range.
    public bool TryValidate(out string error)
    {
        error = null;
        if (!Enum.IsDefined(typeof(DifficultyLevel), level) || !Enum.IsDefined(typeof(BrainMode), brain))
            error = "Invalid difficulty or brain selection.";
        else if (boardWidth < 1 || boardHeight < 1 || (long)boardWidth * boardHeight > 10000)
            error = "Board dimensions must be positive and total at most 10,000 tiles.";
        else if (maximumStrength < 1 || maximumFood < 1 || maximumWater < 1 ||
            maximumStrength > 1000000 || maximumFood > 1000000 || maximumWater > 1000000 ||
            startingStrength < 0 || startingStrength > maximumStrength || startingFood < 1 ||
            startingFood > maximumFood || startingWater < 1 || startingWater > maximumWater || startingGold < 0 || startingGold > 1000000)
            error = "Starting supplies must fit positive resource limits; food and water must be positive.";
        else if (!Chance(foodSpawnChance) || !Chance(waterSpawnChance) || !Chance(goldSpawnChance) || !Chance(traderSpawnChance))
            error = "Spawn chances must be between zero and one.";
        else if (minimumResourceAmount < 1 || maximumResourceAmount < minimumResourceAmount || maximumResourceAmount > 1000000 ||
            minimumGoldAmount < 1 || maximumGoldAmount < minimumGoldAmount || maximumGoldAmount > 1000000)
            error = "Resource amounts must form a positive inclusive range up to 1,000,000.";
        else if (!Multiplier(movementCostMultiplier) || !Multiplier(foodCostMultiplier) || !Multiplier(waterCostMultiplier))
            error = "Cost multipliers must be finite numbers from zero to 100.";
        else if (actionFoodCost < 0 || actionWaterCost < 0 || actionFoodCost > 1000000 || actionWaterCost > 1000000 ||
            restStrength < 1 || restStrength > 1000000 || turnLimit < 1 || turnLimit > 100000 ||
            resourceRespawnTurns < 1 || tradeAmount < 1 || tradeAmount > 1000)
            error = "Invalid action costs, recovery, turn limit, respawn interval, or trade amount.";
        else if (terrainRules == null || terrainRules.Length == 0)
            error = "At least one terrain rule is required.";
        else
        {
            int total = 0;
            var seen = new System.Collections.Generic.HashSet<TerrainKind>();
            foreach (var rule in terrainRules)
            {
                if (rule == null || !Enum.IsDefined(typeof(TerrainKind), rule.terrain) || !seen.Add(rule.terrain) ||
                    rule.weight < 0 || rule.weight > 10000 || rule.strengthCost < 0 || rule.foodCost < 0 || rule.waterCost < 0 ||
                    rule.strengthCost > 1000 || rule.foodCost > 1000 || rule.waterCost > 1000)
                {
                    error = "Terrain rules need unique types and nonnegative bounded weights and costs.";
                    break;
                }
                total += rule.weight;
            }
            if (error == null && total == 0) error = "At least one terrain weight must be positive.";
        }
        return error == null;
    }

    // Copy terrain entries as well as the preset so a running game cannot edit the menu's values.
    public DifficultyPreset Copy()
    {
        var copy = (DifficultyPreset)MemberwiseClone();
        if (terrainRules != null)
        {
            copy.terrainRules = Array.ConvertAll(terrainRules, rule => rule?.Copy());
        }

        return copy;
    }

    private static bool Chance(float value) => value >= 0 && value <= 1;
    private static bool Multiplier(float value) => value >= 0 && value <= 100;
}

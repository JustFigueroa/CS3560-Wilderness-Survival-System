using System.Collections.Generic;
using UnityEngine;

public abstract class Difficulty : MonoBehaviour
{
    // C# equivalent of Map<Terrain, Integer>; requires custom Inspector support.
    [System.NonSerialized] public Dictionary<Terrain, int> terrainWeights = new Dictionary<Terrain, int>();
    public float movementCostMultiplier;
    public float waterCostMultiplier;
    public float foodCostMultiplier;
    public double foodSpawnRate;
    public double waterSpawnRate;
    public double goldSpawnRate;
    public double traderSpawnRate;

    public Dictionary<Terrain, int> getTerrainWeights() => throw new System.NotImplementedException();
    public double getFoodSpawnRate() => throw new System.NotImplementedException();
    public double getWaterSpawnRate() => throw new System.NotImplementedException();
    public double getGoldSpawnRate() => throw new System.NotImplementedException();
    public double getTraderSpawnRate() => throw new System.NotImplementedException();
}

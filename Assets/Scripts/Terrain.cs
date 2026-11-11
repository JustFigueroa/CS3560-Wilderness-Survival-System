using System.Collections.Generic;
using UnityEngine;

public abstract class Terrain : MonoBehaviour
{
    public int movementCost;
    public int waterCost;
    public int foodCost;
    public Vision visionType;

    public int getMovementCost() => throw new System.NotImplementedException();
    public int getWaterCost() => throw new System.NotImplementedException();
    public int getFoodCost() => throw new System.NotImplementedException();
    public Vision getVisionType() => throw new System.NotImplementedException();
}

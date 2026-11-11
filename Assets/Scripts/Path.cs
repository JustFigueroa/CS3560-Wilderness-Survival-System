using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{
    public List<Direction> moves = new List<Direction>();
    public int movementCost;
    public int waterCost;
    public int foodCost;
}

using System.Collections.Generic;
using UnityEngine;

public abstract class Vision : MonoBehaviour
{
    public abstract List<Square> getVisibleSquares(Map map, Square playerLocation);
    public abstract Path closestFood();
    public abstract Path closestWater();
    public abstract Path closestGold();
    public abstract Path closestTrader();
    public abstract Path secondClosestFood();
    public abstract Path secondClosestWater();
    public abstract Path secondClosestGold();
    public abstract Path secondClosestTrader();
    public abstract Path easiestPath(Map map, Player player);
}

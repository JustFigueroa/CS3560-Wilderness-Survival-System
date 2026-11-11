using System.Collections.Generic;
using UnityEngine;

public class Cautious : Vision
{
    public override List<Square> getVisibleSquares(Map map, Square playerLocation) => throw new System.NotImplementedException();
    public override Path closestFood() => throw new System.NotImplementedException();
    public override Path closestWater() => throw new System.NotImplementedException();
    public override Path closestGold() => throw new System.NotImplementedException();
    public override Path closestTrader() => throw new System.NotImplementedException();
    public override Path secondClosestFood() => throw new System.NotImplementedException();
    public override Path secondClosestWater() => throw new System.NotImplementedException();
    public override Path secondClosestGold() => throw new System.NotImplementedException();
    public override Path secondClosestTrader() => throw new System.NotImplementedException();
    public override Path easiestPath(Map map, Player player) => throw new System.NotImplementedException();
}

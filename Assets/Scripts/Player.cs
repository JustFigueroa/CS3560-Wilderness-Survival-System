using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int maxStrength;
    public int maxWater;
    public int maxFood;
    public int currentStrength;
    public int currentWater;
    public int currentFood;
    public int currentGold;
    public Square occupiedSquare;
    public Brain brain;
    public Vision vision;

    public void move(Direction direction, Map map) => throw new System.NotImplementedException();
    public void rest() => throw new System.NotImplementedException();
    public void interactWith(Item item, int turn) => throw new System.NotImplementedException();
    public Trade proposeTrade(Player player, Trader trader) => throw new System.NotImplementedException();
    public TradeDecision evaluateTrade(Trade offer, Player player) => throw new System.NotImplementedException();
    public TradeDecision evaluateCounter(Trade offer, Player player) => throw new System.NotImplementedException();
    public bool applyTrade(Trade offer) => throw new System.NotImplementedException();
    public void updateVision(Terrain terrain) => throw new System.NotImplementedException();
}

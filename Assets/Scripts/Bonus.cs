using System.Collections.Generic;
using UnityEngine;

public abstract class Bonus : MonoBehaviour, Item
{
    public int amount;
    public bool isRepeating;
    public int lastCollectedTurn;
    public bool isDepleted;

    public void interact(Player player, int turn) => throw new System.NotImplementedException();
    public bool canCollect(int turn) => throw new System.NotImplementedException();
    public void collect(Player player, int turn) => throw new System.NotImplementedException();
    public abstract void applyBonus(Player player);
}

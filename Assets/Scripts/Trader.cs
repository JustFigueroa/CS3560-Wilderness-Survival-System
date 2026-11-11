using System.Collections.Generic;
using UnityEngine;

public class Trader : MonoBehaviour, Item
{
    public int counterOfferCount;

    public virtual void interact(Player player, int turn) => throw new System.NotImplementedException();
    public virtual TradeDecision evaluate(Trade offer) => throw new System.NotImplementedException();
    public virtual Trade counterOffer(Trade offer) => throw new System.NotImplementedException();
}

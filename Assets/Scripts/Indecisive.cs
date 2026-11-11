using System.Collections.Generic;
using UnityEngine;

public class Indecisive : Trader
{
    public override TradeDecision evaluate(Trade offer) => throw new System.NotImplementedException();
    public override Trade counterOffer(Trade offer) => throw new System.NotImplementedException();
}

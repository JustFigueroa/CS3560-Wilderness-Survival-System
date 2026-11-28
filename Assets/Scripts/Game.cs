using System;

namespace WildernessGameplay
{
    // Authoritative game state. No scene objects or frame timing are used here.
    public sealed class Game
    {
        public Difficulty Difficulty { get; }
        public DifficultyPreset Rules => Difficulty.Settings;
        public Map Map { get; }
        public Player Player { get; }
        public Square[,] Tiles => Map.Tiles;
        public bool[,] Visible => Map.Visible;
        public bool[,] Explored => Map.Explored;
        public int[,] Visits => Map.Visits;
        public int Turn { get; private set; }
        public SessionPhase Phase { get; private set; }
        public string Message { get; private set; }
        public event Action<string> EventReported;
        public VisionMode CurrentVision { get; private set; }
        private readonly Random tradeRandom;
        private int usedStrength, usedFood, usedWater, usedGold;
        private int gainedStrength, gainedFood, gainedWater, gainedGold;
        public Square Current => Tiles[Player.X, Player.Y];

        internal Game(Difficulty difficulty, Map map, Player player, Random random)
        {
            Difficulty = difficulty;
            Map = map;
            Player = player;
            tradeRandom = random;
            Visits[Player.X, Player.Y] = 1;
            Phase = SessionPhase.Running;
            RefreshVision();
            Message = "";
            CheckOutcome();
        }
        public bool IsInside(int x, int y) => Map.IsInside(x, y);
        public bool IsKnown(int x, int y) => IsInside(x, y) && (Rules.rememberExploredTiles ? Explored[x, y] : Visible[x, y]);
        public bool CanMove(int x, int y) => Phase == SessionPhase.Running && Player.CanMove(Map.GetSquare(x, y));

        // A move pays terrain costs, collects resources, and schedules any trader offer in the same turn.
        public bool Move(int x, int y)
        {
            if (!CanMove(x, y))
            {
                return false;
            }

            BeginAction();
            var tile = Tiles[x, y];
            Report("Entered a " + tile.Terrain + " tile.");
            usedStrength = tile.StrengthCost;
            usedFood = tile.FoodCost;
            usedWater = tile.WaterCost;
            Player.Move(tile);
            Visits[x, y]++;
            CollectResources();
            if (Current.Trader != null) Player.InteractWith(Current.Trader, Turn);
            Finish(null, false, true);
            return true;
        }

        // Entry pickup shares the movement turn and has no additional upkeep.
        private void CollectResources()
        {
            int food = Player.currentFood, water = Player.currentWater, gold = Player.currentGold;
            Player.Collect(Current, Turn + 1);
            gainedFood = Player.currentFood - food;
            gainedWater = Player.currentWater - water;
            gainedGold = Player.currentGold - gold;
        }

        // A rest restores strength; Finish charges the configured food/water upkeep.
        public bool Rest()
        {
            if (Phase != SessionPhase.Running || Player.currentStrength >= Player.maxStrength)
            {
                return false;
            }

            BeginAction();
            gainedStrength = Player.Rest(Rules.restStrength);
            Finish("Rested.", true);
            return true;
        }

        // Entry always presents an offer, even when the player is full or cannot afford it.
        private void PresentTraderOffer()
        {
            var trader = Current.Trader;
            if (trader == null) return;
            trader.PresentOffer(Player, Rules, tradeRandom);
            if (trader.OfferPending) ReportOffer(trader, "Trader provides offer: ");
            else Report("Trader leaves.");
        }

        private void ReportOffer(Trader trader, string prefix) =>
            Report(prefix + trader.OfferAmount + " " + trader.CounterSupply + " for " + trader.CounterPrice + " gold.");

        // Player and trader responses are separate Next actions. Only completed exchanges charge upkeep.
        public bool Trade(SupplyKind supply, bool acceptOffer)
        {
            var trader = Current.Trader;
            if (Phase != SessionPhase.Running || trader == null || trader.NextAction != TraderAction.None ||
                !trader.OfferPending || trader.CounterSupply != supply) return false;
            int room = supply == SupplyKind.Food ? Player.maxFood - Player.currentFood : Player.maxWater - Player.currentWater;
            if (acceptOffer && (Player.currentGold < trader.CounterPrice || room < trader.OfferAmount)) return false;
            BeginAction();
            trader.Negotiations++;
            trader.PlayerAccepted = acceptOffer;
            trader.PlayerOffer = Player.ProposeTrade(trader, acceptOffer);
            // Non-indecisive traders honor an accepted offer immediately.
            if (acceptOffer && trader.HonorsAcceptanceImmediately)
            {
                Exchange(trader, supply, trader.OfferAmount, trader.PlayerPrice);
                trader.OfferPending = false;
                Finish("Player accepts " + (trader.IsCounterOffer ? "counteroffer." : "offer."), true, true);
            }
            else
            {
                trader.NextAction = TraderAction.Respond;
                Finish(acceptOffer ? "Player accepts " + (trader.IsCounterOffer ? "counteroffer." : "offer.") :
                    "Player provides counteroffer: " + trader.PlayerPrice + " gold for " + trader.OfferAmount + " " + supply + ".", false);
            }
            return true;
        }

        // Show exactly one pending trader response (or player departure) per Next/autoplay action.
        public bool AdvanceTrader()
        {
            var trader = Current.Trader;
            if (Phase != SessionPhase.Running || trader == null || trader.NextAction == TraderAction.None) return false;
            BeginAction();
            if (trader.NextAction == TraderAction.PlayerLeave)
            {
                trader.NextAction = TraderAction.None;
                Finish("Player leaves.", false);
                return true;
            }
            if (trader.NextAction == TraderAction.Offer)
            {
                PresentTraderOffer();
                Finish(null, false);
                return true;
            }
            trader.NextAction = TraderAction.None;
            TradeDecision decision = trader.Evaluate(trader.PlayerOffer, Player, tradeRandom);
            if (decision == TradeDecision.ACCEPT)
            {
                Exchange(trader, trader.CounterSupply, trader.OfferAmount, trader.PlayerPrice);
                trader.OfferPending = false;
                Finish(trader.PlayerAccepted ? "Trader accepts offer." : "Trader accepts counteroffer.", true, true);
            }
            else if (decision == TradeDecision.REJECT)
            {
                trader.OfferPending = false;
                Finish("Trader leaves.", false);
            }
            else
            {
                trader.CounterOffer(tradeRandom);
                string prefix = trader is Indecisive && trader.PlayerAccepted
                    ? "Indecisive trader becomes indecisive. Indecisive Trader provides counteroffer: "
                    : "Trader provides counteroffer: ";
                ReportOffer(trader, prefix);
                Finish(null, false);
            }
            return true;
        }

        // Reject now and schedule leaving for the following action; neither action charges upkeep.
        public bool DeclineTrade()
        {
            var trader = Current.Trader;
            if (Phase != SessionPhase.Running || trader == null || !trader.OfferPending || trader.NextAction != TraderAction.None) return false;
            BeginAction();
            trader.OfferPending = false;
            trader.NextAction = TraderAction.PlayerLeave;
            Finish("Player rejects " + (trader.IsCounterOffer ? "counteroffer." : "offer."), false);
            return true;
        }

        private void Exchange(Trader trader, SupplyKind supply, int amount, int price)
        {
            usedGold = price;
            if (supply == SupplyKind.Food) gainedFood = amount;
            else gainedWater = amount;
            if (!Player.ApplyTrade(WildernessGameplay.Trade.Purchase(supply, amount, price)))
                throw new InvalidOperationException("An accepted trade must be affordable and fit inventory.");

            trader.CounterPrice = 0;
        }

        public void EndStranded()
        {
            if (Phase != SessionPhase.Running) return;
            Phase = SessionPhase.Lost;
        }

        private void BeginAction()
        {
            Message = "";
            usedStrength = usedFood = usedWater = usedGold = 0;
            gainedStrength = gainedFood = gainedWater = gainedGold = 0;
        }

        private void Report(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            if (message.StartsWith("Trader ", StringComparison.Ordinal) && Current.Trader != null)
                message = Current.Trader.Mood + " " + message;
            Message = string.IsNullOrEmpty(Message) ? message : Message + "\n" + message;
            EventReported?.Invoke(message);
        }

        private static string ResourceMessage(string label, string sign, int strength, int food, int water, int gold) =>
            label + ": " + sign + strength + " str        " + sign + food + " food        " +
            sign + water + " water        " + sign + gold + " gold";

        // Complete one action: charge upkeep, advance time, refresh visibility, log, and check the outcome.
        private void Finish(string description, bool upkeep, bool showResources = false)
        {
            if (upkeep)
            {
                usedFood += Math.Min(Player.currentFood, Rules.actionFoodCost);
                usedWater += Math.Min(Player.currentWater, Rules.actionWaterCost);
                Player.currentFood = Math.Max(0, Player.currentFood - Rules.actionFoodCost);
                Player.currentWater = Math.Max(0, Player.currentWater - Rules.actionWaterCost);
            }

            Turn++;
            Map.Respawn(Turn);

            RefreshVision();
            Report(description);
            if (showResources)
            {
                Report(ResourceMessage("Used", "-", usedStrength, usedFood, usedWater, usedGold));
                Report(ResourceMessage("Gained", "+", gainedStrength, gainedFood, gainedWater, gainedGold));
            }
            CheckOutcome();
        }

        private void CheckOutcome()
        {
            // Exhaustion takes precedence over reaching the goal on the same turn.
            if (Player.currentFood <= 0 || Player.currentWater <= 0)
            {
                Phase = SessionPhase.Lost;
            }
            else if (Player.X == Rules.boardWidth - 1)
            {
                Phase = SessionPhase.Won;
            }
            else if (Turn >= Rules.turnLimit)
            {
                Phase = SessionPhase.Lost;
            }
        }

        // Terrain selects the vision object; update the map's visible squares without moving scene objects.
        public void RefreshVision()
        {
            Player.UpdateVision(Current);
            CurrentVision = Player.Vision.Mode;
            Array.Clear(Visible, 0, Visible.Length);
            foreach (var square in Player.Vision.GetVisibleSquares(Map, Current))
                Visible[square.X, square.Y] = Explored[square.X, square.Y] = true;
        }
        public bool HasWon() => Phase == SessionPhase.Won;
        // Resolve pending trade messages first; otherwise ask the selected brain for one action.
        public bool PlayTurn()
        {
            if (Phase != SessionPhase.Running) return false;
            if (AdvanceTrader()) return true;
            var trader = Current.Trader;
            if (trader != null && trader.OfferPending)
            {
                var response = Player.EvaluateTrade(trader, Rules);
                return response == TradeDecision.REJECT ? DeclineTrade() : Trade(trader.CounterSupply, response == TradeDecision.ACCEPT);
            }
            if (Player.Brain.Mode != Rules.brain) Player.Brain = Brain.Create(Rules.brain);
            var decision = Player.Brain.MakeMove(this);
            switch (decision.Action)
            {
                case BrainAction.Move: return Move(decision.Destination.X, decision.Destination.Y);
                case BrainAction.Rest: return Rest();
                default: EndStranded();
                return false;
            }
        }
    }
}

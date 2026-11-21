namespace WildernessGameplay
{
    // Base collectible item. Collection transfers only what fits; repeating bonuses can refill after their timer.
    public abstract class Bonus : Item
    {
        public int Amount, InitialAmount;
        public bool IsRepeating;
        public int LastCollectedTurn = -1;
        public bool IsDepleted => Amount == 0;
        public bool CanCollect(int turn) => !IsDepleted;
        public void Interact(Player player, int turn) => Collect(player, turn);
        public void Collect(Player player, int turn)
        {
            if (!CanCollect(turn)) return;
            Amount -= ApplyBonus(player);
            LastCollectedTurn = turn;
        }
        protected abstract int ApplyBonus(Player player);
        public void Respawn(int turn, int interval)
        {
            if (IsRepeating && LastCollectedTurn >= 0 && turn - LastCollectedTurn >= interval)
            { Amount = InitialAmount; LastCollectedTurn = -1; }
        }
    }
}

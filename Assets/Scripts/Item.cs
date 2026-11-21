namespace WildernessGameplay
{
    // A map item the player can interact with; implemented by bonuses and traders.
    public interface Item { void Interact(Player player, int turn); }
}

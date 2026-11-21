namespace WildernessGameplay
{
    // Seek supplies when critical; otherwise favor eastward progress, then supply routes.
    public sealed class EastFirst : Brain
    {
        public override BrainMode Mode => BrainMode.EastFirst;
        protected override Path ChoosePath(Game game, VisiblePaths paths) =>
            CriticalSupplies(game) ? Supplies(game) ?? East(game, paths) : East(game, paths) ?? Supplies(game);
    }
}

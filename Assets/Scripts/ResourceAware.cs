namespace WildernessGameplay
{
    // Seeks needed supplies first, then chooses eastward progress using the same shared routes.
    public sealed class ResourceAware : Brain
    {
        public override BrainMode Mode => BrainMode.ResourceAware;
        protected override Path ChoosePath(Game game, VisiblePaths paths) => Supplies(game) ?? East(game, paths);
    }
}

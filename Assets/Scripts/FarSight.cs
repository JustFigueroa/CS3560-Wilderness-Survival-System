namespace WildernessGameplay
{
    // Plains/desert vision: a diamond extending four squares from the player.
    public sealed class FarSight : Vision
    {
        public override VisionMode Mode => VisionMode.FarSight;
        protected override int Radius => 4;
        protected override bool Contains(int dx, int dy) => System.Math.Abs(dx) + System.Math.Abs(dy) <= Radius;
    }
}

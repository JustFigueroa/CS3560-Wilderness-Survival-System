namespace WildernessGameplay
{
    // Forest vision: a diamond extending two squares from the player.
    public sealed class Cautious : Vision
    {
        public override VisionMode Mode => VisionMode.Cautious;
        protected override int Radius => 2;
        protected override bool Contains(int dx, int dy) => System.Math.Abs(dx) + System.Math.Abs(dy) <= Radius;
    }
}

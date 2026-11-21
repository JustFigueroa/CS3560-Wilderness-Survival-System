namespace WildernessGameplay
{
    // Mountain vision: a forward corridor plus the immediately adjacent western square.
    public sealed class Focused : Vision
    {
        public override VisionMode Mode => VisionMode.Focused;
        protected override int Radius => 3;
        protected override bool Contains(int dx, int dy) => (dx >= 0 && System.Math.Abs(dy) <= 1) || System.Math.Abs(dx) + System.Math.Abs(dy) <= 1;
    }
}

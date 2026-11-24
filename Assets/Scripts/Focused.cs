namespace WildernessGameplay
{
    // A three-row corridor reaching east (base range three) plus the adjacent squares. Terrain changes its length only.
    public sealed class Focused : Vision
    {
        public override VisionMode Mode => VisionMode.Focused;
        public override int BaseRadius => 3;
        protected override bool Contains(int dx, int dy) => (dx >= 0 && System.Math.Abs(dy) <= 1) || System.Math.Abs(dx) + System.Math.Abs(dy) <= 1;
    }
}

using System;

public enum TerrainKind
{
    Plains,
    Forest,
    Desert,
    Swamp,
    Mountain
}

public enum VisionMode
{
    KeenEyed,
    Cautious,
    Focused,
    FarSight
}

public enum BrainMode
{
    ResourceAware,
    EastFirst
}

public enum SessionPhase
{
    Running,
    Won,
    Lost
}

public enum SupplyKind
{
    Food,
    Water
}

public enum TraderMood
{
    Generous,
    Patient,
    Impatient,
    Indecisive
}

[Serializable]
public class TerrainRule
{
    public TerrainKind terrain;
    public int weight = 1;
    public int strengthCost = 1;
    public int foodCost = 1;
    public int waterCost = 1;
    public TerrainRule Copy() => (TerrainRule)MemberwiseClone();
}

public enum TraderAction { None, Offer, Respond, PlayerLeave }

using AthensWorkspace.MHWs.Models;

namespace AthensWorkspace.Models.MHWs;

public class AmuletPattern
{
    public byte Rare { get; set; }
    public byte Group1 { get; set; }
    public byte Group2 { get; set; }
    public byte Group3 { get; set; }
    public Slot Slot1 { get; set; }
    public Slot Slot2 { get; set; }
    public Slot Slot3 { get; set; }

    public override bool Equals(object? obj) => Equals(obj as AmuletPattern);

    private bool Equals(AmuletPattern? other) =>
        other != null && Rare == other.Rare &&
        Group1 == other.Group1 && Group2 == other.Group2 && Group3 == other.Group3 &&
        Slot1 == other.Slot1 && Slot2 == other.Slot2 && Slot3 == other.Slot3;

    public override int GetHashCode() => HashCode.Combine(Rare, Group1, Group2, Group3, (int)Slot1, (int)Slot2, (int)Slot3);
    public static bool operator ==(AmuletPattern? left, AmuletPattern? right) => Equals(left, right);
    public static bool operator !=(AmuletPattern? left, AmuletPattern? right) => !Equals(left, right);
}
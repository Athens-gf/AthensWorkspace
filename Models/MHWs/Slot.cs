using Utility;

namespace AthensWorkspace.MHWs.Models;

[Flags]
public enum Slot : byte
{
    [IconPath("/img/mhws/icon/slot/none.png")]
    None = 0,

    [IconPath("/img/mhws/icon/slot/weapon1.png")]
    Weapon1 = 1 << 0,

    [IconPath("/img/mhws/icon/slot/weapon2.png")]
    Weapon2 = 1 << 1,

    [IconPath("/img/mhws/icon/slot/weapon3.png")]
    Weapon3 = 1 << 2,

    [IconPath("/img/mhws/icon/slot/armor1.png")]
    Armor1 = 1 << 5,

    [IconPath("/img/mhws/icon/slot/armor2.png")]
    Armor2 = 1 << 6,

    [IconPath("/img/mhws/icon/slot/armor3.png")]
    Armor3 = 1 << 7,
}

public static class ExSlot
{
    public static byte GetSize(this Slot slot) => slot switch
    {
        Slot.None => 0,
        Slot.Weapon1 or Slot.Armor1 => 1,
        Slot.Weapon2 or Slot.Armor2 => 2,
        Slot.Weapon3 or Slot.Armor3 => 3,
        _ => throw new ArgumentOutOfRangeException(nameof(slot), slot, null)
    };

    public static bool IsWeapon(this Slot slot) => (Slot.Weapon1 | Slot.Weapon2 | Slot.Weapon3).HasFlag(slot);
    public static bool IsArmor(this Slot slot) => (Slot.Armor1 | Slot.Armor2 | Slot.Armor3).HasFlag(slot);

    public static string ToStr(this Slot slot) => slot == Slot.None ? "" : (slot.IsWeapon() ? "武器" : "防具") + slot.GetSize();
}
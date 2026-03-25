using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;

namespace AthensWorkspace.MHWs.Models;

[Flags]
public enum SkillType : byte
{
    [EnumText("武器")] T0Weapon = 1 << 0,
    [EnumText("防具")] T1Armor = 1 << 1,
    [EnumText("シリーズ")] T2Series = 1 << 2,
    [EnumText("グループ")] T3Group = 1 << 3,
}

public static class ExSkillType
{
    public static IEnumerable<SelectListItem> Sli(SkillType type) => ExEnum.GetIter<SkillType>()
        .Select(t => new SelectListItem { Value = ((int)t).ToString(), Selected = t == type, Text = t.GetText() });
}
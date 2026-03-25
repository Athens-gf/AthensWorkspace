using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;

namespace AthensWorkspace.MHWs.Models;

[Flags]
public enum Icon : short
{
    [EnumText("グループ"), IconPath("/img/mhws/icon/skill/group.png")]
    Group = 1 << 0,

    [EnumText("シリーズ"), IconPath("/img/mhws/icon/skill/series.png")]
    Series = 1 << 1,

    [EnumText("攻撃系"), IconPath("/img/mhws/icon/skill/attack.png")]
    Attack = 1 << 2,

    [EnumText("会心系"), IconPath("/img/mhws/icon/skill/critical.png")]
    Critical = 1 << 3,

    [EnumText("属性系"), IconPath("/img/mhws/icon/skill/enhancement.png")]
    Enhancement = 1 << 4,

    [EnumText("切れ味系"), IconPath("/img/mhws/icon/skill/sharpness.png")]
    Sharpness = 1 << 5,

    [EnumText("弾系"), IconPath("/img/mhws/icon/skill/guns.png")]
    Guns = 1 << 6,

    [EnumText("バフ系"), IconPath("/img/mhws/icon/skill/power.png")]
    Power = 1 << 7,

    [EnumText("補助・軽減系"), IconPath("/img/mhws/icon/skill/viability.png")]
    Viability = 1 << 8,

    [EnumText("防御系"), IconPath("/img/mhws/icon/skill/resist.png")]
    Resist = 1 << 9,

    [EnumText("回復系"), IconPath("/img/mhws/icon/skill/recovery.png")]
    Recovery = 1 << 10,

    [EnumText("スタミナ系"), IconPath("/img/mhws/icon/skill/stamina.png")]
    Stamina = 1 << 11,

    [EnumText("アイテム系"), IconPath("/img/mhws/icon/skill/item.png")]
    Item = 1 << 12,

    [EnumText("探索系"), IconPath("/img/mhws/icon/skill/gathering.png")]
    Gathering = 1 << 13,
}

public static class ExIconType
{
    public static IEnumerable<SelectListItem> Sli(Icon type) => ExEnum.GetIter<Icon>()
        .Select(t => new SelectListItem { Value = ((int)t).ToString(), Selected = t == type, Text = t.GetText() });
}
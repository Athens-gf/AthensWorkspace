using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models;
using AthensWorkspace.Models.MHWs;
using AthensWorkspace.ViewModels.MHWs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Utility;
using static AthensWorkspace.ViewModels.MHWs.ExAmulet;

// ReSharper disable AccessToModifiedClosure

namespace AthensWorkspace.Controllers;

public class MHWsController(
    MHWsDbContext mhwsDbContext,
    UserManager<OAuthUser> userManager,
    IConfiguration configuration
) : AdminController(userManager, configuration)
{
    public IActionResult Amulet() => View(new AmuletVm(mhwsDbContext));

    private static List<Slots> CalcSelectableSlots(List<AmuletPattern> basePatterns, Rare? rare) =>
        basePatterns.Where(pattern => rare == null || pattern.Rare == rare).Select(pattern => pattern.Slots).Distinct().ToList();

    public IActionResult GuessLevels(
        byte? rawRare, string? rawSlots,
        short? skillId1, byte? level1,
        short? skillId2, byte? level2,
        short? skillId3, byte? level3
    )
    {
        var (patterns, rare, slots) = ToRareSlots(mhwsDbContext, rawRare, rawSlots);
        var groups = mhwsDbContext.AmuletSkillGroup.ToList();
        var baseSkills = mhwsDbContext.Skill.ToList();
        var skills = ToSkills(skillId1, level1, skillId2, level2, skillId3, level3);
        return Json(ExAmulet.GuessLevels(patterns, groups, baseSkills, rare, slots, skills));
    }

    public IActionResult JudgeAmulet(
        byte? rawRare, string? rawSlots,
        short? skillId1, byte? level1,
        short? skillId2, byte? level2,
        short? skillId3, byte? level3
    )
    {
        var (patterns, rare, slots) = ToRareSlots(mhwsDbContext, rawRare, rawSlots);
        var groups = mhwsDbContext.AmuletSkillGroup.ToList();
        var baseSkills = mhwsDbContext.Skill.ToList();
        var skills = ToSkills(skillId1, level1, skillId2, level2, skillId3, level3);
        return ExAmulet.JudgeAmulet(patterns, groups, baseSkills, rare, slots, skills);
    }

    public IActionResult JudgeAmulet_(
        byte? rawRare, string? rawSlots,
        short? skillId1, byte? level1,
        short? skillId2, byte? level2,
        short? skillId3, byte? level3
    )
    {
        Rare? rare = rawRare != null ? (Rare)rawRare : null;
        var basePatterns = mhwsDbContext.AmuletPattern.Where(pattern => rare == null || pattern.Rare == rare).ToList();
        var usablePatterns = basePatterns.ToList();
        var selectableSlots = CalcSelectableSlots(basePatterns, rare);
        var slotsDic = usablePatterns.Select(pattern => pattern.Slots).Distinct().ToDictionary(ss => ss.ValStr);
        Slots? slots = null;
        if (rawSlots != null && slotsDic.TryGetValue(rawSlots, out var tmpSlots))
        {
            slots = tmpSlots;
            var rares = usablePatterns.Where(pattern => pattern.Slots == tmpSlots).Select(pattern => pattern.Rare).Distinct().ToList();
            if (rares.Count == 1)
            {
                rare = rares[0];
                selectableSlots = CalcSelectableSlots(basePatterns, rare);
            }
        }

        usablePatterns = usablePatterns.Where(pattern =>
        {
            var ss = pattern.Slots;
            return slots == null ? selectableSlots.Contains(ss) : slots == ss;
        }).ToList();
        var allGroupIds = usablePatterns.SelectMany(pattern => pattern.GroupIds).ToOrderedList(groupId => groupId != 0);
        var selectableGroupIdsDic = MakeSerialDic(_ => allGroupIds.ToList());
        selectableGroupIdsDic[1] = selectableGroupIdsDic[1].Where(groupId => groupId <= 4).ToList();
        var baseGroups = mhwsDbContext.AmuletSkillGroup.Where(group => allGroupIds.Contains(group.Id)).ToList();
        var usableGroups = baseGroups.ToList();
        var skillIdWithLevel2GroupIds = usableGroups.ToGroupDictionary(group => (group.SkillId, group.Level), group => group.Id);

        var skills = new List<(short? skillId, byte? level)> { (skillId1, level1), (skillId2, level2), (skillId3, level3) }
            .ZipWithIndexBase1().Where(tuple => tuple.value.skillId != null).Select(tuple => (skillId: (short)tuple.value.skillId!, tuple.value.level, tuple.index)).ToList();
        var decidedSkillTuples = skills.Where(tuple => tuple is { level: not null })
            .Select(tuple => (tuple.skillId, level: (byte)tuple.level!, tuple.index)).ToList();
        var predictedGroupIdsDic = decidedSkillTuples
            .Select(tuple => (tuple.index, groupIds: skillIdWithLevel2GroupIds.TryGetValue((tuple.skillId, tuple.level), out var groupId) ? groupId : []))
            .ToDictionary();

        List<List<byte>> decidedGroupIds = [];
        List<List<byte>> decidedGroupIdPairs = [];

        switch (predictedGroupIdsDic.Count)
        {
            case 1:
            {
                var (_, decidedGroupIdsA) = predictedGroupIdsDic.First();
                decidedGroupIds = [decidedGroupIdsA];
                decidedGroupIdPairs = [decidedGroupIdsA];
                break;
            }
            case 2:
            case 3:
            {
                var (_, decidedGroupIdsA) = predictedGroupIdsDic.First();
                var (_, decidedGroupIdsB) = predictedGroupIdsDic.Skip(1).First();
                decidedGroupIds = [decidedGroupIdsA, decidedGroupIdsB];
                decidedGroupIdPairs = decidedGroupIdsA.SelectMany(groupIdA => decidedGroupIdsB.Select(groupIdB => new List<byte> { groupIdA, groupIdB })).ToList();
                break;
            }
        }

        usablePatterns = usablePatterns
            .Where(pattern => decidedGroupIds.All(groupIds => pattern.GroupIds.Any(groupIds.Contains)))
            .ToList();

        allGroupIds = usablePatterns
            .Select(pattern => pattern.GroupIds.Where(groupId => groupId != 0).OrderBy().ToArray())
            .DistinctBy(groupIds => groupIds.Join("-"))
            .SelectMany(baseGroupIds => decidedGroupIdPairs.Count == 0
                ? baseGroupIds
                : decidedGroupIdPairs.SelectMany(removingGroupIds =>
                {
                    if (!removingGroupIds.All(baseGroupIds.Contains)) return [];
                    var groupIds = baseGroupIds.ToList();
                    removingGroupIds.ForEach(groupId => groupIds.Remove(groupId));
                    return groupIds;
                })).ToOrderedList();

        switch (predictedGroupIdsDic.Count)
        {
            case 1:
            {
                var (serialA, _) = predictedGroupIdsDic.First();
                selectableGroupIdsDic = selectableGroupIdsDic.Select(pair => (pair.Key, serialA == pair.Key ? pair.Value : allGroupIds.ToList())).ToDictionary();
                break;
            }
            case 2:
            case 3:
            {
                var (serialA, _) = predictedGroupIdsDic.First();
                var (serialB, _) = predictedGroupIdsDic.Skip(1).First();
                selectableGroupIdsDic[3] = allGroupIds.ToList();
                selectableGroupIdsDic.Remove(serialB);
                break;
            }
        }

        var remainingRares = usablePatterns.Select(pattern => pattern.Rare).Distinct().ToList();
        if (rare == null && remainingRares.Count == 1)
        {
            rare = remainingRares[0];
            selectableSlots = CalcSelectableSlots(basePatterns, rare);
        }

        usableGroups = usableGroups.Where(group => allGroupIds.Contains(group.Id)).ToList();

        short? selectedSkillId = skills.Where(tuple => tuple is { level: null }).Select(tuple => tuple.skillId).ToList().FirstOrDefault();
        var skillId2Groups = usableGroups.ToGroupDictionary(group => group.SkillId);
        List<byte> selectableLevels = [];
        if (selectedSkillId is { } skillId && skillId2Groups.TryGetValue(skillId, out var groups))
            selectableLevels = groups.ToOrderedList(group => group.Level);

        var resetSkillSerials = predictedGroupIdsDic.Where(pair => pair.Value.Count == 0).Select(pair => pair.Key).ToList();
        return Json(new
        {
            rare, slots = slots?.Id, selectableSlots = selectableSlots.Select(s => s.Id), selectableLevels, selectableGroupIdsDic, resetSkillSerials, predictedGroupIdsDic
        });
    }

    public IActionResult ImportAmulets(string input)
    {
        var basePatterns = mhwsDbContext.AmuletPattern.ToList();
        var patternsDic = basePatterns.ToDictionary(pattern => (pattern.Group1, pattern.Group2, pattern.Group3, pattern.Slots.ValStr), pattern => (byte)pattern.Rare);
        var baseGroups = mhwsDbContext.AmuletSkillGroup.ToList();
        var groupDic = baseGroups.ToGroupDictionary(group => (group.SkillId, group.Level), group => group.Id);
        var skillNameDic = mhwsDbContext.Skill.ToDictionary(skill => skill.Name, skill => skill);

        var amulets = input.Split('\n')
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
            {
                var strings = line.Split(',');
                var slots = strings[^6..].Join(",");
                var skills = Enumerable.Range(0, 3)
                    .Select(i => new { name = strings[i * 2], level = byte.TryParse(strings[i * 2 + 1], out var level) ? level : (byte)0 })
                    .Where(arg => arg.level != 0 && skillNameDic.ContainsKey(arg.name))
                    .Select(arg => arg with { level = Math.Min(skillNameDic[arg.name].MaxLevel, arg.level) })
                    .ToList();
                var skillIdWithLevelList = skills.Select(arg => (skillNameDic[arg.name].Id, arg.level)).ToList();
                var rare = 0;
                if (skillIdWithLevelList.Count > 1 &&
                    groupDic.TryGetValue(skillIdWithLevelList[0], out var groupIds1) &&
                    groupDic.TryGetValue(skillIdWithLevelList[1], out var groupIds2))
                {
                    if (skillIdWithLevelList.Count <= 2 || !groupDic.TryGetValue(skillIdWithLevelList[2], out var groupIds3)) groupIds3 = [0];
                    var tuples = groupIds1
                        .SelectMany(groupId1 => groupIds2.SelectMany(groupId2 => groupIds3.Select(groupId3 => (groupId1, groupId2, groupId3, slots))))
                        .ToList();
                    foreach (var tuple in tuples)
                    {
                        if (!patternsDic.TryGetValue(tuple, out var value)) continue;
                        rare = value;
                        break;
                    }
                }

                return new { id = Guid.NewGuid().ToString(), rare, slots, skills };
            })
            .Where(arg => arg.rare != 0).ToList();
        return Json(amulets);
    }
}
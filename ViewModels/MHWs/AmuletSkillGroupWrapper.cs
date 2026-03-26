using AthensWorkspace.MHWs.Models;
using AthensWorkspace.Models.MHWs;

namespace AthensWorkspace.ViewModels.MHWs;

public struct AmuletSkillGroupWrapper(AmuletSkillGroup skillGroup, Skill? skill)
{
    public AmuletSkillGroup SkillGroup { get; init; } = skillGroup;
    public Skill? Skill { get; set; } = skill;
}
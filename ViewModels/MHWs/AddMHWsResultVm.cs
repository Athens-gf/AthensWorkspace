using System.ComponentModel.DataAnnotations;

namespace AthensWorkspace.MHWs.ViewModels.Database;

public class AddMHWsResultVm
{
    [Display(Name = "スキル")] public int Skills { get; set; }
    [Display(Name = "護石パターン")] public int AmuletPatterns { get; set; }
    [Display(Name = "護石スキルグループ")] public int AmuletSkillGroups { get; set; }
}
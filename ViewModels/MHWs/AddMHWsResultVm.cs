using System.ComponentModel.DataAnnotations;

namespace AthensWorkspace.MHWs.ViewModels.Database;

public class AddMHWsResultVm
{
    [Display(Name = "スキル")] public int Skills { get; set; }
}
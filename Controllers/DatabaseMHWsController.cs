using AthensWorkspace.Controllers;
using AthensWorkspace.MHWs.Data;
using AthensWorkspace.MHWs.Models;
using AthensWorkspace.MHWs.ViewModels.Database;
using AthensWorkspace.MHWs.ViewModels.DatabaseFromExcel;
using AthensWorkspace.Models;
using AthensWorkspace.Models.Database;
using AthensWorkspace.ViewModels.DatabaseFromExcel;
using AthensWorkspace.ViewModels.MHWs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Utility;
using Utility.Excel;

namespace AthensWorkspace.MHWs.Controllers.DatabaseFromExcel;

[Authorize]
public class DatabaseMHWsController(
    MHWsDbContext mhwsDbContext,
    UserManager<OAuthUser> userManager,
    IConfiguration configuration
) : AdminController(userManager, configuration)
{
    public IActionResult Index()
    {
        if (IsAdmin()) return View();
        return RedirectToAction("Index", "Account");
    }

    public IActionResult List() => CheckAdminRedirect(() => View(new AddMHWsResultVm()));
    public IActionResult AddResult(AddMHWsResultVm addResult) => CheckAdminRedirect(() => View(addResult));

    [ValidateAntiForgeryToken]
    public IActionResult Upload(UploadFileModels fileModel)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");

        var uploadFile = fileModel.UploadFile;
        if (uploadFile.Length <= 0) return RedirectToAction(nameof(Index));
        var fileName = Path.GetFileName(uploadFile.FileName);

        if (Path.GetExtension(fileName) != ".xlsx") return RedirectToAction(nameof(Index));

        var dataMatrixDic = ExcelReader.Read(uploadFile.OpenReadStream());
        var addCountDic = new Dictionary<string, int>();

        var uploadList = new[]
        {
            (SkillUpVm.SheetName, (Func<DataMatrix, IUploadItemVm>)(matrix => new SkillUpVm(mhwsDbContext, matrix))),
            (AmuletPatternUpVm.SheetName, (Func<DataMatrix, IUploadItemVm>)(matrix => new AmuletPatternUpVm(mhwsDbContext, matrix))),
            (AmuletSkillGroupUpVm.SheetName, (Func<DataMatrix, IUploadItemVm>)(matrix => new AmuletSkillGroupUpVm(mhwsDbContext, matrix))),
        };

        foreach (var (sheetName, ctor) in uploadList)
        {
            if (!dataMatrixDic.TryGetValue(sheetName, out var matrix)) continue;
            Console.WriteLine(sheetName);
            Console.WriteLine(matrix.Rows.Count);
            var upload = ctor(matrix);
            if (upload.ErrorContextOpt.NonEmpty)
                return Content(upload.ErrorContextOpt.Get);
            if (upload.AddItemCount != 0)
            {
                upload.AddItems(mhwsDbContext);
                if (addCountDic.ContainsKey(upload.BaseName))
                    addCountDic[upload.BaseName] += upload.AddItemCount;
                else
                    addCountDic[upload.BaseName] = upload.AddItemCount;
            }

            if (upload.HasUpdateItem)
                return View($"Update{upload.BaseName}", upload);
        }

        // 追加していない場合Listに移動
        if (addCountDic.Values.All(i => i == 0)) return RedirectToAction(nameof(List));

        var addResult = new AddMHWsResultVm
        {
            Skills = addCountDic.GetOrElse("Skills", 0),
            AmuletSkillGroups = addCountDic.GetOrElse("AmuletSkillGroups", 0),
            AmuletPatterns = addCountDic.GetOrElse("AmuletPatterns", 0),
        };
        return RedirectToAction(nameof(AddResult), addResult);
    }

    public IActionResult UpdateItem(IUploadItemVm uploadData)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        if (!ModelState.IsValid) return RedirectToAction(nameof(Index));
        if (uploadData.HasUpdateItem)
            uploadData.UpdateItems(mhwsDbContext);
        return uploadData.IsRemainData
            ? RedirectToAction(nameof(Index))
            : RedirectToAction($"List{uploadData.BaseName}");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Update([Bind] IUploadItemVm data) => UpdateItem(data);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateSkills([Bind] SkillUpVm data) => UpdateItem(data);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAmuletSkillGroups([Bind] AmuletSkillGroupUpVm data) => UpdateItem(data);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult UpdateAmuletPatterns([Bind] AmuletPatternUpVm data) => UpdateItem(data);

    public IActionResult ListSkills() => CheckAdminRedirect(() => View(mhwsDbContext.Skill.ToList()));

    public IActionResult ListAmuletSkillGroups() =>
        CheckAdminRedirect(() =>
        {
            var amuletSkillGroups = mhwsDbContext.AmuletSkillGroup.ToList();
            return View(amuletSkillGroups.Select(group => new AmuletSkillGroupWrapper(group, mhwsDbContext.Skill.Find(group.SkillId))));
        });

    public IActionResult ListAmuletPatterns() => CheckAdminRedirect(() => View(mhwsDbContext.AmuletPattern.ToList()));

    public static IEnumerable<SelectListItem> IconSli(Icon icon) => ExEnum
        .GetIter<Icon>().Select(i => new SelectListItem { Value = ((int)i).ToString(), Text = i.GetText(), Selected = icon == i });

    public IActionResult EditSkill(short id) => CheckAdminRedirect(() =>
    {
        var skill = mhwsDbContext.Skill.Find(id);
        if (skill == null) return RedirectToAction(nameof(ListSkills));
        return View(skill);
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditSkill([Bind] Skill skill) => CheckAdminRedirect(() =>
    {
        if (!ModelState.IsValid) return View(skill);
        mhwsDbContext.Update(skill);
        mhwsDbContext.SaveChanges();
        return View(skill);
    });

    public IActionResult RemoveAmuletSkillGroups() => CheckAdminRedirect(() =>
    {
        var groups = mhwsDbContext.AmuletSkillGroup.ToList();
        mhwsDbContext.RemoveRange(groups);
        mhwsDbContext.SaveChanges();
        return RedirectToAction(nameof(ListAmuletSkillGroups));
    });

    public IActionResult RemoveAmuletPatterns() => CheckAdminRedirect(() =>
    {
        var patterns = mhwsDbContext.AmuletPattern.ToList();
        mhwsDbContext.RemoveRange(patterns);
        mhwsDbContext.SaveChanges();
        return RedirectToAction(nameof(ListAmuletPatterns));
    });

    public IActionResult Download()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Home");
        var bookHelper = new BookHelper();

        var skills = mhwsDbContext.Skill.ToList();
        var maxLevel = skills.Select(skill => skill.MaxLevel).Max();
        var explanationByLevelHeader = Enumerable.Range(1, maxLevel).Select(l => $"個別説明{l}").ToList();
        var skillHeader = SkillUpVm.BaseHeader;
        skillHeader.AddRange(explanationByLevelHeader);
        bookHelper.GetSheet(SkillUpVm.SheetName).WriteAsTable(skillHeader, skills, (skill, dic) =>
        {
            dic["名前"] = skill.Name;
            dic["読み"] = skill.Ruby;
            dic["種別"] = skill.Type.GetText();
            dic["連番"] = skill.Order;
            dic["アイコン"] = skill.Icon.ToString();
            dic["最大Lv"] = skill.MaxLevel;
            dic["説明"] = skill.Explanation;
            var explanationByLevel = skill.ExplanationByLevel();
            dic["個別説明"] = explanationByLevel.Join("@");
            foreach (var (header, explanation) in explanationByLevelHeader.Zip(explanationByLevel))
                dic[header] = explanation;
        });

        var dicSkill = skills.ToDictionary(skill => skill.Id, skill => skill.Name);
        bookHelper.GetSheet(AmuletSkillGroupUpVm.SheetName).WriteAsTable(AmuletSkillGroupUpVm.Header, mhwsDbContext.AmuletSkillGroup.ToList(), (group, dic) =>
        {
            dic["グループ"] = group.Id;
            dic["スキル"] = dicSkill[group.SkillId];
            dic["レベル"] = group.Level;
        });

        bookHelper.GetSheet(AmuletPatternUpVm.SheetName).WriteAsTable(AmuletPatternUpVm.Header, mhwsDbContext.AmuletPattern.ToList(), (pattern, dic) =>
        {
            dic["Rare"] = (byte)pattern.Rare;
            dic["Group1"] = pattern.Group1;
            dic["Group2"] = pattern.Group2;
            dic["Group3"] = pattern.Group3;

            dic["Slot1"] = ToStr(pattern.Slot1);
            dic["Slot2"] = ToStr(pattern.Slot2);
            dic["Slot3"] = ToStr(pattern.Slot3);
            return;

            string ToStr(Slot slot)
            {
                if (slot == Slot.None) return "";
                if (slot.IsWeapon()) return "W" + slot.GetSize();
                return "" + slot.GetSize();
            }
        });

        var fileName = $"MHWs_{DateTimeUtil.NowJst:yyyyMMdd_HHmmss}.xlsx";
        return bookHelper.File(fileName);
    }
}
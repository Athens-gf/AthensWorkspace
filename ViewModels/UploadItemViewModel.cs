using Microsoft.EntityFrameworkCore;
using Utility.Excel;
using Utility;
using Utility.Option;
using static Utility.Option.ExOption;

// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace AthensWorkspace.ViewModels.DatabaseFromExcel;

public class DisplayUpdateItem<T>
{
    protected T ItemDb { get; }
    protected T ItemExcel { get; }

    protected DisplayUpdateItem((T, T) item)
    {
        (ItemDb, ItemExcel) = item;
    }

    protected string MakeDisplay<TV>(Func<T, TV> getter) => MakeDisplay(getter, v => v?.ToString() ?? "");

    protected string MakeDisplay<TV>(Func<T, TV> getter, Func<TV, string> toString)
    {
        var valueDb = getter(ItemDb);
        var valueExcel = getter(ItemExcel);
        if (typeof(TV) == typeof(string))
        {
            return (valueDb?.ToString() ?? "").Equals(valueExcel?.ToString() ?? "")
                ? $"<span>{toString(valueDb)}</span>"
                : toString(valueDb).DiffHtml(toString(valueExcel));
        }

        if (valueDb == null || !valueDb.Equals(valueExcel))
        {
            return
                $"<span style=\"color: blue;\">{toString(valueDb)}</span>→<span style=\"color: red;\">{toString(valueExcel)}</span>";
        }

        return $"<span>{toString(valueDb)}</span>";
    }
}

public interface IUploadItemVm
{
    string BaseName { get; }
    void AddItems(DbContext context);
    void UpdateItems(DbContext context);
    IOption<string> ErrorContextOpt { get; }
    bool HasUpdateItem { get; }
    int AddItemCount { get; }
    bool IsRemainData { get; }
}

public abstract class UploadItemVm<D, T> : IUploadItemVm where D : DbContext where T : new()
{
    public abstract string BaseName { get; }
    public List<T> UpdateDbItems { get; set; } = [];
    public List<T> UpdatedItems { get; set; } = [];
    public List<T> AddedItems { get; init; } = [];
    public bool IsRemainData { get; set; }

    public IOption<string> ErrorContextOpt { get; init; } = None<string>();
    public bool HasUpdateItem => UpdatedItems.Count != 0;
    public int AddItemCount => AddedItems.Count;

    protected UploadItemVm()
    {
    }

    protected UploadItemVm(D context, DataMatrix matrix,
        List<(string, string)> convertProjections,
        IOption<Dictionary<string, Func<string, object>>> makeConvertExtraDicOpt,
        Func<T, object> getName, Func<T, object> getEqual, Func<T, object> getId, Action<T, object> setId,
        string target, Func<D, List<T>> getDbItems, int maxCount = 10)
    {
        var readItems = makeConvertExtraDicOpt.NonEmpty
            ? matrix.Convert<T>(convertProjections, makeConvertExtraDicOpt.Get)
            : matrix.Convert<T>(convertProjections);

        if (readItems.Count != readItems.Select(getName).Distinct().Count())
        {
            var duplications = readItems.Select(getName).GroupBy(name => name).Where(g => g.Count() != 1)
                .Select(grouping => grouping.Key)
                .Join("、");
            ErrorContextOpt = Some<string>($"{target}に重複があります。\n{duplications}");
            return;
        }

        var dbItems = getDbItems(context);

        AddedItems = [];
        var updateItems = new List<T>();
        var updateDbItems = new List<T>();

        readItems.ForEach(item =>
        {
            var dbItem = dbItems.FirstOrDefault(m => getEqual(m).Equals(getEqual(item)));
            if (dbItem == null)
                AddedItems.Add(item);
            else if (!dbItem.Equals(item))
            {
                setId(item, getId(dbItem));
                updateItems.Add(item);
                updateDbItems.Add(dbItem);
            }
        });
        UpdatedItems = updateItems.Take(maxCount).ToList();
        UpdateDbItems = updateDbItems.Take(maxCount).ToList();
        IsRemainData = updateItems.Count > maxCount;
    }

    public abstract void AddItems(DbContext context);
    public abstract void UpdateItems(DbContext context);
}
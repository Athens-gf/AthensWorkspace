using System.Reflection;

namespace Utility.Excel;

public class DataMatrix
{
    public List<string> Keys { get; init; } = null!;
    public List<Dictionary<string, object>> Rows { get; init; } = null!;

    public bool IsNoneData =>
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        Rows.FirstOrDefault()?.All(pair => pair.Value == null || (pair.Value.ToString()?.IsNullOrEmpty() ?? true)) ??
        true;

    private static void SetProperty<T>(T obj, string target, object value, bool isNullNum)
    {
        var prop = typeof(T).GetProperty(target, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (prop == null) return;

        if (isNullNum && (prop.PropertyType == typeof(byte) ||
                          prop.PropertyType == typeof(short) ||
                          prop.PropertyType == typeof(int) ||
                          prop.PropertyType == typeof(float)))
            return;

        prop.SetValue(obj, System.Convert.ChangeType(value, prop.PropertyType));
    }


    public List<T> Convert<T>() where T : new() => Convert<T>([]);

    public List<T> Convert<T>(List<(string, string)> convertProjections) where T : new() =>
        Convert<T>(convertProjections, new Dictionary<string, Func<string, object>>());

    public List<T> Convert<T>(List<(string, string)> convertProjections,
        Dictionary<string, Func<string, object>> extra)
        where T : new()
    {
        var convertProjectionKeys = convertProjections.Select(t => t.Item1);
        return Rows.Select(row =>
        {
            var obj = new T();
            foreach (var (key, target) in Keys.Where(key => !convertProjectionKeys.Contains(key))
                         .Select(key => (key, key)).Concat(convertProjections))
            {
                var value = row.GetOrElse(key, "");
                var valStr = System.Convert.ToString(value)?.Trim() ?? "";
                var isNullNum = string.IsNullOrWhiteSpace(valStr);
                if (extra.TryGetValue(target, out var func))
                {
                    isNullNum = false;
                    value = func(valStr);
                }

                SetProperty(obj, target, value, isNullNum);
            }

            return obj;
        }).ToList();
    }
}
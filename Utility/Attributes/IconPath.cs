using AthensWorkspace.Utility;

namespace Utility;

[AttributeUsage(AttributeTargets.Field)]
public class IconPath(string path) : Attribute
{
    public string Path { get; } = path;
}

public static class ExIcon
{
    private static readonly Dictionary<Enum, string> PathCache = new();

    public static string GetPath(this Enum instance) =>
        ExAttribute.GetValue<IconPath, string>(instance, PathCache, icon => icon.Path, e => e.ToString(), ss => string.Join("、", ss));
}
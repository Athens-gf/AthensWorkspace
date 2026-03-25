using AthensWorkspace.Utility;

namespace Utility;

[AttributeUsage(AttributeTargets.Field)]
public class Ruby(string ruby) : Attribute
{
    public string Text { get; } = ruby;
}

public static class ExRuby
{
    private static readonly Dictionary<Enum, string> TextCache = new();

    public static string GetRuby(this Enum instance) =>
        ExAttribute.GetValue<Ruby, string>(instance, TextCache, icon => icon.Text, e => e.ToString(), ss => string.Join("、", ss));
}
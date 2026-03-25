using System.Text.Json;

// ReSharper disable MemberCanBePrivate.Global

namespace Utility;

public static class Utility
{
    public static string? Omit(this string? str, int length, string suffix = "…")
    {
        if (str == null) return null;
        if (str.Length <= length) return str;
        return str[..(length - 1)] + suffix;
    }

    private static readonly string[] BootstrapColors =
        ["primary", "secondary", "success", "danger", "warning", "info", "light", "dark"];

    public static string GetRepeatedBootstrapColor(int index) => BootstrapColors[index % BootstrapColors.Length];

    public static bool TryDeserialize<T>(this string? json, out T result) where T : class
    {
        result = null!;
        try
        {
            if (json == null) return false;
            var deserialize = JsonSerializer.Deserialize<T>(json);
            result = deserialize!;
            return deserialize != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static List<T> SomeCreate<T>(this int count, Func<T> maker) => Enumerable
        .Range(0, count).Select(_ => maker()).ToList();

    public static List<T> SomeCreate<T>(this int count) where T : new() => count.SomeCreate(() => new T());

    public static string Remove(this string text, string remove) => text.Replace(remove, "");

    public static string Remove(this string text, IEnumerable<string> removes) =>
        removes.Aggregate(text, (preText, remove) => preText.Remove(remove));

    public static bool EqualLine(this string? str0, string? str1)
    {
        if ((str0 ?? "") == (str1 ?? "")) return true;
        if (str0 == null || str1 == null) return false;
        return str0.Replace("\r", "") == str1.Replace("\r", "");
    }

    //「ぁ」～「より」までと、「ー」「ダブルハイフン」をひらがなとする
    public static bool IsHiragana(this char c) => c is >= '\u3041' and <= '\u309F' or '\u30FC' or '\u30A0';
    public static bool IsHiragana(this string str) => str.All(c => c.IsHiragana());
}
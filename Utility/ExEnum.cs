using Utility.Option;
using static Utility.Option.ExOption;

namespace Utility;

[AttributeUsage(AttributeTargets.Field)]
public class EnumText(string text) : Attribute
{
    public string Text { get; } = text;
}

public static class ExEnum
{
    private static readonly Dictionary<Enum, string> TextCache = new();

    public static string GetText(this Enum instance)
    {
        lock (TextCache)
        {
            if (TextCache.TryGetValue(instance, out var text)) return text;

            var instanceType = instance.GetType();

            string EnumToText(Enum enumElement)
            {
                if (TextCache.TryGetValue(enumElement, out text)) return text;

                var attributes = instanceType.GetField(enumElement.ToString())
                    ?.GetCustomAttributes(typeof(EnumText), true) ?? Array.Empty<object>();
                if (attributes.Length == 0) return instance.ToString();

                var enumText = ((EnumText)attributes[0]).Text;
                TextCache.Add(enumElement, enumText);

                return enumText;
            }

            if (Enum.IsDefined(instanceType, instance))
                return EnumToText(instance);
            if (instanceType.GetCustomAttributes(typeof(FlagsAttribute), true).Length <= 0)
                return instance.ToString();
            {
                var instanceValue = Convert.ToInt64(instance);

                var enums =
                    (from Enum value in Enum.GetValues(instanceType)
                        where (instanceValue & Convert.ToInt64(value)) != 0
                        select value);

                var enumerable = enums as Enum[] ?? enums.ToArray();
                var enumSumValue = enumerable.Sum(Convert.ToInt64);

                if (enumSumValue != instanceValue) return instance.ToString();

                var enumText = string.Join(", ",
                    (from Enum value in enumerable
                        select EnumToText(value)).ToArray());

                TextCache.TryAdd(instance, enumText);

                return enumText;
            }
        }
    }

    public static T GetEnum<T>(this long num) where T : Enum => (T)Enum.ToObject(typeof(T), num);
    public static T GetEnum<T>(this int num) where T : Enum => (T)Enum.ToObject(typeof(T), num);
    public static T GetEnum<T>(this byte num) where T : Enum => (T)Enum.ToObject(typeof(T), num);

    public static IEnumerable<T> SelectEnum<T>(this IEnumerable<long> nums) where T : Enum =>
        nums.Select(num => num.GetEnum<T>());

    public static string GetText<T>(this long num) where T : Enum => num.GetEnum<T>().GetText();

    private static IOption<T> GetHelper<T>(Func<T, bool> checker) where T : Enum
    {
        var list = GetIter<T>().ToList();
        return list.All(e => !checker(e)) ? None<T>() : Some(list.First(checker));
    }

    public static IOption<T> GetEnum<T>(this string str) where T : Enum =>
        GetHelper(new Func<T, bool>(e => e.ToString() == str));

    public static IOption<T> GetEnumByText<T>(this string? str) where T : Enum =>
        GetHelper(new Func<T, bool>(e => e.GetText() == str));

    public static IEnumerable<T> GetIter<T>() where T : Enum => from T e in Enum.GetValues(typeof(T)) select e;
}
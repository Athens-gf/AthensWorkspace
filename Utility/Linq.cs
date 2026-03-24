using Utility.Option;
using static Utility.Option.ExOption;

// ReSharper disable InconsistentNaming

// ReSharper disable UnusedMethodReturnValue.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Utility;

public static class Linq
{
    public static bool IsNullOrEmpty(this string? enumerable) => enumerable == null || !enumerable.Any();
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? enumerable) => enumerable == null || !enumerable.Any();

    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source) =>
        source.SelectMany(list => list);

    public static IEnumerable<T> NotWhere<T>(this IEnumerable<T> source,
        Func<T, bool> predicate) => source.Where(x => !predicate(x));


    public static IEnumerable<T> Distinct<T, TKey>
        (this IEnumerable<T> source, Func<T, TKey> keySelector)
        => source.Distinct(new DelegateComparer<T, TKey>(keySelector));

    public static IEnumerable<T> LinqInsert<T>(this IEnumerable<T> source, int index, T value)
    {
        var list = source.ToList();
        list.Insert(index, value);
        return list;
    }

    public static IEnumerable<T> LinqInsertRange<T>(this IEnumerable<T> source, int index, IEnumerable<T> values)
    {
        var list = source.ToList();
        list.InsertRange(index, values);
        return list;
    }

    public static IEnumerable<(T2, T1)> Swap<T1, T2>(this IEnumerable<(T1, T2)> source) =>
        source.Select(tuple => (tuple.Item2, tuple.Item1));

    public static IEnumerable<(T value, int index)> ZipWithIndex<T>(this IEnumerable<T> source) =>
        source.Select((t, i) => (t, i));

    public static IEnumerable<(T value, bool isFirst)> ZipWithIsFirst<T>(this IEnumerable<T> source) =>
        source.Select((t, i) => (t, i == 0));

    public static IEnumerable<(T value, Func<string, string, string> firstFunc)>
        ZipWithIsFirstFunc<T>(this IEnumerable<T> source) =>
        source.Select((t, i) =>
        {
            string Func(string sFirst, string sOther) => i == 0 ? sFirst : sOther;
            return (t, (Func<string, string, string>)Func);
        });

    public static IEnumerable<(TFirst, TEnum)> ZipWithEnum<TFirst, TEnum>(this IEnumerable<TFirst> first)
        where TEnum : Enum => first.Zip(ExEnum.GetIter<TEnum>(), (f, s) => (f, s));

    #region ZipAll

    private static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, Func<TFirst> getFirstDefault, Func<TSecond> getSecondDefault,
        Func<TFirst, TSecond, TResult> resultSelector)
    {
        var firstList = first.ToList();
        var secondList = second.ToList();
        return firstList.Concat(Enumerable.Range(0, Math.Max(0, secondList.Count - firstList.Count))
                .Select(_ => getFirstDefault()))
            .Zip(secondList.Concat(Enumerable.Range(0, Math.Max(0, firstList.Count - secondList.Count))
                .Select(_ => getSecondDefault())), resultSelector);
    }

    public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TFirst firstDefault, TSecond secondDefault,
        Func<TFirst, TSecond, TResult> resultSelector) =>
        first.ZipAll(second, () => firstDefault, () => secondDefault, resultSelector);

    public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> resultSelector)
        where TFirst : new() where TSecond : new() =>
        first.ZipAll(second, () => new TFirst(), () => new TSecond(), resultSelector);

    public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TFirst firstDefault, Func<TFirst, TSecond, TResult> resultSelector)
        where TSecond : new() =>
        first.ZipAll(second, () => firstDefault, () => new TSecond(), resultSelector);

    public static IEnumerable<TResult> ZipAll<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TSecond secondDefault, Func<TFirst, TSecond, TResult> resultSelector)
        where TFirst : new() =>
        first.ZipAll(second, () => new TFirst(), () => secondDefault, resultSelector);

    public static IEnumerable<TResult> ZipAllWithNull<TFirst, TSecond, TResult>(this IEnumerable<TFirst?> first,
        IEnumerable<TSecond?> second, Func<TFirst, TSecond, TResult> resultSelector)
        where TFirst : class where TSecond : class =>
        first.ZipAll(second, () => null, () => null, resultSelector!);

    public static IEnumerable<(TFirst first, TSecond second)> ZipAll<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TFirst firstDefault, TSecond secondDefault) =>
        first.ZipAll(second, firstDefault, secondDefault, (f, s) => (f, s));

    public static IEnumerable<(TFirst first, TSecond second)> ZipAll<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second) where TFirst : new() where TSecond : new() =>
        first.ZipAll(second, (f, s) => (f, s));

    public static IEnumerable<(TFirst first, TSecond second)> ZipAll<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TFirst firstDefault) where TSecond : new() =>
        first.ZipAll(second, firstDefault, (f, s) => (f, s));

    public static IEnumerable<(TFirst first, TSecond second)> ZipAll<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second, TSecond secondDefault) where TFirst : new() =>
        first.ZipAll(second, secondDefault, (f, s) => (f, s));

    public static IEnumerable<(TFirst first, TSecond second)> ZipAllWithNull<TFirst, TSecond>(
        this IEnumerable<TFirst> first,
        IEnumerable<TSecond> second) where TFirst : class where TSecond : class =>
        first.ZipAllWithNull(second, (f, s) => (f, s));

    #endregion

    /// <summary> リストを文字列化し、結合した文字列を返す </summary>
    /// <param name="source"></param>
    /// <param name="separator">中間文字列</param>
    /// <returns>結合した文字列</returns>
    public static string Join<T>(this IEnumerable<T> source, string separator) =>
        string.Join(separator, source.Select(x => x?.ToString()));

    public static IOrderedEnumerable<T> OrderBy<T>(this IEnumerable<T> source) where T : IComparable<T>
        => source.OrderBy(t => t);

    public static IOrderedEnumerable<T> OrderByDescending<T>(this IEnumerable<T> source) where T : IComparable<T>
        => source.OrderByDescending(t => t);

    public static IOrderedEnumerable<KeyValuePair<TKey, TValue>> OrderBy<TKey, TValue>(
        this Dictionary<TKey, TValue> source) where TKey : IComparable<TKey>
        => source.OrderBy(pair => pair.Key);

    public static IOrderedEnumerable<KeyValuePair<TKey, TValue>> OrderByDescending<TKey, TValue>(
        this Dictionary<TKey, TValue> source) where TKey : IComparable<TKey>
        => source.OrderByDescending(pair => pair.Key);

    /// <summary> リストを文字列化し、結合した文字列を返す </summary>
    /// <param name="source"></param>
    /// <param name="separator">中間文字列</param>
    /// <param name="prefix">文字列が存在する場合、前方につける文字</param>
    /// <param name="suffix">文字列が存在する場合、後方につける文字</param>
    /// <returns>結合した文字列</returns>
    public static string Join<T>(this IEnumerable<T> source, string separator, string prefix, string suffix)
    {
        var list = source.ToList();
        return list.Count != 0 ? prefix + list.Join(separator) + suffix : "";
    }

    public static (List<T>, List<T>) Split<T>(this IEnumerable<T> source, Func<T, bool> splitter)
    {
        var list = source.ToList();
        return (list.Where(splitter).ToList(), list.Where(t => !splitter(t)).ToList());
    }

    public static (List<TA>, List<TB>) Split<T, TA, TB>(this IEnumerable<T> source, Func<T, bool> splitter,
        Func<T, TA> selectorA, Func<T, TB> selectorB)
    {
        var list = source.ToList();
        return (list.Where(splitter).Select(selectorA).ToList(),
            list.Where(t => !splitter(t)).Select(selectorB).ToList());
    }

    /// <summary> 文字列のリストから空文字列を取り除いたリストを返す </summary>
    public static IEnumerable<string> RemoveEmpty(this IEnumerable<string> source) =>
        source.Where(s => !string.IsNullOrEmpty(s));

    /// <summary> 与えられたリストが空だった場合、defaultValueを1つだけ要素として持つリストを得る
    /// から出ない場合は与えられたリストをそのまま帰す</summary>
    /// <param name="source"></param>
    /// <param name="emptyValue">リストが空の場合の値</param>
    public static IEnumerable<T> IfEmpty<T>(this IEnumerable<T> source, T emptyValue)
    {
        var list = source.ToList();
        return list.Count != 0 ? list : [emptyValue];
    }

    /// <summary> 与えられたリストが空だった場合、defaultValueのOptionを1つだけ要素として持つリストを得る
    /// から出ない場合は与えられたリストをそのまま帰す</summary>
    /// <param name="source"></param>
    /// <param name="emptyValue">リストが空の場合の値</param>
    public static IEnumerable<IOption<T>> IfEmpty<T>(this IEnumerable<IOption<T>> source, T emptyValue) =>
        source.IfEmpty(Some(emptyValue));

    /// <summary> 特定の要素と絵それを除いたリストの組み合わせを順に返す </summary>
    public static IEnumerable<(T value, IEnumerable<T> other)> Pickup<T>(this IEnumerable<T> source)
    {
        var list = source.ToList();
        return list.Select(t => (t, list.Where(x => !Equals(t, x))));
    }

    #region Dictionary

    public static IEnumerable<TValue> SelectGet<TKey, TValue>(this IEnumerable<TKey> list,
        IDictionary<TKey, TValue> dic) where TKey : notnull =>
        list.Where(dic.ContainsKey).Select(key => dic[key]);

    public static TValue GetOrElse<TKey, TValue>(this IDictionary<TKey, TValue> dic,
        TKey key, TValue elseValue) where TKey : notnull =>
        dic.TryGetValue(key, out var value) ? value : elseValue;

    /// <summary>
    /// 辞書sourceとupdateを組み合わせた新しい辞書を作成する。
    /// 同じKeyの場合、updateが優先される。
    /// </summary>
    /// <param name="source">基底辞書</param>
    /// <param name="update">優先辞書</param>
    public static Dictionary<TKey, TValue> Update<TKey, TValue>(this IDictionary<TKey, TValue> source,
        IDictionary<TKey, TValue> update) where TKey : notnull =>
        source.Concat(update)
            .GroupBy(pair => pair.Key)
            .ToDictionary(x => x.Key, x => x.Last().Value);

    public static Dictionary<TKey, List<TSource>> ToGroupDictionary<TSource, TKey>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector) where TKey : notnull =>
        source.GroupBy(keySelector).ToDictionary(grouping => grouping.Key, group => group.ToList());

    public static Dictionary<TKey, List<TElement>> ToGroupDictionary<TSource, TKey, TElement>
        (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        where TKey : notnull => source.GroupBy(keySelector)
        .ToDictionary(grouping => grouping.Key, grouping => grouping.Select(elementSelector).ToList());

    public static IEnumerable<List<long>> ToStepGroups
        (this IEnumerable<long> source) => source.ZipWithIndex()
        .ToGroupDictionary(tuple => tuple.value - tuple.index, tuple => tuple.value)
        .OrderBy(pair => pair.Key)
        .Select(pair => pair.Value);

    public static Dictionary<TToKey, TValue> SelectKeys<TPreKey, TToKey, TValue>
        (this IDictionary<TPreKey, TValue> source, Func<TPreKey, TToKey> selector)
        where TPreKey : notnull where TToKey : notnull =>
        source.ToDictionary(pair => selector(pair.Key), pair => pair.Value);

    public static Dictionary<TKey, TTo> SelectValues<TKey, TPre, TTo>
        (this IDictionary<TKey, TPre> source, Func<TPre, TTo> selector) where TKey : notnull =>
        source.ToDictionary(pair => pair.Key, pair => selector(pair.Value));

    public static Dictionary<TKey, List<TValue>> ToDictionary<TKey, TValue>(
        this IEnumerable<IGrouping<TKey, TValue>> source)
        where TKey : notnull => source.ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

    #endregion

    public static IEnumerable<T[]> Permutation<T>(this IEnumerable<T> collection)
    {
        var list = collection.ToList();
        if (list.Count == 1) yield return [list.First()];

        foreach (var item in list)
        {
            var selected = new[] { item };
            var unused = list.Except(selected);

            foreach (var rightSide in unused.Permutation())
            {
                yield return selected.Concat(rightSide).ToArray();
            }
        }
    }

    public static IEnumerable<T[]> Combination<T>(this IEnumerable<T> collection, int k, bool withRepetition = false)
    {
        if (k == 1)
        {
            foreach (var item in collection) yield return [item];
            yield break;
        }

        var list = collection.ToList();
        foreach (var item in list)
        {
            var leftSide = new[] { item };

            // item よりも前のものを除く （順列と組み合わせの違い)
            // 重複を許さないので、unusedから item そのものも取り除く
            var unused = withRepetition
                ? list
                : list.SkipWhile(e => !(e?.Equals(item) ?? item == null)).Skip(1).ToList();

            foreach (var rightSide in unused.Combination(k - 1, withRepetition))
                yield return leftSide.Concat(rightSide).ToArray();
        }
    }
}
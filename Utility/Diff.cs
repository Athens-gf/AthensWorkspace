using System.Drawing;
using Path = System.Collections.Generic.IEnumerable<System.Drawing.Point>;

namespace Utility;

// ReSharper disable once MemberCanBePrivate.Global
public static class DiffExtension
{
    private const char Minus = '-';
    private const char Plus = '+';
    private const char Equal = '=';
    private static readonly Point ZeroPoint = new(x: 0, y: 0);

    private static readonly Dictionary<char, string> ColorDic = new()
        { { Plus, "red" }, { Minus, "blue" }, { Equal, "black" } };

    private static readonly Dictionary<char, string> BackgroundColorDic = new()
        { { Plus, "#FFD5EC" }, { Minus, "#D9E5FF" }, { Equal, "white" } };

    public static string DiffHtml(this string preStr, string toStr,
        Dictionary<char, string>? colorDic = null,
        Dictionary<char, string>? backgroundColorDic = null)
    {
        if (preStr == toStr) return preStr;
        colorDic ??= ColorDic;
        backgroundColorDic ??= BackgroundColorDic;

        var buffer = "";
        for (var diff = preStr.Diff(toStr); diff.Any();)
        {
            var type = diff.First().Value;
            var color = colorDic.GetOrElse(type, colorDic[Equal]);
            var backColor = backgroundColorDic.GetOrElse(type, backgroundColorDic[Equal]);

            buffer += $"<span style='color: {color}; background-color: {backColor};'>";
            diff.TakeWhile(x => x.Value == type)
                .Select(x => $"<ruby>{x.Key}<rt>{(x.Value == Equal ? "" : x.Value)}</rt></ruby>")
                .ToList().ForEach(s => buffer += s);
            buffer += "</span>";

            diff = diff.SkipWhile(x => x.Value == type).ToList();
        }

        return buffer;
    }

    public static IList<KeyValuePair<T, char>> Diff<T>(this IEnumerable<T> pre, IEnumerable<T> to)
    {
        var tempPre = pre.ToList();
        var tempTo = to.ToList();
        var swap = tempPre.Skip(tempTo.Count).Any();
        List<T> listA = !swap ? tempPre : tempTo, listB = swap ? tempPre : tempTo;

        var offset = listA.Count + 1;
        var delta = listB.Skip(listA.Count).Count();
        var size = listA.Concat(listB).Count() + 3;

        var fp = Enumerable.Repeat(-1, size).ToArray();
        var paths = Enumerable.Empty<Path>().ToList();

        for (var p = 0; fp[delta + offset] != listB.Count; ++p)
            paths.AddRange(Enumerable.Range(-p, delta + p)
                .Concat(Enumerable.Range(delta + 1, p).Reverse())
                .Select(k => Snake(listA, listB, ref fp, k, offset)).ToList()
                .Concat(new[] { Snake(listA, listB, ref fp, delta, offset) }));

        return paths.MakeResultData(listA, listB, swap);
    }

    // ------------------------------------------------------------------------
    // 最小編集距離を求めつつ、ついでに経路情報を記録するよ∩( ・ω・)∩
    // 昔のアルゴリズムって一つの関数に複数の目的を詰め込んだ処理が多い気がする
    private static IEnumerable<Point> Snake<T>(IReadOnlyCollection<T> a, IReadOnlyCollection<T> b, ref int[] fp, int k,
        int offset)
    {
        var index = k + offset;
        var snake = Snake(a, b, k, Math.Max(fp[index - 1] + 1, fp[index + 1])).ToList();

        fp[index] = snake.Last().Y;
        return snake;
    }

    private static IEnumerable<Point> Snake<T>(IReadOnlyCollection<T> a, IReadOnlyCollection<T> b, int k, int y)
    {
        var x = y - k;
        return new[] { new Point(x, y) }
            .Concat(PointSequence(x, y)
                .TakeWhile(p => a.Skip(p.X - 1).Any() && b.Skip(p.Y - 1).Any() &&
#pragma warning disable CS8602
                                a.ElementAt(p.X - 1).Equals(b.ElementAt(p.Y - 1)))).ToList();
#pragma warning restore CS8602
    }

    private static IEnumerable<Point> PointSequence(int x, int y)
    {
        for (int tempX = x, tempY = y;;) yield return new Point(++tempX, ++tempY);
        // ReSharper disable once IteratorNeverReturns
    }

    private static Point OffsetX(this Point point, int offset = 1) => point with { X = point.X + offset };

    private static Point OffsetY(this Point point, int offset = 1) => point with { Y = point.Y + offset };

    // ---------------------------------
    // 結果データを作ります∩( ・ω・)∩
    private static IList<KeyValuePair<T, char>> MakeResultData<T>(this List<Path> paths,
        IReadOnlyCollection<T> a, IReadOnlyCollection<T> b, bool swap)
    {
        var prunedPaths = paths.Prune(swap);
        var ret = Enumerable.Empty<KeyValuePair<T, char>>().ToList();
        for (var path = prunedPaths.FirstOrDefault(); path != null;)
        {
            var tail = path.BuildCommonElementData(a, ref ret).Last();
            path = prunedPaths.BuildAddedElementData(tail, b, swap, ref ret) ??
                   prunedPaths.BuildDeletedElementData(tail, a, swap, ref ret);
        }

        return ret;
    }

    private static Path? BuildAddedElementData<T>(this List<Path> paths,
        Point tail,
        IEnumerable<T> sequence,
        bool swap,
        ref List<KeyValuePair<T, char>> resultSequence)
    {
        return paths.BuildChangedElementData(tail, sequence, swap, ref resultSequence,
            isAdded: true);
    }

    private static Path? BuildDeletedElementData<T>(this List<Path> paths,
        Point tail, IEnumerable<T> sequence,
        bool swap,
        ref List<KeyValuePair<T, char>> resultSequence)
    {
        return paths.BuildChangedElementData(tail, sequence, swap, ref resultSequence,
            isAdded: false);
    }

    private static Path? BuildChangedElementData<T>(this IEnumerable<Path> paths,
        Point tail,
        IEnumerable<T> sequence,
        bool swap,
        ref List<KeyValuePair<T, char>> resultSequence,
        bool isAdded)
    {
        var nextStartPoint = isAdded ? tail.OffsetY() : tail.OffsetX();
        var query = paths.Where(x => x.First().Equals(nextStartPoint)).ToList();
        if (!query.Any()) return null;

        var ret = query.First().ToList();
        var index = (isAdded ? ret.First().Y : ret.First().X) - 1;
        resultSequence.Add(new KeyValuePair<T, char>
            (sequence.ElementAt(index), isAdded ^ swap ? Plus : Minus));
        return ret;
    }

    private static Path BuildCommonElementData<T>(this Path path, IEnumerable<T> sequence,
        ref List<KeyValuePair<T, char>> resultSequence)
    {
        var pathList = path.ToList();
        var start = pathList.First().X;
        var count = pathList.Last().X - start;

        resultSequence.AddRange(Enumerable.Range(start, count)
            .Select(x => new KeyValuePair<T, char>(sequence.ElementAt(x), Equal)));

        return pathList;
    }

    // -------------------------------------------
    // 経路情報から余分な枝を刈ります∩( ・ω・)∩
    private static List<Path> Prune(this List<Path> paths, bool swap) =>
        paths.Prune(paths.Last(), swap)?.ToList() ?? new List<Path>();

    private static IEnumerable<Path>? Prune(this List<Path> paths, Path last, bool swap)
    {
        var lastList = last.ToList();
        var startPoint = lastList.First();
        var sequence = new[] { lastList };

        if (startPoint.Equals(ZeroPoint)) return sequence;
        var path = paths
            .Where(p => startPoint.AdjacentPoints(swap).Contains(p.Last()))
            .Select(p => paths.Prune(p, swap))
            .FirstOrDefault(result => result != null);

        return path?.Concat(sequence);
    }

    private static IEnumerable<Point> AdjacentPoints(this Point point, bool swap)
    {
        var x = new[] { point.OffsetX(-1), point.OffsetY(-1) };
        return !swap ? x : x.Reverse();
    }
}
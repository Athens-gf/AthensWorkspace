using Utility.Option.Functional;
using Utility.Option.Linq;
using Utility.Option.Internal;

// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace Utility.Option
{
    /// <summary>
    /// Represents optional values.
    /// </summary>
    /// <typeparam name="T">Inclusion type in <see cref="ExOption"/></typeparam>
    public interface IOption<out T> : IEnumerable<T>
    {
        /// <summary>
        /// If this instance is None, return true, false otherwise.
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// If this instance is None, return false, true otherwise.
        /// </summary>
        bool NonEmpty { get; }

        /// <summary>
        /// return Option's data. If this instance is None, throw ArgumentNullException.
        /// </summary>
        /// <returns></returns>
        T Get { get; }
    }

    /// <summary>
    /// Represents IOption factory methods.
    /// </summary>
    public static class ExOption
    {
        /// <summary>
        /// create None object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IOption<T> None<T>() => new None<T>();

        /// <summary>
        /// create Some object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Inclusion instance in <see cref="ExOption"/></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public static IOption<T> Some<T>(T value) =>
            value != null ? new Some<T>(value) : throw new ArgumentNullException();

        /// <summary>
        /// if <paramref name="value"/> is null, create None, create Some otherwise.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">Inclusion instance in <see cref="ExOption"/></param>
        /// <returns></returns>
        public static IOption<T> Option<T>(T? value) => value != null ? new Some<T>(value) : None<T>();


        /// <summary> フィルタが通ったらSome、それ以外はNone </summary>
        public static IOption<T> Filter<T>(this T value, Func<T, bool> filter) =>
            filter(value) ? Some(value) : None<T>();

        /// <summary> Noneを排除したリストを返す </summary>
        public static IEnumerable<T> NoneFilter<T>(this IEnumerable<IOption<T>> iter) =>
            iter.Where(opt => opt.NonEmpty).Select(opt => opt.Get);

        /// <summary> 要素の中に1つでもNoneがあった場合、None、全てSomeの場合はそのリストを返す </summary>
        /// <returns></returns>
        public static IOption<IEnumerable<T>> FilterNotNone<T>(this IEnumerable<IOption<T>> iter)
        {
            var list = iter.ToList();
            return !list.Any() || list.Any(opt => opt.IsEmpty)
                ? None<IEnumerable<T>>()
                : Some(list.Select(opt => opt.Get));
        }

        /// <summary> リストを1要素(T1)に変換する、要素の中に1つでもNoneがあった場合、デフォルト値を返す </summary>
        public static T1 GetOrElse<T0, T1>(this IEnumerable<IOption<T0>> iter,
            Func<IEnumerable<T0>, T1> converter, T1 noneDefault) =>
            iter.FilterNotNone().Select(converter).GetOrElse(noneDefault);

        /// <summary> リストを1要素(T1)に変換する、要素の中に1つでもNoneがあった場合、Noneを返す </summary>
        public static IOption<T1> OrElse<T0, T1>(this IEnumerable<IOption<T0>> iter,
            Func<IEnumerable<T0>, T1> converter) =>
            iter.FilterNotNone().Select(converter);

        public static IOption<int> ParseIntOpt(this string str) => int.TryParse(str, out var i) ? Some(i) : None<int>();

        public static IOption<float> ParseFloatOpt(this string str) =>
            float.TryParse(str, out var i) ? Some(i) : None<float>();

        public static IOption<T> FirstSome<T>(this IEnumerable<IOption<T>> iter)
        {
            var list = iter.ToList();
            return list.Any(opt => opt.NonEmpty) ? list.First(opt => opt.NonEmpty) : None<T>();
        }

        public static IOption<T> LastSome<T>(this IEnumerable<IOption<T>> iter)
        {
            var list = iter.ToList();
            return list.Any(opt => opt.NonEmpty) ? list.Last(opt => opt.NonEmpty) : None<T>();
        }

        public static IOption<T> FindOpt<T>(this IEnumerable<T> iter, Predicate<T> match) =>
            Option(iter.ToList().Find(match));

        public static IOption<T1> Option<T0, T1>(this IDictionary<T0, T1> dic, T0 key) =>
            dic.ContainsKey(key) ? Some(dic[key]) : None<T1>();

        public static IEnumerable<T> Concat<T>(this IEnumerable<T> dic, IOption<T> opt) =>
            dic.Concat(opt.Select(v => new List<T> { v }).GetOrElse(new List<T>()));

        #region Mix

        public static IOption<(T0, T1)> Mix<T0, T1>(this IOption<T0> opt0, IOption<T1> opt1) =>
            from t0 in opt0
            from t1 in opt1
            select (t0, t1);

        public static IOption<(T0, T1, T2)> Mix<T0, T1, T2>(this IOption<(T0, T1)> opt0, IOption<T2> opt1) =>
            from t0 in opt0
            from t2 in opt1
            select (t0.Item1, t0.Item2, t2);

        public static IOption<(T0, T1, T2, T3)>
            Mix<T0, T1, T2, T3>(this IOption<(T0, T1, T2)> opt0, IOption<T3> opt1) =>
            from t0 in opt0
            from t3 in opt1
            select (t0.Item1, t0.Item2, t0.Item3, t3);

        public static IOption<(T0, T1, T2, T3, T4)>
            Mix<T0, T1, T2, T3, T4>(this IOption<(T0, T1, T2, T3)> opt0, IOption<T4> opt1) =>
            from t0 in opt0
            from t4 in opt1
            select (t0.Item1, t0.Item2, t0.Item3, t0.Item4, t4);

        #endregion
    }
}
using Utility.Option.Functional;

// ReSharper disable UnusedMember.Global

// ReSharper disable ReturnTypeCanBeEnumerable.Global

namespace Utility.Option.Linq
{
    public static class OptionExtensions
    {
        public static IOption<T1> Select<T0, T1>(this IOption<T0> opt, Func<T0, T1> f) => opt.Map(f);

        public static IOption<T1> SelectMany<T0, T1>(this IOption<T0> opt, Func<T0, IOption<T1>> f) => opt.FlatMap(f);

        public static IOption<T2> SelectMany<T0, T1, T2>(this IOption<T0> opt, Func<T0, IOption<T1>> selector,
            Func<T0, T1, T2> resultSelector) => opt.FlatMap(o => selector(o).Map(t => resultSelector(o, t)));

        public static IOption<T> Where<T>(this IOption<T> opt, Func<T, bool> predicate) => opt.Filter(predicate);
    }
}
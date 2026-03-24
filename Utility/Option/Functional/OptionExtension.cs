// ReSharper disable UnusedMember.Global

namespace Utility.Option.Functional
{
    /// <summary>
    /// Declare extension methods of <see>
    ///     <cref>Data.IOption{A}</cref>
    /// </see>
    /// .
    /// </summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Returns a Some containing the result of applying <paramref name="f"/> to this IOption's value if this IOption is nonempty. Otherwise return None.
        /// </summary>
        /// <typeparam name="T0">original type</typeparam>
        /// <typeparam name="T1">type of after applying f</typeparam>
        /// <param name="opt"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IOption<T1> Map<T0, T1>(this IOption<T0> opt, Func<T0, T1> f) =>
            opt.IsEmpty ? ExOption.None<T1>() : ExOption.Option(f(opt.Get));

        /// <summary>
        /// Returns the result of applying <paramref name="f"/> to this value if this is nonempty.
        /// Returns None if this is empty.
        /// Slightly different from <see cref="Map{A, B}(IOption{A}, Func{A, B})"/> in that <paramref name="f"/> is expected to return an IOption(which could be None).
        /// </summary>
        /// <typeparam name="T0"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <param name="opt"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        public static IOption<T1> FlatMap<T0, T1>(this IOption<T0> opt, Func<T0, IOption<T1>> f) =>
            opt.IsEmpty ? ExOption.None<T1>() : f(opt.Get);

        public static IOption<T> Filter<T>(this IOption<T> opt, Func<T, bool> p) =>
            (opt.IsEmpty || p(opt.Get)) ? opt : ExOption.None<T>();

        public static IOption<T> FilterNot<T>(this IOption<T> opt, Func<T, bool> p) =>
            (opt.IsEmpty || !p(opt.Get)) ? opt : ExOption.None<T>();

        public static IOption<T> OrElse<T>(this IOption<T> opt, IOption<T> noneDefaultOpt) =>
            opt.IsEmpty ? noneDefaultOpt : opt;

        public static IOption<T> OrElse<T>(this IOption<T> opt, T noneDefault) =>
            opt.IsEmpty ? ExOption.Option(noneDefault) : opt;

        public static T GetOrElse<T>(this IOption<T> opt, T noneDefault) => opt.IsEmpty ? noneDefault : opt.Get;


        public static IEnumerable<T> ToEnumerable<T>(this IOption<T> opt) =>
            opt.IsEmpty ? Enumerable.Empty<T>() : Enumerable.Repeat(opt.Get, 1);

        public static void ForEach<T>(this IOption<T> opt, Action<T> act)
        {
            if (opt.NonEmpty) act(opt.Get);
        }
    }
}
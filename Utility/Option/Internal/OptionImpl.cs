using System.Collections;

namespace Utility.Option.Internal
{
    /// <summary>
    /// Class `Some[A]` represents existing values of type A
    /// </summary>
    /// <typeparam name="T">Inclusion type in <see cref="ExOption"/></typeparam>
    [Serializable]
    internal sealed class Some<T> : IOption<T>, IEquatable<T>
    {
        public Some(T value)
        {
            if (value == null)
                throw new ArgumentNullException();
            Get = value;
        }

        public bool IsEmpty => false;

        public bool NonEmpty => true;

        public T Get { get; }

        public override string ToString() => $"Some<{typeof(T).Name}>({Get?.ToString()})";

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            yield return Get;
        }

        public IEnumerator GetEnumerator()
        {
            yield return Get;
        }

        private bool Equals(IOption<T> other) => !other.IsEmpty && EqualityComparer<T>.Default.Equals(Get, other.Get);

        public bool Equals(T? other) => EqualityComparer<T>.Default.Equals(Get, other);

        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) || obj is IOption<T> other && Equals(other);

        public override int GetHashCode() =>
            EqualityComparer<T>.Default.GetHashCode(Get ?? throw new InvalidOperationException());
    }

    /// <summary>
    /// Class `None[A]` represents non-existent values.
    /// </summary>
    /// <typeparam name="T">Inclusion type in <see cref="ExOption"/></typeparam>
    [Serializable]
    internal sealed class None<T> : IOption<T>, IEquatable<T>
    {
        public bool IsEmpty => true;

        public bool NonEmpty => false;

        public T Get => throw new NullReferenceException("object is not exist.");

        public override string ToString() => $"None<{typeof(T).Name}>";

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            yield break;
        }

        public IEnumerator GetEnumerator()
        {
            yield break;
        }

        private static bool Equals(IOption<T> other) => other.IsEmpty;

        public bool Equals(T? other) => false;

        public override bool Equals(object? obj) =>
            ReferenceEquals(this, obj) || obj is IOption<T> other && Equals(other);

        public override int GetHashCode() => 0;
    }
}
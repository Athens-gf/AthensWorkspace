namespace Utility;

public class DelegateComparer<T, TKey>(Func<T, TKey> keySelector) : IEqualityComparer<T>
{
    public bool Equals(T? x, T? y) => y != null && x != null && keySelector(x)!.Equals(keySelector(y));
    public int GetHashCode(T obj) => keySelector(obj)!.GetHashCode();
}
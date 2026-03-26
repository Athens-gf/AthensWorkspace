namespace AthensWorkspace.Utility;

public static class ExAttribute
{
    public static T GetValue<A, T>(Enum instance,
        Dictionary<Enum, T> cache, Func<A, T> getter,
        Func<Enum, T> getDef, Func<T[], T> aggregate
    ) where A : Attribute
    {
        lock (cache)
        {
            if (cache.TryGetValue(instance, out var v)) return v;

            var instanceType = instance.GetType();

            T EnumToText(Enum enumElement)
            {
                if (cache.TryGetValue(enumElement, out v)) return v;

                var attributes = instanceType.GetField(enumElement.ToString())
                    ?.GetCustomAttributes(typeof(A), true) ?? [];
                if (attributes.Length == 0) return getDef(instance);

                var value = getter((A)attributes[0]);
                cache.Add(enumElement, value);

                return value;
            }

            if (Enum.IsDefined(instanceType, instance))
                return EnumToText(instance);
            if (instanceType.GetCustomAttributes(typeof(FlagsAttribute), true).Length <= 0)
                return getDef(instance);
            {
                var instanceValue = Convert.ToInt64(instance);

                var enums =
                    from Enum value in Enum.GetValues(instanceType)
                    where (instanceValue & Convert.ToInt64(value)) != 0
                    select value;

                var enumerable = enums as Enum[] ?? enums.ToArray();
                var enumSumValue = enumerable.Sum(Convert.ToInt64);

                if (enumSumValue != instanceValue) return getDef(instance);

                var values = (from Enum value in enumerable select EnumToText(value)).ToArray();
                var aggregateValue = aggregate(values);

                cache.TryAdd(instance, aggregateValue);

                return aggregateValue;
            }
        }
    }
}
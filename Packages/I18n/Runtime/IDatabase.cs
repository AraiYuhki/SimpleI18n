using System;

namespace Xeon.Localization
{
    public interface IDatabase<TData, TEnum>
        where TData : ITranslateDataSet<TEnum>
        where TEnum : Enum
    {
        TData FindByKey(string key);
        bool TryFindByKey(string key, out TData translated);
    }
}

using System;

namespace Xeon.Localization
{
    public interface ITranslateDataSet<T> where T : Enum
    {
        bool Translate(T language, out string result);
    }
}

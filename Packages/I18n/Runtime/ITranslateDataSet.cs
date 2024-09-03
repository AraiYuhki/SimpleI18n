using System;

namespace Xeon.Localization
{
    public interface ITranslateDataSet<T> where T : Enum
    {
        string Text { get; }
        bool Translate(T language, out string result);
    }
}

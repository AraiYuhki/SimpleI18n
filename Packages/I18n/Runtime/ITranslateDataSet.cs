using UnityEngine;

namespace Xeon.Localization
{
    public interface ITranslateDataSet
    {
        string Text { get; }
        bool Translate(SystemLanguage language, out string result);
    }
}

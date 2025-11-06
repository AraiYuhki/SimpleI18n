using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.Localization.Sample
{
    public enum Language { English, Japanese, Chinese, Korean }

    [Serializable]
    public struct TranslateData : ITranslateDataSet<Language>, IKeyEntry
    {
        [SerializeField] private string key;
        [SerializeField] private string ja;
        [SerializeField] private string en;
        [SerializeField] private string zh;
        [SerializeField] private string ko;

        public string Key => key;

        public bool Translate(Language language, out string result)
        {
            switch (language)
            {
                case Language.English:
                    result = en;
                    return !string.IsNullOrEmpty(result);
                case Language.Japanese:
                    result = ja;
                    return !string.IsNullOrEmpty(result);
                case Language.Chinese:
                    result = zh;
                    return !string.IsNullOrEmpty(result);
                case Language.Korean:
                    result = ko;
                    return !string.IsNullOrEmpty(result);
                default:
                    result = key;
                    return false;
            }
        }
    }

    [CreateAssetMenu(fileName = "I18nDictionary", menuName = "Scriptable Objects/I18nDictionary")]
    public class TextDatabase : KeyedDatabaseBase<TranslateData>, IDatabase<TranslateData, Language>
    {
        // 重複していた entries / buildOnEnable / duplicateLastWins の再定義を削除（基底クラスに存在）
        public TranslateData FindByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key is null or empty", nameof(key));
            }
            if (!TryGetEntry(key, out var data))
            {
                throw new KeyNotFoundException($"Key '{key}' not found");
            }
            return data;
        }

        public bool TryFindByKey(string key, out TranslateData translated)
        {
            return TryGetEntry(key, out translated);
        }
#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")] private void RebuildContextMenu() => Prewarm();
#endif
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Xeon.Localization.Sample
{
    public enum Language { English, Japanese, Chinese, Korean }

    [Serializable]
    public struct TranslateData : ITranslateDataSet<Language>
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
    public class I18nDictionary : ScriptableObject, IDatabase<TranslateData, Language>
    {
        [Tooltip("翻訳エントリ一覧。インスペクタで編集。起動時にソート・圧縮されます。")]
        [SerializeField] private TranslateData[] entries;
        [SerializeField, Tooltip("OnEnable 時に自動でソート・キャッシュ構築するかどうか")] private bool buildOnEnable = true;
        [SerializeField, Tooltip("同一キーが複数存在する場合、後勝ちにする (true) / 先勝ちにする (false)")] private bool duplicateLastWins = true;
        [SerializeField, Tooltip("キャッシュ構築後に元配列を破棄してメモリを解放するか")] private bool discardEntriesAfterBuild = true;

        private TranslateData[] _data = Array.Empty<TranslateData>();
        private bool _built;
        private string _lastKey;
        private int _lastIndex = -1;

        private void OnEnable()
        {
            if (buildOnEnable)
                BuildCacheIfNeeded();
        }

        public void Prewarm() => BuildCacheIfNeeded(force: true);

        private void BuildCacheIfNeeded(bool force = false)
        {
            if (_built && !force)
                return;

            if (entries == null || entries.Length == 0)
            {
                _data = Array.Empty<TranslateData>();
                _built = true;
                _lastKey = null;
                _lastIndex = -1;
                return;
            }

            var temp = new List<TranslateData>(entries.Length);
            foreach (var e in entries)
            {
                if (string.IsNullOrEmpty(e.Key))
                    continue;
                temp.Add(e);
            }
            temp.Sort((a, b) => string.Compare(a.Key, b.Key, StringComparison.Ordinal));

            var unique = new List<TranslateData>(temp.Count);
            TranslateData? pending = null;
            string currentKey = null;
            for (int i = 0; i < temp.Count; i++)
            {
                var item = temp[i];
                if (currentKey == null || !string.Equals(item.Key, currentKey, StringComparison.Ordinal))
                {
                    if (pending.HasValue)
                        unique.Add(pending.Value);
                    currentKey = item.Key;
                    pending = item;
                }
                else
                {
                    if (duplicateLastWins) pending = item; // 後勝ち／先勝ち設定
                }
            }
            if (pending.HasValue)
                unique.Add(pending.Value);

            _data = unique.ToArray();
            _built = true;
            _lastKey = null; _lastIndex = -1;
            if (discardEntriesAfterBuild)
                entries = null;
        }

        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            if (!_built)
                BuildCacheIfNeeded();
            
            return FindKeyIndexCached(key) >= 0;
        }

        private int FindKeyIndexCached(string key)
        {
            if (_lastIndex >= 0 && _lastKey == key) return _lastIndex;
            int idx = BinarySearch(_data, key);
            if (idx >= 0) { _lastKey = key; _lastIndex = idx; }
            return idx;
        }

        private static int BinarySearch(TranslateData[] arr, string key)
        {
            int low = 0, high = arr.Length - 1;
            while (low <= high)
            {
                int mid = low + ((high - low) >> 1);
                int cmp = string.Compare(arr[mid].Key, key, StringComparison.Ordinal);
                if (cmp == 0) return mid;
                if (cmp < 0) low = mid + 1; else high = mid - 1;
            }
            return -1;
        }

        private static Language Map(SystemLanguage sys)
        {
            switch (sys)
            {
                case SystemLanguage.Japanese: return Language.Japanese;
                case SystemLanguage.English: return Language.English;
                case SystemLanguage.Chinese:
                case SystemLanguage.ChineseSimplified:
                case SystemLanguage.ChineseTraditional: return Language.Chinese;
                case SystemLanguage.Korean: return Language.Korean;
                default: return Language.English; // デフォルト英語
            }
        }
        
        // IDatabase 実装
        public TranslateData FindByKey(string key)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key is null or empty", nameof(key));
            if (!_built) BuildCacheIfNeeded();
            int idx = BinarySearch(_data, key);
            if (idx < 0) throw new KeyNotFoundException($"Key '{key}' not found");
            return _data[idx];
        }

        public bool TryFindByKey(string key, out TranslateData translated)
        {
            translated = default;
            if (string.IsNullOrEmpty(key)) return false;
            if (!_built) BuildCacheIfNeeded();
            int idx = BinarySearch(_data, key);
            if (idx < 0) return false;
            translated = _data[idx];
            return true;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")] private void RebuildContextMenu() => Prewarm();
        [UnityEditor.CustomEditor(typeof(I18nDictionary))]
        private class I18nDictionaryEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                var dict = (I18nDictionary)target;
                if (GUILayout.Button("Rebuild Cache Now"))
                {
                    dict.Prewarm();
                    UnityEditor.EditorUtility.SetDirty(dict);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
            }
        }
#endif
    }
}

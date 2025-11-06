using System;
using TMPro;
using UnityEngine;

namespace Xeon.Localization.Sample
{
    [Serializable]
    public class TMPFontData : IKeyEntry
    {
        [SerializeField] private string key;
        [SerializeField] private TMP_FontAsset ja;
        [SerializeField] private TMP_FontAsset en;
        [SerializeField] private TMP_FontAsset zh;
        [SerializeField] private TMP_FontAsset ko;

        public string Key => key;

        public bool TryTranslate(Language language, out TMP_FontAsset result)
        {
            switch (language)
            {
                case Language.Japanese:
                    result = ja;
                    return result != null;
                case Language.English:
                    result = en;
                    return result != null;
                case Language.Chinese:
                    result = zh;
                    return result != null;
                case Language.Korean:
                    result = ko;
                    return result != null;
                default:
                    result = null;
                    return false;
            }
        }
    }

    [CreateAssetMenu(fileName = "TMP_FontDatabase", menuName = "Scriptable Objects/TMP_FontDatabase")]
    public class TMP_FontDatabase : KeyedDatabaseBase<TMPFontData>, IAssetDatabase<TMP_FontAsset, Language>
    {
        [SerializeField, Tooltip("存在しないキー指定時に default キーへフォールバックするか")] private bool enableDefaultFallback = true;
        [SerializeField, Tooltip("言語フォントが見つからない場合に英語→日本語の順にフォールバック")] private bool enableLanguageFallback = true;
        public static readonly string DefaultKey = "default";

        private bool TryResolveFont(Language language, TMPFontData fontData, out TMP_FontAsset resolvedFontAsset)
        {
            if (fontData != null && fontData.TryTranslate(language, out resolvedFontAsset))
            {
                return true;
            }
            if (enableLanguageFallback && fontData != null)
            {
                if (language != Language.English && fontData.TryTranslate(Language.English, out resolvedFontAsset))
                {
                    return true;
                }
                if (language != Language.Japanese && fontData.TryTranslate(Language.Japanese, out resolvedFontAsset))
                {
                    return true;
                }
            }
            resolvedFontAsset = null;
            return false;
        }

        public TMP_FontAsset Translate(Language language, string translationKey)
        {
            return TryTranslate(language, translationKey, out var translatedFontAsset) ? translatedFontAsset : null;
        }

        public TMP_FontAsset Translate(string translationKey)
        {
            return TryTranslate(translationKey, out var translatedFontAsset) ? translatedFontAsset : null;
        }

        public bool TryTranslate(Language language, string translationKey, out TMP_FontAsset translatedFontAsset)
        {
            translatedFontAsset = null;
            if (TryGetEntry(translationKey, out var fontData) && TryResolveFont(language, fontData, out translatedFontAsset))
            {
                return true;
            }
            if (enableDefaultFallback && translationKey != DefaultKey && TryGetEntry(DefaultKey, out fontData) && TryResolveFont(language, fontData, out translatedFontAsset))
            {
                return translatedFontAsset != null;
            }
            return false;
        }

        public bool TryTranslate(string translationKey, out TMP_FontAsset translatedFontAsset)
        {
            if (TryTranslate(Language.English, translationKey, out translatedFontAsset))
            {
                return true;
            }
            if (TryTranslate(Language.Japanese, translationKey, out translatedFontAsset))
            {
                return true;
            }
            translatedFontAsset = null;
            return false;
        }
#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")] private void RebuildContextMenu() => Prewarm();
#endif
    }
}

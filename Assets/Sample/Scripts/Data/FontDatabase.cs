using System;
using UnityEngine;

namespace Xeon.Localization.Sample
{
    [Serializable]
    public class FontData : IKeyEntry
    {
        [SerializeField] private string key;
        [SerializeField] private Font ja;
        [SerializeField] private Font en;
        [SerializeField] private Font zh;
        [SerializeField] private Font ko;

        public string Key => key;

        public bool TryTranslate(Language language, out Font result)
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
    
    [CreateAssetMenu(fileName = "FontDatabase", menuName = "Scriptable Objects/FontDatabase")]
    public class FontDatabase : KeyedDatabaseBase<FontData>, IAssetDatabase<Font, Language>
    {
        [SerializeField, Tooltip("存在しないキー指定時に default キーへフォールバックするか")] private bool enableDefaultFallback = true;
        [SerializeField, Tooltip("言語フォントが見つからない場合に英語→日本語の順にフォールバック")] private bool enableLanguageFallback = true;
        public static readonly string DefaultKey = "default";

        private bool TryResolveFont(Language language, FontData fontData, out Font resolvedFont)
        {
            if (fontData != null && fontData.TryTranslate(language, out resolvedFont))
            {
                return true;
            }
            if (enableLanguageFallback && fontData != null)
            {
                if (language != Language.English && fontData.TryTranslate(Language.English, out resolvedFont))
                {
                    return true;
                }
                if (language != Language.Japanese && fontData.TryTranslate(Language.Japanese, out resolvedFont))
                {
                    return true;
                }
            }
            resolvedFont = null;
            return false;
        }

        public Font Translate(Language language, string translationKey)
        {
            return TryTranslate(language, translationKey, out var translatedFont) ? translatedFont : null;
        }

        public Font Translate(string translationKey)
        {
            return TryTranslate(translationKey, out var translatedFont) ? translatedFont : null;
        }

        public bool TryTranslate(Language language, string translationKey, out Font translatedFont)
        {
            translatedFont = null;
            if (TryGetEntry(translationKey, out var fontData) && TryResolveFont(language, fontData, out translatedFont))
            {
                return true;
            }
            if (enableDefaultFallback && translationKey != DefaultKey && TryGetEntry(DefaultKey, out fontData) && TryResolveFont(language, fontData, out translatedFont))
            {
                return translatedFont != null;
            }
            return false;
        }

        public bool TryTranslate(string translationKey, out Font translatedFont)
        {
            if (TryTranslate(Language.English, translationKey, out translatedFont))
            {
                return true;
            }
            if (TryTranslate(Language.Japanese, translationKey, out translatedFont))
            {
                return true;
            }
            translatedFont = null;
            return false;
        }
#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")] private void RebuildContextMenu() => Prewarm();
#endif
    }
}

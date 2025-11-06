using System;
using TMPro;
using UnityEngine.AddressableAssets;

namespace Xeon.Localization.Sample
{
    public class Translator : I18n<TranslateData, Language>
    {
        private static Translator instance;

        public static Translator Instance
        {
            get
            {
                if (instance == null)
                    instance = new();
                instance.Initialize();
                return instance;
            }
        }

        private IAssetDatabase<TMP_FontAsset, Language> tmpFontDatabase;

        private Translator()
        {
        }
        
        private void Initialize()
        {
            if (IsInitialized)
                return;
            var dataset = Addressables.LoadAssetAsync<TextDatabase>(nameof(TextDatabase)).WaitForCompletion();
            SetDatabase(dataset);
            
            var fontData = Addressables.LoadAssetAsync<FontDatabase>(nameof(FontDatabase)).WaitForCompletion();
            RegisterFontDatabase(fontData);
            
            tmpFontDatabase = Addressables.LoadAssetAsync<TMP_FontDatabase>(nameof(TMP_FontDatabase)).WaitForCompletion();
        }
        
        // TMP_FontAsset 翻訳 API
        public TMP_FontAsset TranslateTMPFont(string key)
        {
            ThrowIfTMPFontDatabaseIsNotRegistered();
            return tmpFontDatabase.Translate(key);
        }

        public TMP_FontAsset TranslateTMPFont(Language language, string key)
        {
            ThrowIfTMPFontDatabaseIsNotRegistered();
            return tmpFontDatabase.Translate(language, key);
        }

        public bool TryTranslateTMPFont(string key, out TMP_FontAsset result)
        {
            result = null;
            ThrowIfTMPFontDatabaseIsNotRegistered();
            return tmpFontDatabase.TryTranslate(key, out result);
        }

        public bool TryTranslateTMPFont(Language language, string key, out TMP_FontAsset result)
        {
            result = null;
            ThrowIfTMPFontDatabaseIsNotRegistered();
            return tmpFontDatabase.TryTranslate(language, key, out result);
        }
        
        private void ThrowIfTMPFontDatabaseIsNotRegistered()
        {
            if (tmpFontDatabase == null)
                throw new InvalidOperationException("TMP Font Database is not registered.");
        }
    }
}

using UnityEngine.AddressableAssets;
using Xeon.Localization;

namespace Xeon.Localization.Sample
{
    public class Translator
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
        
        private I18nDictionary dataset;
        private I18n<TranslateData, Language> translator;

        private Translator()
        {
        }

        public string Translate(string key, params (string, object)[] param)
        {
            return translator.Translate(key, param);
        }

        public string Translate(Language language, string key, params (string, object)[] param)
        {
            return translator.Translate(language, key, param);
        }

        public string Translate(string key, int select, params (string, object)[] param)
        {
            return translator.TransChoice(key, select, param);
        }
        
        public string Translate(Language language, string key, int select, params (string, object)[] param)
        {
            return translator.TransChoice(language, key, select, param);
        }

        public string Translate(object language, string key, params (string, object)[] param)
        {
            if (language is Language lang)
                return translator.Translate(lang, key, param);
            return Translate(key, param);
        }

        public string TransChoice(string key, int select, params (string, object)[] param)
        {
            return translator.TransChoice(key, select, param);
        }
        public void SetLanguage(Language language)
        {
            translator.DefaultLanguage = language;
        }
        
        private void Initialize()
        {
            translator ??= new I18n<TranslateData, Language>(dataset, Language.English);
            if (translator.IsInitialized)
                return;
            dataset = Addressables.LoadAssetAsync<I18nDictionary>(nameof(I18nDictionary)).WaitForCompletion();
            translator.SetDatabase(dataset);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Localization.Sample
{
    public class SampleMenu : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown languageSelect;
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_InputField input;
        [SerializeField] private TranslateText[] translateTexts = Array.Empty<TranslateText>();
        [SerializeField] private TranslateTMPText[] translateTMPTexts = Array.Empty<TranslateTMPText>();
        [SerializeField] private Text messageLabel;

        public void Start()
        {
            languageSelect.ClearOptions();
            var options = new List<TMP_Dropdown.OptionData>();
            foreach (var lang in Enum.GetNames(typeof(Language)))
            {
                options.Add(new TMP_Dropdown.OptionData(lang));
            }
            languageSelect.AddOptions(options);
        }

        public void OnChangedLanguage()
        {
            var languages = Enum.GetValues(typeof(Language)).Cast<Language>().ToList();
            Translator.Instance.DefaultLanguage = languages[languageSelect.value];
            foreach (var text in translateTexts)
                text.Refresh((int)slider.value);
            foreach (var text in translateTMPTexts)
                text.Refresh((int)slider.value);
            messageLabel.text = Translator.Instance.Translate("message", ("name", input.text));
        }

        public void OnEndEditInput()
        {
            var text = Translator.Instance.Translate("message", ("name", input.text));
            messageLabel.text = text;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Localization.Sample
{
    public class TranslateText : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private string key;
        [SerializeField] private bool useChoice;
        [SerializeField] private int choiceIndex;
        [SerializeField] private Language selectLanguage;

        private void Start()
        {
            Refresh(0);
        }

        public void Refresh(int choiceIndex)
        {
            this.choiceIndex = choiceIndex;
            if (useChoice)
                text.text = Translator.Instance.Translate(key, choiceIndex);
            else
                text.text = Translator.Instance.Translate(key);
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;
            if (text == null)
                return;
            if (string.IsNullOrEmpty(key))
                return;
            if (useChoice)
                text.text = Translator.Instance.Translate(selectLanguage, key, choiceIndex);
            else
                text.text = Translator.Instance.Translate(selectLanguage, key);
        }
    }
}

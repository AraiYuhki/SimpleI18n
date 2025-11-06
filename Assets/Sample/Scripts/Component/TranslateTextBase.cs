using UnityEngine;

namespace Xeon.Localization.Sample
{

    public abstract class TranslateTextBase : MonoBehaviour
    {
        [SerializeField] protected string key;
        [SerializeField] protected bool useChoice;
        [SerializeField] protected int choiceIndex;
        [SerializeField] protected Language selectLanguage;
        [SerializeField] protected bool translateFont = false;

        protected abstract string text { set; }

        private void Start()
        {
            Refresh(0);
        }

        public void Refresh(int choiceIndex)
        {
            this.choiceIndex = choiceIndex;
            if (useChoice)
                text = Translator.Instance.TransChoice(key, choiceIndex);
            else
                text = Translator.Instance.Translate(key);
        }

        protected virtual void OnValidate()
        {
            if (Application.isPlaying)
                return;
            
            if (string.IsNullOrEmpty(key))
                return;
            
            if (useChoice)
                text = Translator.Instance.TransChoice(selectLanguage, key, choiceIndex);
            else
                text = Translator.Instance.Translate(selectLanguage, key);
        }
    }
}

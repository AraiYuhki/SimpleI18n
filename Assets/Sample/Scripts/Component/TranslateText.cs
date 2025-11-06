using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Xeon.Localization.Sample
{
    public class TranslateText : TranslateTextBase
    {
        [SerializeField] private Text target;

        protected override string text
        {
            set
            {
                if (target == null)
                    return;
                target.text = value;
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (Application.isPlaying || !translateFont)
                return;
            if (target == null)
                return;
            target.font = Translator.Instance.TranslateFont(FontDatabase.DefaultKey);
        }
    }
}

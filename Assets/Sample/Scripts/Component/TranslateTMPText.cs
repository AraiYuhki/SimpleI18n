using TMPro;
using UnityEngine;

namespace Xeon.Localization.Sample
{
    public class TranslateTMPText : TranslateTextBase
    {
        [SerializeField] private TMP_Text target;

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
            target.font = Translator.Instance.TranslateTMPFont(TMP_FontDatabase.DefaultKey);
        }
    }
}

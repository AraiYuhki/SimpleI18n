using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Xeon.Localization.Sample
{
    public class SliderWithLabel : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private TMP_Text label;

        public void OnChangedSlider()
        {
            label.text = slider.value.ToString();
        }
    }
}

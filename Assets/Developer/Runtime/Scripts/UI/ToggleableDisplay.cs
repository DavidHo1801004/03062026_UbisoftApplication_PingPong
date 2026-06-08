using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Developer.UI
{
    [RequireComponent(typeof(LayoutElement))]
    [AddComponentMenu("Developer/UI/Toggleable Display")]
    public class ToggleableDisplay : SizeableDisplay
    {
        [SerializeField]
        private bool isEnabled;

        [Header("REFERENCES")]
        [SerializeField]
        private Image enabledImage;
        [SerializeField]
        private Image disabledImage;

        [Header("EVENTS")]
        [SerializeField]
        private UnityEvent onEnabledEvent;
        [SerializeField]
        private UnityEvent onDisabledEvent;

        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                UpdateDisplay();
            }
        }



#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateDisplay();
        }
#endif



        private void UpdateDisplay()
        {
            if (enabledImage)
                enabledImage.enabled = isEnabled;

            if (disabledImage)
                disabledImage.enabled = !isEnabled;
        }
    }
}

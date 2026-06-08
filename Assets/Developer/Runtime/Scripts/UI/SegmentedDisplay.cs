using LMK.Attribute;
using System.Collections.Generic;
using UnityEngine;

namespace Developer.UI
{
    [AddComponentMenu("Developer/UI/Segmented Display")]
    public class SegmentedDisplay : SizeableDisplay
    {
        [SerializeField]
        private ToggleableDisplay displayPrefab;

        [SerializeField]
        private Transform containerTransform;

        [SerializeField]
        [Min(0)]
        private int maxValue = 3;

        [SerializeField]
        [ReadOnly]
        private int currentValue = 0;

        public int CurrentValue
        {
            get => currentValue;
            set
            {
                value = Mathf.Clamp(value, 0, maxValue);
                if (currentValue == value) return;
                currentValue = value;
                UpdateDisplay();
            }
        }

        public int MaxValue
        {
            get => maxValue;
            set
            {
                value = Mathf.Max(0, value);
                if (maxValue == value) return;
                maxValue = value;
                UpdateDisplay();
            }
        }

        private bool isValid = true;

        private readonly List<ToggleableDisplay> displays = new();



        private void Awake()
        {
            if (displayPrefab == null
                || containerTransform == null)
            {
                isValid = false;
                return;
            }

            displays.Capacity = maxValue;
            UpdateDisplay();
        }



        private void UpdateDisplay()
        {
            if (!isValid) return;

            Debug.Log($"[{name}] {currentValue}/{maxValue}");
            // Add new displayed segments 
            for (int i = displays.Count; i < maxValue; i++)
                AddNewSegmentDisplay();

            for (int i = 0; i < maxValue; i++)
                displays[^1].gameObject.SetActive(true);

            for (int i = maxValue; i < displays.Count; i++)
                displays[^1].gameObject.SetActive(false);

            // Toggle displayed segments
            for (int i = 0; i < currentValue; i++)
                displays[i].IsEnabled = true;

            for (int i = currentValue; i < maxValue; i++)
                displays[i].IsEnabled = false;
        }

        private void AddNewSegmentDisplay()
        {
            var display = Instantiate(displayPrefab);
            display.transform.SetParent(containerTransform, false);
            displays.Add(display);
        }
    }
}

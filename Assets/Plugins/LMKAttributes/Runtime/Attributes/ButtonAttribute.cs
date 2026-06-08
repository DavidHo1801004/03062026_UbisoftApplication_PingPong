using System;
using UnityEngine;

namespace LMK.Attribute
{
    /// <summary>
    /// Expose the target method to the inspector with the given name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ButtonAttribute : PropertyAttribute
    {
        public string Label { get; private set; }

        public ButtonAttribute(string label = null)
        {
            Label = label;
        }
    }
}

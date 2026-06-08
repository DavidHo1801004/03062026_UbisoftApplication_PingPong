using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace LMK.Attribute.Editor
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ButtonAttributeDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            foreach (var method in GetMethods())
            {
                var buttonAttribute = (ButtonAttribute)System.Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
                if (buttonAttribute != null)
                {
                    string buttonLabel = string.IsNullOrEmpty(buttonAttribute.Label) ? method.Name : buttonAttribute.Label;
                    if (GUILayout.Button(buttonLabel))
                    {
                        method.Invoke(target, null);
                    }
                }
            }
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var buttonsContainer = new VisualElement();

            foreach (var method in GetMethods())
            {
                var buttonAttribute = (ButtonAttribute)System.Attribute.GetCustomAttribute(method, typeof(ButtonAttribute));
                if (buttonAttribute != null)
                {
                    var buttonElement = new Button(() => { method.Invoke(target, null); })
                    {
                        text = string.IsNullOrEmpty(buttonAttribute.Label) ? method.Name : buttonAttribute.Label
                    };
                    buttonsContainer.Add(buttonElement);
                }
            }

            root.Add(buttonsContainer);

            return root;
        }



        private MethodInfo[] GetMethods()
        {
            return target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}

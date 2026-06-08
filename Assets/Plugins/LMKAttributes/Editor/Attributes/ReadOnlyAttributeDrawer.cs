using UnityEngine;
using UnityEditor;

namespace LMK.Attribute.Editor
{
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyAttributeDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (ReadOnlyAttribute)attribute;

            SerializedProperty conditionProp =
                property.serializedObject.FindProperty(attr.targetFieldName);

            bool shouldDisable =
                conditionProp == null ||
                (conditionProp.propertyType == SerializedPropertyType.Boolean && conditionProp.boolValue);

            EditorGUI.BeginDisabledGroup(shouldDisable);
            EditorGUI.PropertyField(position, property, label, true);
            EditorGUI.EndDisabledGroup();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}

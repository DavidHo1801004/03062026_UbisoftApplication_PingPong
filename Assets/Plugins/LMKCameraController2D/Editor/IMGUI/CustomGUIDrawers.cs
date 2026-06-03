using UnityEditor;
using UnityEngine;

namespace LMK.Editor
{
    public static class CustomGUIDrawers
    {
        /// <summary>
        /// Draw a extendable and scrollable text field.
        /// </summary>
        /// <param name="_property">        The assigned property. </param>
        /// <param name="_scrollPosition">  The scroll position of the extended box if exceeded <paramref name="_maxLines"/>. </param>
        /// <param name="_minLines">        Minimum number of lines displayed at any point. </param>
        /// <param name="_maxLines">        Minimum number of lines before the text field is wrapped in a scroll box. </param>
        public static void MultiLineTextField(SerializedProperty _property, ref Vector2 _scrollPosition, int _minLines = 1, int _maxLines = 10)
        {
            GUIStyle wrappedTextStyle = new(EditorStyles.textArea)
            {
                wordWrap = true
            };

            float lineHeight = wrappedTextStyle.fontSize + 4f;
            float minHeight = lineHeight * _minLines;
            float maxHeight = lineHeight * _maxLines;
            float calculatedHeight = wrappedTextStyle.CalcHeight(new GUIContent(_property.stringValue), EditorGUIUtility.currentViewWidth - 30);
            float clampedHeight = Mathf.Clamp(calculatedHeight, minHeight, maxHeight);

            EditorGUILayout.BeginVertical();

            EditorGUILayout.PrefixLabel(_property.displayName);

            EditorGUI.BeginChangeCheck();
            {
                if (calculatedHeight > maxHeight)
                {
                    _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(clampedHeight));
                    _property.stringValue = EditorGUILayout.TextArea(_property.stringValue, wrappedTextStyle, GUILayout.ExpandHeight(true));
                }
                else
                {
                    _property.stringValue = EditorGUILayout.TextArea(_property.stringValue, wrappedTextStyle, GUILayout.Height(clampedHeight));
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_property.serializedObject.targetObject, $"{_property.displayName} Updated.");
                _property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_property.serializedObject.targetObject);
            }

            if (calculatedHeight > maxHeight)
                EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Draw a limited character text field.
        /// </summary>
        /// <param name="_property">        The assigned property. </param>
        /// <param name="_maxCharacters">   The maximum number of characters in the text field. </param>
        public static void LimitedTextField(SerializedProperty _property, int _maxCharacters)
        {
            GUIStyle leftAlignedTextStyle = new(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleRight
            };

            EditorGUILayout.BeginVertical();

            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.PropertyField(_property);
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_property.serializedObject.targetObject, $"{_property.displayName} Updated.");
                _property.serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(_property.serializedObject.targetObject);
            }

            EditorGUILayout.LabelField($"({_property.stringValue.Length}/{_maxCharacters})", leftAlignedTextStyle);

            EditorGUILayout.EndVertical();

            if (_property.stringValue.Length > _maxCharacters)
            {
                _property.stringValue = _property.stringValue[.._maxCharacters];

            }
            if (_property.stringValue.Length == _maxCharacters)
            {
                EditorGUILayout.HelpBox(
                    $"{_property.displayName} can not exceed {_maxCharacters} letters.",
                    MessageType.Warning);
            }
        } 
    }
}

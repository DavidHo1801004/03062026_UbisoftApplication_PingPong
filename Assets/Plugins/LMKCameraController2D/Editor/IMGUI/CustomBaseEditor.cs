using System.Collections.Generic;

using UnityEditor;
using UnityEngine;

namespace LMK.Editor
{
    [CanEditMultipleObjects]
    public class CustomBaseEditor : UnityEditor.Editor
    {
        // Properties excluded from default drawer.
        private readonly List<string> excludedProperties = new () { "m_Script" };



        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, excludedProperties.ToArray());

            GUILayout.Space(10);

            if (GUILayout.Button(
                $"This inspector is controlled by a custom editor.\n" +
                $"Click this to locate the custom editor file used for this script.", 
                EditorStyles.helpBox))
            {
                HighlightAsset(GetScriptFilePath());
            }

            serializedObject.ApplyModifiedProperties();
        }



        /// <summary>
        /// Use this instead of <see cref="SerializedObject.FindProperty(string)"/> <br/>
        /// This will automatically remove the property from the default drawer 
        /// to allow custom editor display.
        /// </summary>
        /// <param name="_propertyName"> Name of the property to find. </param>
        /// <returns>
        /// A serialized property with the given name if found. Otherwise, null.
        /// </returns>
        protected SerializedProperty AssignToProperty(string _propertyName)
        {
            excludedProperties.Add(_propertyName);

            return serializedObject.FindProperty(_propertyName);
        }



        private void HighlightAsset(string assetPath)
        {
            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset == null) return;

            EditorGUIUtility.PingObject(asset);
        }

        private string GetScriptFilePath()
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            if (script == null)
            {
                Debug.LogError("Could not find script file.");
                return null;
            }

            string path = AssetDatabase.GetAssetPath(script);
            return path;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using LMK.Editor;

namespace LMK.Attribute.Editor
{
    [CustomPropertyDrawer(typeof(SelectionGroupMemberAttribute))]
    public class SelectionGroupMemberAttributeDrawer : PropertyDrawer
    {
        // Key cache will store all selected enum selection key attribute field path references.
        // Outer dictionary store all unique objects using targetObject.InstanceID + property path.
        // Inner dictionary store all selection group keys using GroupKey. 
        //
        private static readonly Dictionary<string, Dictionary<string, string>> KeyCache = new();

        private SelectionGroupMemberAttribute Attr => (SelectionGroupMemberAttribute)attribute;



        #region IMGUI
        public override void OnGUI(Rect _Position, SerializedProperty _Property, GUIContent _Label)
        {
            RegisterSelectionCallback();

            if (ShouldShow(_Property))
                EditorGUI.PropertyField(_Position, _Property, _Label, true);
        }

        public override float GetPropertyHeight(SerializedProperty _Property, GUIContent _Label)
        {
            return ShouldShow(_Property)
                ? EditorGUI.GetPropertyHeight(_Property, _Label, true)
                : 0f;
        }
        #endregion

        #region UI Toolkit
        public override VisualElement CreatePropertyGUI(SerializedProperty _Property)
        {
            RegisterSelectionCallback();

            PropertyField field = new(_Property);

            void UpdateVisibility()
            {
                if (_Property.serializedObject.targetObject == null)
                    return;

                _Property.serializedObject.Update();

                field.style.display = ShouldShow(_Property)
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
            }

            field.RegisterCallbackOnce<AttachToPanelEvent>(_ =>
            {
                UpdateVisibility();

                // Track the KEY property instead of this value property
                var keyProp = GetKeyProperty(_Property);
                if (keyProp != null)
                {
                    field.TrackPropertyValue(keyProp, _ => UpdateVisibility()); 
                }
            });

            return field;
        }
        #endregion



        #region Core Logic
        /// <summary>
        /// Define if the property should be displayed or not.
        /// </summary>
        private bool ShouldShow(SerializedProperty _Property)
        {
            var keyProp = GetKeyProperty(_Property);
            if (keyProp == null) return true; 

            if (keyProp.propertyType == SerializedPropertyType.Enum &&
                Attr.MatchValue is Enum asEnumValue)
            {
                int attributeEnumIndex = Array.IndexOf(keyProp.enumNames, asEnumValue.ToString());
                if (attributeEnumIndex < 0) return false; // Enum value does not match enum type of property.
                return attributeEnumIndex == keyProp.enumValueIndex;
            }
            else 
            if (keyProp.propertyType == SerializedPropertyType.Boolean &&
                Attr.MatchValue is bool asBoolean)
            {
                return asBoolean == keyProp.boolValue;
            }

            return true; // fail-safe: show if something went wrong
        }

        /// <summary>
        /// Get the property which contains the corresponding key of the selection member.
        /// </summary>
        private SerializedProperty GetKeyProperty(SerializedProperty _Property)
        {
            CacheKeys(_Property);

            GetDeclareType(_Property, out string parentPath);
            string uniqueObjectPath = $"[{_Property.serializedObject.targetObject.GetInstanceID()}]{parentPath}";

            if (KeyCache.TryGetValue(uniqueObjectPath, out var dict))
                if (dict.TryGetValue(Attr.GroupKey, out var fieldName))
                    return _Property.serializedObject.FindProperty(fieldName);

            return null;
        }

        /// <summary>
        /// Construct lookup map for all selection groups in the C# declaring type which contains the serialized property.
        /// See <see cref="GetDeclareType(SerializedProperty, out string)"/> for how the target C# class is acquired.
        /// </summary>
        private void CacheKeys(SerializedProperty _Property)
        {
            Type declareType = GetDeclareType(_Property, out string parentPath);
            string uniqueObjectPath = $"[{_Property.serializedObject.targetObject.GetInstanceID()}]{parentPath}";

            if (KeyCache.ContainsKey(uniqueObjectPath)) return;

            var dict = new Dictionary<string, string>();
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var field in declareType.GetFields(flags))
            {
                var keyAttr = field.GetCustomAttribute<SelectionGroupKeyAttribute>();
                if (keyAttr == null) continue;

                // Duplicate group key handling
                if (dict.ContainsKey(keyAttr.GroupKey))
                {
                    Debug.LogError(
                        $"[EnumSelectionKey] Duplicate group key: '{keyAttr.GroupKey}' in ({declareType.Name}). Only the first will be used.",
                        _Property.serializedObject.targetObject);
                    continue;
                }

                dict.Add(keyAttr.GroupKey, (parentPath == null) ? field.Name : $"{parentPath}.{field.Name}");
            }

            KeyCache[uniqueObjectPath] = dict;
        }

        /// <summary>
        /// Get the actual C# class containing the definition of the property, not the UnityEngine.Object that initializes it.
        /// </summary>
        private Type GetDeclareType(SerializedProperty _Property, out string _ParentPath)
        {
            _ParentPath = null;

            // If property path is "X.Y.Z", parent path is "X.Y"
            int lastParentPathId = _Property.propertyPath.LastIndexOf('.');
            if (lastParentPathId == -1) return fieldInfo.DeclaringType;
            
            _ParentPath = _Property.propertyPath[..lastParentPathId];
            return _Property.serializedObject.FindProperty(_ParentPath).GetFieldType();
        }
        #endregion

        #region Callbacks
        private void RegisterSelectionCallback()
        {
            // Register once
            Selection.selectionChanged -= CleanKeyCache;
            Selection.selectionChanged += CleanKeyCache;
        }

        private void CleanKeyCache()
        {
            KeyCache.Clear();
        }
        #endregion
    }
}

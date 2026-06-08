using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using LMK.UIElements;
using Developer.Utilities;
using LMK.Editor.Utilities;

namespace LMK.Attribute.Editor
{
    [CustomPropertyDrawer(typeof(InlineReferenceAttribute), true)]
    public class InlineReferenceAttributeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty _Property)
        {
            // Handle invalid property type.
            if (_Property.propertyType != SerializedPropertyType.ManagedReference)
            {
                return new Label("Error: Inline attribute only works on SerializeReference fields.");
            }

            _Property.serializedObject.Update();
            SerializationUtility.ClearAllManagedReferencesWithMissingTypes(_Property.serializedObject.targetObject);

            // Draw default inline reference selector.
            return DrawInlineReferenceSelector(_Property);
        }



        #region Drawer Utilities
        private VisualElement DrawInlineReferenceSelector(SerializedProperty _Property)
        {
            var errorIcon = Resources.Load<Texture2D>("Sprites/error-triangle-reversed-64");
            var root = Resources.Load<VisualTreeAsset>("InlineReferenceDrawerUXML").Instantiate();

            // Type handling
            Type currentType = GetManagedReferenceType(_Property);
            var possibleTypes = GetAllDerivedTypes();

            // Get current type
            int currentIndex = -1;
            if (currentType != null)
            {
                var idx = possibleTypes.IndexOf(currentType);
                if (idx >= 0)
                    currentIndex = idx;
            }

            // Label
            var label = root.Q<Label>("content-label");
            label.text = _Property.displayName;

            // Initialize dropdown element
            var typeSelectField = root.Q<DropdownField>("type-select");
            typeSelectField.SetValueWithoutNotify(
                currentIndex == -1 
                ? "None" 
                : TypeToDisplayName(possibleTypes[currentIndex]));

            UpdateTypeSelectDisplay(typeSelectField, currentIndex);

            // Initialize dropdown content
            var typeSelectOptions = possibleTypes
                .Select(type => new GUIContent()
                {
                    text = TypeToDisplayName(type),
                    image = IsValidSubClass(type) ? null : errorIcon,
                    tooltip = IsValidSubClass(type) ? null : "Invalid class, selecting this would results in an empty reference."
                }).ToList();
            typeSelectOptions.Insert(0, new GUIContent("None"));

            var dropdownContent = new SearchableDropdown(typeSelectOptions.ToArray());
            dropdownContent.OnSelectedItemChanged += (index) =>
            {
                string itemName = dropdownContent.GetMenuItemNameAt(index);
                typeSelectField.SetValueWithoutNotify(itemName);

                if (index == 0)
                {
                    _Property.managedReferenceValue = null;
                }
                else
                {
                    Type selectedType = possibleTypes[index - 1];

                    _Property.managedReferenceValue = Activator.CreateInstance(selectedType);
                }

                UpdateTypeSelectDisplay(typeSelectField, index);
                _Property.serializedObject.ApplyModifiedProperties();
            };

            typeSelectField.RegisterCallback<PointerDownEvent>((e) =>
            {
                if (e.button != 0) return;

                UnityEditor.PopupWindow.Show(typeSelectField.worldBound, dropdownContent);
            });

            // Property field
            var propertyField = root.Q<PropertyField>("property-field");
            propertyField.BindProperty(_Property);
            propertyField.Bind(_Property.serializedObject);

            // Content switcher
            var switcher = root.Q<Switcher>("content-switcher");
            switcher.OnInitialized += () =>
            {
                if (_Property.managedReferenceValue == null)
                    switcher.SwitchActive(0);
                else
                    switcher.SwitchActive(1);
            };

            return root;
        }



        /// <summary>
        /// Valid subclass must not derived from UnityEngine.Object.
        // Add exceptions here for documentation purposes
        /// </summary>
        private bool IsValidSubClass(Type _Type)
        {
            if (_Type.IsSubclassOf(typeof(UnityEngine.Object))) return false;

            return true;
        }

        /// <summary>
        /// Get display name from type.
        /// </summary>
        private string TypeToDisplayName(Type _Type)
        {
            return RegExUtils.ParseCamelCase(_Type.Name);
        }

        /// <summary>
        /// Get the current managed reference type of the property using reflection.
        /// </summary>
        private Type GetManagedReferenceType(SerializedProperty _Property)
        {
            string typename = _Property.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(typename))
                return null;

            // typename = "AssemblyName TypeName"
            var parts = typename.Split(' ');
            return Type.GetType($"{parts[1]}, {parts[0]}");
        }

        /// <summary>
        /// Retrieve all derived types of the property base reflection type in all loaded assemblies.
        /// </summary>
        private List<Type> GetAllDerivedTypes()
        {
            Type baseType = GetElementType(fieldInfo.FieldType);
            return TypeCache.GetTypesDerivedFrom(baseType).ToList();
        }

        /// <summary>
        /// Handle collections to retrieve element type, or field type for single field.
        /// </summary>
        /// <param name="_FieldType"> Type of the given field. </param>
        private Type GetElementType(Type _FieldType)
        {
            // Handle native arrays
            if (_FieldType.IsArray)
                return _FieldType.GetElementType();

            // Handle generic collections
            if (_FieldType.IsGenericType)
            {
                Type genericDef = _FieldType.GetGenericTypeDefinition();
                if (genericDef == typeof(List<>))
                    return _FieldType.GetGenericArguments()[0];
            }

            return fieldInfo.FieldType;
        }

        /// <summary>
        /// Update type select field display.
        /// </summary>
        private void UpdateTypeSelectDisplay(VisualElement _Element, int _SelectedIndex)
        {
            if (_SelectedIndex < 0)
                _Element.AddToClassList("invalid");
            else
                _Element.RemoveFromClassList("invalid");
        }
        #endregion
    }
}

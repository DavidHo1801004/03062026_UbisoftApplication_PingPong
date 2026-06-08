using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace Developer.MotorControl.Editor
{
    [CustomPropertyDrawer(typeof(MotorControlModule))]
    public class MotorControlModuleDrawer : PropertyDrawer
    {
        private const string USS_CLASS_NAME = "lmk-motor-control-module";



        public override VisualElement CreatePropertyGUI(SerializedProperty _Property)
        {
            var target = _Property.managedReferenceValue as MotorControlModule;
            target?.SyncInputActionKeys();

            var defaultStyleSheet = Resources.Load<StyleSheet>("UI Toolkit/MotorControlModuleUSS");
            var root = new Foldout();
            root.text = _Property.displayName;
            root.AddToClassList(USS_CLASS_NAME);
            if (defaultStyleSheet != null)
                root.styleSheets.Add(defaultStyleSheet);

            var current = _Property.Copy();
            var end = current.GetEndProperty();
            if (current.NextVisible(true))
                do
                {
                    PropertyField propertyField = new(current, current.displayName);
                    root.Add(propertyField);
                }
                while (current.NextVisible(false) && !SerializedProperty.EqualContents(current, end));

            Foldout inputFoldout = new();
            inputFoldout.text = "Input Actions";
            root.Add(inputFoldout);

            var passiveControlProperty = _Property.FindPropertyRelative("passiveControl");
            PropertyField passiveControl = new(passiveControlProperty, "Passive Control");
            inputFoldout.Add(passiveControl);

            var inputActionsProperty = _Property.FindPropertyRelative("inputActions");
            VisualElement inputList = null;
            if (inputActionsProperty != null)
            {
                inputList = DrawInputActionRefDictionary(inputActionsProperty);
                inputList.style.display = passiveControlProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
                inputFoldout.Add(inputList);
            }

            passiveControl.RegisterValueChangeCallback((e) =>
            {
                if (inputList != null)
                    inputList.style.display = e.changedProperty.boolValue ? DisplayStyle.None : DisplayStyle.Flex;
            });

            return root;
        }



        private VisualElement DrawInputActionRefDictionary(SerializedProperty _InputActionList)
        {
            VisualElement list = new();

            var keysProperty = _InputActionList.FindPropertyRelative("keys");
            var valuesProperty = _InputActionList.FindPropertyRelative("values");

            for (int i = 0; i < keysProperty.arraySize; i++)
            {
                PropertyField propertyField = new(
                    valuesProperty.GetArrayElementAtIndex(i), 
                    keysProperty.GetArrayElementAtIndex(i).stringValue);

                list.Add(propertyField);
            }

            return list;
        }
    }
}

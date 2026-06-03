using System;
using System.Reflection;
using System.Collections;

using UnityEditor;

namespace LMK.Editor
{
    public static class SerializedPropertyExtension
    {
        /// <summary>
        /// Get the underlying type of a <see cref="SerializedProperty"/>.
        /// </summary>
        public static Type GetFieldType(this SerializedProperty _Property)
        {
            Type parentType = _Property.serializedObject.targetObject.GetType();
            string[] path = _Property.propertyPath.Replace(".Array.data[", "[").Split('.');

            Type currentType = parentType;

            foreach (var element in path)
            {
                if (element.Contains("["))
                {
                    // Array/List element handling
                    string fieldName = element.Substring(0, element.IndexOf("["));
                    currentType = GetField(currentType, fieldName);

                    if (currentType.IsArray)
                    {
                        currentType = currentType.GetElementType();
                    }
                    else if (typeof(IList).IsAssignableFrom(currentType))
                    {
                        currentType = currentType.GetGenericArguments()[0];
                    }
                }
                else
                {
                    currentType = GetField(currentType, element);
                }
            }

            return currentType;
        }



        private static Type GetField(Type _Type, string _FieldName)
        {
            while (_Type != null)
            {
                FieldInfo field = _Type.GetField(_FieldName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field != null)
                    return field.FieldType;

                _Type = _Type.BaseType;
            }

            return null;
        }
    }
}
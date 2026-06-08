using System;
using System.Collections.Generic;
using System.Linq;

namespace Developer.GameplaySystems.Attributes
{
    /// <summary>
    /// Internal class contains a list of <see cref="UnitAttribute">attributes</see>.
    /// </summary>
    internal sealed class UnitAttributeSet
    {
        private readonly Dictionary<Type, UnitAttribute> attributes = new();

        private readonly UnitGameplayData ownerCombatData;



        public UnitAttributeSet(UnitGameplayData _Owner)
        {
            ownerCombatData = _Owner;
        }

        public override string ToString()
        {
            string result = "Attributes:\n";

            foreach (var attr in attributes.Values)
            {
                result += attr.ToString() + "\n";
            }

            return result;
        }



        /// <summary>
        /// Deep clone.
        /// </summary>
        public UnitAttributeSet Clone(UnitGameplayData _NewOwner)
        {
            UnitAttributeSet clone = new(_NewOwner);
            foreach (var attribute in attributes)
            {
                var cloneAttribute = attribute.Value.Clone();
                cloneAttribute.OwnerData = clone.ownerCombatData;
                clone.attributes.Add(attribute.Key, cloneAttribute);
            }

            return clone;
        }

        /// <summary>
        /// Check if this attribute set contains the given attribute type.
        /// </summary>
        public bool ContainsAttribute<T>() where T : UnitAttribute
        {
            return attributes.ContainsKey(typeof(T));
        }

        /// <inheritdoc cref="ContainsAttribute{T}()"/>
        public bool ContainsAttribute(Type _AttributeType)
        {
            return attributes.ContainsKey(_AttributeType);
        }


        /// <summary>
        /// Add a new attribute to the set if not present. <br/>
        /// If the set already contains the attribute, this method does nothing.
        /// </summary>
        /// <param name="_Template">    Attribute template of the attribute to add. <br/>
        ///                             This means the attribute will not be added directly, but cloned before adding to set. </param>
        public UnitAttribute AddAttribute(UnitAttribute _Template)
        {
            Type type = _Template.GetType();
            if (attributes.ContainsKey(type)) return null;

            var clone = _Template.Clone();
            clone.OwnerData = ownerCombatData;

            attributes.Add(type, clone);

            return clone;
        }

        /// <inheritdoc cref="RemoveAttribute(Type)"/>
        /// <typeparam name="T"> The target attribute type. </typeparam>
        public void RemoveAttribute<T>() where T : UnitAttribute
        {
            RemoveAttribute(typeof(T));
        }

        /// <summary>
        /// Remove the given attribute from the attribute set if exist.
        /// </summary>
        /// <param name="_AttributeType"> The target attribute type. </param>
        public void RemoveAttribute(Type _AttributeType)
        {
            if (!_AttributeType.IsSubclassOf(typeof(UnitAttribute))) return;
            if (!attributes.ContainsKey(_AttributeType)) return;

            attributes.Remove(_AttributeType);
        }



        /// <inheritdoc cref="GetAttribute(Type)"/>
        /// <typeparam name="T"> The target attribute type. </typeparam>
        public T GetAttribute<T>() where T : UnitAttribute
        {
            return (T)GetAttribute(typeof(T));
        }

        /// <summary>
        /// Get the given attribute from the attribute set if exist.
        /// </summary>
        /// <param name="_AttributeType"> The target attribute type. </param>
        /// <returns>
        /// <see langword="null"/> if attribute does not exist; <br/>
        /// otherwise, returns the attribute.
        /// </returns>
        public UnitAttribute GetAttribute(Type _AttributeType)
        {
            if (!_AttributeType.IsSubclassOf(typeof(UnitAttribute))) return null;
            if (!attributes.ContainsKey(_AttributeType)) return null;

            return attributes[_AttributeType];
        }

        /// <summary>
        /// Get all attributes with the given categories.
        /// </summary>
        /// <param name="_Policy">      How should the category matching be handled. </param>
        /// <param name="_Categories">  Categories to search for. </param>
        /// <returns>
        /// An array of all matching attributes.
        /// </returns>
        public UnitAttribute[] GetAttributes(MatchPolicy _Policy = MatchPolicy.Any, params UnitAttribute.Category[] _Categories)
        {
            return attributes.Values.Where((e) => e.ContainsCategory(_Policy, _Categories)).ToArray();
        }
    }
}

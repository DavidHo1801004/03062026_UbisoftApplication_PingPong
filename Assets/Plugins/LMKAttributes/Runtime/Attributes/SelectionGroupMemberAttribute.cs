using UnityEngine;

namespace LMK.Attribute
{
    /// <summary>
    /// Define a field as an enum group member. <br/>
    /// This is supposed to be used with <see cref="SelectionGroupKeyAttribute"/> to create togglable enum selection groups.
    /// </summary>
    public class SelectionGroupMemberAttribute : PropertyAttribute
    {
        /// <summary>
        /// Define which selection group this member belongs to. <br/>
        /// A matching key must be defined by a <see cref="SelectionGroupKeyAttribute"/> of a valid enum field in the same class.
        /// </summary>
        /// <remarks>
        /// <b>NOTE:</b> If this does not match any existing key in the same class, this attribute will do nothing.
        /// </remarks>
        public readonly string GroupKey;

        /// <summary>
        /// Name of the enum value.
        /// </summary>
        public readonly object MatchValue;   

        public SelectionGroupMemberAttribute(string _GroupKey, object _MatchValue) 
            : base(applyToCollection: true)
        {
            GroupKey = _GroupKey;
            MatchValue = _MatchValue;
        }
    }
}

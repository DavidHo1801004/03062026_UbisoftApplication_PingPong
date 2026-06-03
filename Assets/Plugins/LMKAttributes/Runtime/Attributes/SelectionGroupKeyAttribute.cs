using UnityEngine;

namespace LMK.Attribute
{
    /// <summary>
    /// Define an enum field as a new group key. <br/>
    /// This is supposed to be used with <see cref="SelectionGroupMemberAttribute"/> to create togglable enum selection groups.
    /// </summary>
    public class SelectionGroupKeyAttribute : PropertyAttribute
    {
        /// <summary>
        /// Unique key shared between all <see cref="SelectionGroupKeyAttribute"/> within the container class.
        /// </summary>
        public readonly string GroupKey;

        public SelectionGroupKeyAttribute(string _GroupKey)
        {
            GroupKey = _GroupKey;
        }
    }
}

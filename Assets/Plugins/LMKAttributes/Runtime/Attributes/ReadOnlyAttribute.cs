using UnityEngine;

namespace LMK.Attribute
{
    /// <summary>
    /// Disable a property field controls in inspector. <br/>
    /// Support conditional controls.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute
    {
        internal readonly string targetFieldName = null;

        public ReadOnlyAttribute() { }

        /// <summary>
        /// Create a new ReadOnly attribute that only disable when the target bool field is true. <br/>
        /// If the target field does not exist, the attribute is active by default.
        /// </summary>
        /// <param name="_Field"> Name of the target bool conditional field. (use nameof()) </param>
        public ReadOnlyAttribute(string _Field)
        {
            targetFieldName = _Field;
        }
    }
}

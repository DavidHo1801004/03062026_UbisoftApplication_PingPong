using UnityEngine;

namespace GASU.Attribute
{
    /// <summary>
    /// Creates and serializes a per-object instance for this reference field.
    /// </summary>
    /// <remarks>
    /// <b>NOTE:</b> The target field must have <see cref="SerializeReference"/> as its attribute.
    /// </remarks>
    public class InlineReferenceAttribute : PropertyAttribute { }
}

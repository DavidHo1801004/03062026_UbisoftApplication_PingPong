using Developer.GameplaySystems.Attributes;

namespace Developer.GameplaySystems.General
{
    /// <summary>
    /// Contains data of an modification.
    /// </summary>
    public readonly struct AttributeModSpec
    {
        public readonly float magnitude;
        public readonly AttributeModOp operation;
        public readonly ModAppPolicy policy;

        public AttributeModSpec(
            float _Magnitude,
            AttributeModOp _Operation,
            ModAppPolicy _Policy)
        {
            magnitude = _Magnitude;
            operation = _Operation;
            policy = _Policy;
        }
    }

    /// <summary>
    /// Contains references to the active modifier.
    /// </summary>
    public class ActiveModHandle
    {
        public readonly int id;
        public readonly AttributeModOp operation;
        public readonly UnitAttribute attributeRef;

        public ActiveModHandle(
            int _ModifierId,
            AttributeModOp _Operation,
            UnitAttribute _AttributeRef)
        {
            id = _ModifierId;
            operation = _Operation;
            attributeRef = _AttributeRef;
        }
    }

    /// <summary>
    /// Methods for apply modifications.
    /// </summary>
    public enum AttributeModOp
    {
        // Add a flat value before multiplication.
        Addition,

        // Multiply added values.
        Multiplication,

        // Divide final value.
        Division,

        // Directly set final value.
        // First applied override will be used when multiple overrides are present.
        Override,
    }

    /// <summary>
    /// How modifications are treated when applied.
    /// </summary>
    public enum ModAppPolicy
    {
        // Applied directly to the base value of the attribute.
        Instant,

        // Stored as temp modifications till removed.
        Persistent,

        // TODO - C1: durational modifications
    }
}

using Developer.GameplaySystems.Attributes;
using UnityEngine;

namespace Developer.PingPong.GameplayAttributes
{
    public class AccelerationAttribute : UnitAttribute
    {
        protected override Category[] Categories => new Category[] { };

        protected override float PreModifyValue(float _NewValue)
        {
            return Mathf.Max(0, _NewValue);
        }

        public AccelerationAttribute(float _BaseValue) : base(_BaseValue) { }
    }
}

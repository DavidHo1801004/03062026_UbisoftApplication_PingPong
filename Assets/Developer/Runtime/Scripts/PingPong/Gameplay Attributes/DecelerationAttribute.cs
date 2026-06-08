using Developer.GameplaySystems.Attributes;
using UnityEngine;

namespace Developer.PingPong.GameplayAttributes
{
    public class DecelerationAttribute : UnitAttribute
    {
        protected override Category[] Categories => new Category[] { };

        protected override float PreModifyValue(float _NewValue)
        {
            return Mathf.Max(0, _NewValue);
        }

        public DecelerationAttribute(float _BaseValue) : base(_BaseValue) { }
    }
}

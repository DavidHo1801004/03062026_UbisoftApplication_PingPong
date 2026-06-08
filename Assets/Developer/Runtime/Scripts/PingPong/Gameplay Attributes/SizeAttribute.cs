using Developer.GameplaySystems.Attributes;
using UnityEngine;

namespace Developer.PingPong.GameplayAttributes
{
    public class SizeAttribute : UnitAttribute
    {
        protected override Category[] Categories => new Category[] { };

        protected override float PreModifyValue(float _NewValue)
        {
            var maxValue = OwnerData.GetAttribute<MaxSizeAttribute>();
            if (maxValue != null)
            {
                _NewValue = Mathf.Min(_NewValue, maxValue.ModifiedValue);
            }

            return _NewValue;
        }

        public SizeAttribute(float _BaseValue) : base(_BaseValue) { }
    }
}

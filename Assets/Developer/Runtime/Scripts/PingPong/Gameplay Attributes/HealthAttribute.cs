using Developer.GameplaySystems.Attributes;
using UnityEngine;

namespace Developer.PingPong.GameplayAttributes
{
    public class HealthAttribute : UnitAttribute
    {
        protected override Category[] Categories => new Category[] { };

        protected override float PreModifyValue(float _NewValue)
        {
            var roundedValue = Mathf.Round(_NewValue);
            var maxValue = OwnerData.GetAttribute<MaxHealthAttribute>();
            if (maxValue != null)
            {
                roundedValue = Mathf.Min(roundedValue, maxValue.ModifiedValue);
            }

            return roundedValue;
        }

        public HealthAttribute(int _BaseValue) : base(_BaseValue) { }
    }
}

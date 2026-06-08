using Developer.GameplaySystems.Attributes;
using UnityEngine;

namespace Developer.PingPong.GameplayAttributes
{
    public class MaxHealthAttribute : UnitAttribute
    {
        protected override Category[] Categories => new Category[] { };

        protected override float PreModifyValue(float _NewValue)
        {
            return Mathf.Round(_NewValue);
        }

        public MaxHealthAttribute(int _BaseValue) : base(_BaseValue) { }
    }
}

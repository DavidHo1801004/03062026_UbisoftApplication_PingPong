using Developer.GameplaySystems;
using Developer.PingPong.GameplayAttributes;
using UnityEngine;
using Developer.GameplaySystems.General;

namespace Developer.PingPong.Pickupable
{
    [System.Serializable]
    public class AddGoalSizeEvent : OverlapEvent
    {
        [SerializeField]
        private float magnitude = 1;

        protected internal override void OnOverlapped(Collider2D _Other)
        {
            if (_Other.TryGetComponent(out GameplayDataComponent asGDC)) {
                UnitTag teamTag = asGDC.GameplayData.ContainTags(UnitTag.TeamA)
                    ? UnitTag.TeamA
                    : UnitTag.TeamB;

                var targets = PingPongField.Instance.GetExtensions<GoalExtension>();
                foreach (var target in targets)
                {
                    if (target.GameplayData.ContainTags(teamTag))
                    {
                        target.GameplayData.ModifyAttributeValue<SizeAttribute>(new AttributeModSpec(
                            _Magnitude: magnitude,
                            _Operation: AttributeModOp.Addition,
                            _Policy: ModAppPolicy.Instant));
                    }
                }
            }
        }
    }
}
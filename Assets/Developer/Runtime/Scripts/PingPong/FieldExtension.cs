using Developer.GameplaySystems;
using UnityEngine;

namespace Developer.PingPong
{
    public class FieldExtension : MonoBehaviour
    {
        [Header("GAMEPLAY DATA")]
        [SerializeField]
        private UnitTag[] initialTags;
        [SerializeField]
        protected GameplayDataComponent gameplayDataComp;

        public UnitGameplayData GameplayData => gameplayDataComp.GameplayData;

        public PingPongField AttachedField { get; private set; }

        protected virtual void Start()
        {
            foreach (var tag in initialTags)
                GameplayData.AddTag(tag);
        }

        internal bool Attach(PingPongField _Field)
        {
            if (AttachedField == _Field) return false;

            AttachedField = _Field;
            if (_Field == null)
                OnDetached();
            else
                OnAttached(_Field);

            return true;
        }

        protected virtual void OnAttached(PingPongField _Field) { }

        protected virtual void OnDetached() { }

        protected internal virtual void OnFieldUpdated(PingPongField _Field) { }
    }
}

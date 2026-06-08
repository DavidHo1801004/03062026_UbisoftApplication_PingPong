using UnityEngine;

namespace Developer.GameplaySystems.Equipment
{
    [System.Serializable]
    public abstract class ItemBase
    {
        protected internal virtual void OnSelect(GameObject _Owner) { }

        protected internal virtual void OnDeselect(GameObject _Owner) { }

        public virtual void OnUse(GameObject _Owner, GameObject _Target) { }
    }
}

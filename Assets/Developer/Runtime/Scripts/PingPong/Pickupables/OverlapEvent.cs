using UnityEngine;

namespace Developer.PingPong.Pickupable
{
    [System.Serializable]
    public abstract class OverlapEvent
    {
        /// <summary>
        /// On overlapped with a valid object.
        /// </summary>
        protected internal abstract void OnOverlapped(Collider2D _Other);
    }
}

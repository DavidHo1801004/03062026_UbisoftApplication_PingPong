using LMK.Attribute;
using System;
using System.Collections;
using UnityEngine;

namespace Developer.PingPong.Pickupable
{
    [DisallowMultipleComponent]
    public class PickupableObject : MonoBehaviour
    {
        [SerializeField]
        private float lifeSpan = 4f;

        [Space]
        [SerializeField]
        private Collider2D mainCollider;
        [SerializeField]
        private LayerMask targetLayers;

        [SerializeReference, InlineReference]
        private OverlapEvent[] events;

        public event Action OnDestroyed;


#if UNITY_EDITOR
        private void Reset()
        {
            if (!mainCollider)
                mainCollider = GetComponent<Collider2D>();
        }
#endif

        private void OnEnable()
        {
            GameManager.EventAggregator.Subscribe<EndRoundEvent>(OnEndRoundEvent);
        }

        private void OnDisable()
        {
            GameManager.EventAggregator.Unsubscribe<EndRoundEvent>(OnEndRoundEvent);
        }

        private void Start()
        {
            if (!mainCollider)
                mainCollider = GetComponent<Collider2D>();

            mainCollider.isTrigger = true;
            mainCollider.includeLayers = targetLayers;
            mainCollider.excludeLayers = ~targetLayers;

            StartCoroutine(AutoDestroyCoroutine());
        }

        private void OnTriggerEnter2D(Collider2D _Collider)
        {
            foreach (var e in events)
                e.OnOverlapped(_Collider);

            Destroy(gameObject);
            OnDestroyed?.Invoke();
        }



        private IEnumerator AutoDestroyCoroutine()
        {
            yield return new WaitForSeconds(lifeSpan);
            Destroy(gameObject);
            OnDestroyed?.Invoke();
        }

        #region Callback
        private void OnEndRoundEvent(EndRoundEvent _Event)
        {
            Destroy(gameObject);
        }
        #endregion
    }
}

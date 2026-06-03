using UnityEngine;

namespace LMK.CameraController2D
{
    /// <summary>
    /// Define bounding triggers to update camera bounds.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CameraBoundsTrigger : MonoBehaviour
    {
        [SerializeField]
        private Collider2D boundsTrigger;

        [SerializeField]
        [Tooltip("Layer mask of target object(s).")]
        private LayerMask triggerLayers;

        [Space]
        [SerializeField]
        [Tooltip("Bounds will automatically match the target ")]
        internal CameraBoundsData boundsData;



#if UNITY_EDITOR
        private void Reset()
        {
            if (!boundsTrigger) boundsTrigger = GetComponent<Collider2D>();
        }
#endif

        private void Awake()
        {
            if (!boundsTrigger) boundsTrigger = GetComponent<Collider2D>();

            boundsTrigger.isTrigger = true;
            boundsTrigger.includeLayers = triggerLayers;
            boundsTrigger.excludeLayers = ~triggerLayers;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            CameraController2D.AddBoundsToStack(boundsData);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            CameraController2D.RemoveBoundsFromStack(boundsData);
        }
    } 
}

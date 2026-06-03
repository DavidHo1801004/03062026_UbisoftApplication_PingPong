using UnityEngine;

using LMK.Attribute;

namespace LMK.CameraController2D
{
    /// <summary>
    /// Contains basic data regarding camera bounds.
    /// </summary>
    [System.Serializable]
    public struct CameraBoundsData
    {
        [Tooltip("Root transform of the trigger bounds which define local space. " +
            "In most cases, this is the transform which the bounds is attached to. " +
            "This can be set to any transform to allow trigger scripts to exist on different objects.")]
        public Transform rootTransform;

        [Tooltip("If true, bounds is calculated from local space of the attached transform. \n" +
            "If false, bounds are calculated in worlds space instead.")]
        public bool isLocal;

        [Tooltip("This is calculated automatically by owned camera bounds trigger.")]
        public Rect bounds;


        [Space]
        [SelectionGroupKey("Zoom")]
        [Tooltip("If true, apply a target zoom size to the target camera controller once triggered.")]
        public bool forceZoom;

        [SelectionGroupMember("Zoom", true)]
        [Tooltip("The minimum orthographic size of the target target camera. Set value to negative to disable this value.")]
        public float minSize;

        [SelectionGroupMember("Zoom", true)]
        [Tooltip("The maximum orthographic size of the target target camera. Set value to negative to disable this value.")]
        public float maxSize;



        /// <summary>
        /// Bounds calculated in world space.
        /// </summary>
        public Rect WorldBounds
        {
            get
            {
                var worldBounds = bounds;
                if (isLocal)
                    worldBounds.center += (Vector2)rootTransform.position;

                return worldBounds;
            }

            set
            {
                if (isLocal)
                    value.center -= (Vector2)rootTransform.position;

                bounds = value;
            }
        }
    }
}

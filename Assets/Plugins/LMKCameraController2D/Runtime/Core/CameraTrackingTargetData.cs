using UnityEngine;

using LMK.Attribute;

namespace LMK.CameraController2D
{
    /// <summary>
    /// Contains data of camera tracking target position and offset value.
    /// </summary>
    [System.Serializable]
    public struct CameraTrackingTargetData
    {
        /// <summary>
        /// Define what the target position of the tracking target will be based on. 
        /// </summary>
        public enum Tracking
        {
            /// <summary>
            /// Use a fixed vector as tracking target.
            /// </summary>
            Vector,

            /// <summary>
            /// Use a <see cref="UnityEngine.Transform"/> as tracking target.
            /// </summary>
            Transform
        }

        /// <summary>
        /// Define how to apply offset to the tracking target.
        /// </summary>
        public enum Offset
        {
            /// <summary>
            /// No offset.
            /// </summary>
            None,

            /// <summary>
            /// Fixed offset from center of tracking transform.
            /// </summary>
            Fixed,

            /// <summary>
            /// Offset in moving direction of tracking transform. 
            /// </summary>
            /// <remarks>
            /// If used alongside <see cref="Tracking.Vector"/>, this does nothing.
            /// </remarks>
            MovementPrediction
        }

        /// <summary>
        /// Define how the target visible area are calculated.
        /// </summary>
        public enum VisibleAreaMode
        {
            /// <summary>
            /// A single point at offset target location.
            /// </summary>
            Point,

            /// <summary>
            /// A centered box at the offset target location.
            /// </summary>
            Box,

            /// <summary>
            /// A local space rect with bottom-left corner at offset target location.
            /// </summary>
            Rect,
        }



        [Tooltip("How impactful is this target to the final tracking position calculation of assigned camera controllers")]
        [Min(1)]
        public float weight;


        [Space(10)]
        [Tooltip("Define what the target position of the tracking target will be based on")]
        [SelectionGroupKey("Tracking")]
        public Tracking trackingMode;

        [SelectionGroupMember("Tracking", Tracking.Vector)]
        public Vector2 trackingPosition;

        [SelectionGroupMember("Tracking", Tracking.Transform)]
        public Transform trackingTransform;


        [Space(10)]
        [Tooltip("Define how to apply offset to the tracking target")]
        [SelectionGroupKey("Offset")]
        public Offset offsetMode;

        [SelectionGroupMember("Offset", Offset.Fixed)]
        public Vector2 fixedOffset;

        [Tooltip("How impactful is the target's movement to the final target position")]
        [SelectionGroupMember("Offset", Offset.MovementPrediction)]
        public Vector2 movementPredictionWeight;

        [Tooltip("Maximum value of the offset vector in magnitude.\n" +
            "Set to 0 to disable limit.")]
        [SelectionGroupMember("Offset", Offset.MovementPrediction)]
        [Min(0)]
        public float maxOffsetMagnitude;


        [Space(10)]
        [Tooltip("Define how the target visible area are calculated")]
        [SelectionGroupKey("Visible Area")]
        public VisibleAreaMode visibleAreaMode;

        [InspectorName("Size")]
        [SelectionGroupMember("Visible Area", VisibleAreaMode.Box)]
        public Vector2 visibleBoxSize;

        [InspectorName("Rect")]
        [SelectionGroupMember("Visible Area", VisibleAreaMode.Rect)]
        public Rect visibleRect;



        /// <summary>
        /// The current position of the tracking target.
        /// </summary>
        /// <remarks>
        /// This is not accounted for offsets, which should be handled within camera controller.
        /// </remarks>
        public Vector3 TargetPosition
        {
            get
            {
                if (trackingMode == Tracking.Vector)
                {
                    return trackingPosition;
                }
                else
                if (trackingMode == Tracking.Transform)
                {
                    if (trackingTransform)
                        return trackingTransform.position;
                    else
                        return Vector3.zero;
                }

                return Vector3.zero;
            }
        }
    }
}

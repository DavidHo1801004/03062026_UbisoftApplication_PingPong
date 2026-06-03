using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using LMK.CameraController2D.Extensions;

namespace LMK.CameraController2D
{
    using TrackData = CameraTrackingTargetData;

    /// <summary>
    /// Controller script for handling movement tracking and bounds clipping of a 2D camera.
    /// </summary>
    [ExecuteInEditMode]
    public class CameraController2D : MonoBehaviour
    {
        public static CameraController2D Instance { get; private set; }



        [Tooltip("The controlled camera. \n" +
            "This does NOT have to be attached to the same game object.")]
        [SerializeField] private Camera mainCamera;

        /*
         * Tracking Controls
         * Allow mainCamera to attach to an object's transform.
         */
        [SerializeField] private float zOffset;
        [SerializeField] private List<TrackData> trackingTargets = new();

        [Space(10)]
        [Tooltip("If enable, controlled camera will snap to tracking transform.\n" +
            "Otherwise, allow camera to follow tracking transform with its own speed.")]
        [SerializeField] private bool enableSnapping = false;

        [Tooltip("Ease curve used while snapping is enabled.")]
        [SerializeField] private AnimationCurve snapEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Seconds until tracking transform is directly in the middle of camera view after changing tracking transform.")]
        [SerializeField] private float snapEaseDuration = 0.5f;

        [Tooltip("Minimum distance between the controlled camera and the tracking transform at which movement tracking will be enabled.")]
        [SerializeField] private float leashRadius;

        [Tooltip("Movement speed is calculated by multiplying this value with distance in units between camera and tracking transform.")]
        [Min(0)]
        [SerializeField] private float speedMultiplier = 0.5f;

        [Tooltip("Maximum tracking speed.")]
        [Min(float.Epsilon)]
        [SerializeField] private float maxSpeed = 1.0f;

        [Tooltip("Maximum tracking speed. This is used to prevent infinitely small movement speed.")]
        [Min(float.Epsilon)]
        [SerializeField] private float minSpeed = 0.01f;

        /*
         * Boundary Controls
         * Allow camera view to fit within a defined boundary.
         */
        [Tooltip("Should camera's view be forced to fit within a defined boundary?")]
        [SerializeField] private bool enableBounds = false;

        [Tooltip("Camera's view will be forced to stay within this area.")]
        [SerializeField] internal Rect outerBounds = new(0, 0, 20, 10);

        [HideInInspector]
        [SerializeField] private Rect validBounds;

        /*
         * Panning and Zoom Controls
         */
        [Tooltip("Should camera's orthographic size be shrink to fit within the defined boundary?")]
        [SerializeField] private bool enableHardLock = false;

        [Tooltip("Ease curve used while hard lock is enabled.")]
        [SerializeField] private AnimationCurve zoomEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Tooltip("Seconds until orthographic view of the controlled camera is snapped to the clipped zoom of a new bounds.")]
        [SerializeField] private float zoomEaseDuration = 0.2f;

        [SerializeField] private float minOrthographicSize = 1.0f;
        [SerializeField] private float maxOrthographicSize = 3.0f;

        public static Camera MainCamera { get { return Instance?.mainCamera ?? Camera.main; } }

        public static Vector2 MouseWorldPos => MainCamera.ScreenToWorldPoint(Input.mousePosition);

        public static bool EnableSnapping
        {
            get => Instance.enableSnapping;
            set
            {
                if (Instance.enableSnapping == value) return;

                Instance.enableSnapping = value;
                Instance.UpdateTrackingCoroutine();
            }
        }

        public bool IsBoundsEnabled => enableBounds;

        public Vector3 BoundedTargetPosition { get; private set; }

        public Vector3 UnboundedTargetPosition { get; private set; }

        internal TrackData[] TrackingTargets => trackingTargets.ToArray();



#if UNITY_EDITOR
        private void Reset()
        {
            mainCamera = GetComponent<Camera>();
            if (!mainCamera) mainCamera = Camera.main;

            Screen.autorotateToLandscapeRight = true;
        }

        private void OnValidate()
        {
            InitializeTrackingList();
            InitializeZoomProperties();

            RecalculateValidBounds();
            RecalculateZoomConstrains();

            UpdateTrackingPosition();
            UpdateTrackingCoroutine();
        }
#endif 

        private void Awake()
        {
            // Disable if no valid camera are assigned
            if (!mainCamera)
            {
                enabled = false;
                return;
            }

            #region Singleton
            if (!Instance)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
            #endregion
        }

        private void Start()
        {
            InitializeTrackingList();
            InitializeZoomProperties();

            RecalculateValidBounds();
            RecalculateZoomConstrains();
        }

        private void Update()
        {
            UpdateTrackingTransformsMovement();
        }



        #region Tracking
        // Helper struct contains data of tracking target after calculation and culling.
        private struct TrackingTargetFinalData
        {
            public float weight;

            public Vector3 offsetPosition;

            public Rect visibleRect;
        }

        private Coroutine moveToTransformCoroutine;

        // Position map used for storing the last updated position of a tracking target
        // with Offset.MovementPrediction and Tracking.Transform
        private readonly Dictionary<Transform, Vector2> lastPosTrackMap = new();
        private readonly Dictionary<Transform, Vector2> velocityTrackMap = new();

        private Rect targetVisibleArea;
        public Rect TargetVisibleArea => targetVisibleArea;



        /// <inheritdoc cref="AddTrackingTarget_Core(TrackData)"/>
        public static CameraTrackingTargetHandle AddTrackingTarget(TrackData _NewData)
        {
            return Instance.AddTrackingTarget_Core(_NewData);
        }

        /// <summary>
        /// Add a new tracking target to this camera.
        /// </summary>
        /// <param name="_NewData"> Tracking target data to apply. </param>
        public CameraTrackingTargetHandle AddTrackingTarget_Core(TrackData _NewData)
        {
            trackingTargets.Add(_NewData);

            RegisterTrackingTarget(_NewData);

            UpdateTrackingCoroutine();

            return new CameraTrackingTargetHandle(_NewData, _NewData.GetHashCode());
        }

        /// <inheritdoc cref="RemoveTrackingTarget_Core(CameraTrackingTargetHandle)"/>
        public static bool RemoveTrackingTarget(CameraTrackingTargetHandle _Handle)
        {
            return Instance.RemoveTrackingTarget_Core(_Handle);
        }

        /// <summary>
        /// Remove a registered tracking target using the given handle.
        /// </summary>
        /// <param name="_Handle">  </param>
        private bool RemoveTrackingTarget_Core(CameraTrackingTargetHandle _Handle)
        {
            if (!trackingTargets.Remove(_Handle.data)) return false;

            UnregisterTrackingTarget(_Handle.data);

            if (trackingTargets.Count == 0)
            {
                if (moveToTransformCoroutine != null)
                    StopCoroutine(moveToTransformCoroutine);

                return true;
            }

            UpdateTrackingCoroutine();

            return true;
        }



        #region Coroutines
        /// <summary>
        /// Update tracking coroutines.
        /// </summary>
        private void UpdateTrackingCoroutine()
        {
            if (!Application.isPlaying) return;

            if (enableSnapping)
            {
                if (moveToTransformCoroutine != null) StopCoroutine(moveToTransformCoroutine);

                moveToTransformCoroutine = StartCoroutine(SnappingCoroutine());
            }
            else
            {
                if (moveToTransformCoroutine != null) StopCoroutine(moveToTransformCoroutine);

                moveToTransformCoroutine = StartCoroutine(MoveTowardsTargetPosition());
            }
        }

        // Coroutine to use when snapping is disabled.
        private IEnumerator MoveTowardsTargetPosition()
        {
            float distance;

            while (!enableSnapping)
            {
                UpdateTrackingPosition();

                distance = Vector3.Distance(mainCamera.transform.position, BoundedTargetPosition);

                if (Vector2.Distance(mainCamera.transform.position, UnboundedTargetPosition) > leashRadius)
                {
                    mainCamera.transform.position = Vector3.MoveTowards(
                        mainCamera.transform.position,
                        BoundedTargetPosition,
                        Mathf.Clamp(distance * speedMultiplier, minSpeed, maxSpeed) * Time.deltaTime);
                }

                yield return null;
            }
        }

        // Coroutine to use when snapping is enabled.
        private IEnumerator SnappingCoroutine()
        {
            float elapsedTime = 0;
            Vector3 orgPosition = mainCamera.transform.position;
            while (elapsedTime < snapEaseDuration)
            {
                UpdateTrackingPosition();

                elapsedTime += Time.deltaTime;
                mainCamera.transform.position = Vector3.Lerp(orgPosition, BoundedTargetPosition, snapEaseCurve.Evaluate(elapsedTime / snapEaseDuration));

                yield return null;
            }

            while (enableSnapping)
            {
                UpdateTrackingPosition();
                mainCamera.transform.position = BoundedTargetPosition;

                yield return null;
            }
        }
        #endregion



        // Register initial tracking targets.
        private void InitializeTrackingList()
        {
            foreach (var target in trackingTargets)
            {
                RegisterTrackingTarget(target);
            }
        }

        /// <summary>
        /// Helper function for handling register of tracking target.
        /// </summary>
        private void RegisterTrackingTarget(TrackData _Data)
        {
            if (_Data.trackingMode == TrackData.Tracking.Transform &&
                _Data.offsetMode == TrackData.Offset.MovementPrediction &&
                _Data.trackingTransform != null &&
                !velocityTrackMap.ContainsKey(_Data.trackingTransform))
            {
                lastPosTrackMap.Add(_Data.trackingTransform, _Data.trackingTransform.position);
                velocityTrackMap.Add(_Data.trackingTransform, Vector2.zero);
            }
        }

        /// <summary>
        /// Helper function for handling unregister of tracking target.
        /// </summary>
        private void UnregisterTrackingTarget(TrackData _Data)
        {
            if (_Data.trackingMode == TrackData.Tracking.Transform &&
                _Data.offsetMode == TrackData.Offset.MovementPrediction &&
                _Data.trackingTransform != null)
            {
                lastPosTrackMap.Remove(_Data.trackingTransform);
                velocityTrackMap.Remove(_Data.trackingTransform);
            }
        } 

        /// <summary>
        /// Handle bounds clipping and movement prediction to calculate the valid target location.
        /// </summary>
        private void UpdateTrackingPosition()
        {
            if (trackingTargets.Count == 0) return;

            Vector3 finalPosition = Vector3.zero;
            Vector3 minPos = Vector3.positiveInfinity; 
            Vector3 maxPos = Vector3.negativeInfinity;
            float trackingTargetsTotalWeight = 0;
            bool hasVisibleTargetInBounds = false;

            List<TrackingTargetFinalData> targetsInView = new();

            // Cull and calculate all targets in view (bounds)
            foreach (var target in trackingTargets)
            {
                var targetPosition = GetTrackingTargetFinalPosition(target);
                var visibleRect = GetTrackingTargetVisibleRect(target, targetPosition);

                // Only account for targets within visible bounds if has.
                if (enableBounds && !visibleRect.Overlaps(outerBounds)) continue;

                trackingTargetsTotalWeight += target.weight;

                targetsInView.Add(new TrackingTargetFinalData()
                {
                    weight = target.weight,
                    offsetPosition = targetPosition,
                    visibleRect = visibleRect
                });
            }

            // Calculate final target position & visible area for main camera
            foreach (var target in targetsInView)
            {
                finalPosition += target.offsetPosition * (target.weight / trackingTargetsTotalWeight);

                minPos = Vector3.Min(minPos, target.visibleRect.min);
                maxPos = Vector3.Max(maxPos, target.visibleRect.max);
                hasVisibleTargetInBounds = true;
            }

            // Apply calculated final position.
            finalPosition.z = zOffset;
            UnboundedTargetPosition = finalPosition;

            // Clamp final position to bounds
            if (enableBounds)
            {
                finalPosition = validBounds.ClosestPoint(finalPosition);
            }

            finalPosition.z = zOffset;
            BoundedTargetPosition = finalPosition;

            // Apply calculated final target visible area if has
            if (hasVisibleTargetInBounds)
            {
                Rect unboundedTargetVisibleArea = new Rect()
                {
                    size = Vector2.Max(UnboundedTargetPosition - minPos, maxPos - UnboundedTargetPosition) * 2f,
                    center = UnboundedTargetPosition
                };

                targetVisibleArea = unboundedTargetVisibleArea.OverlapRect(OuterBounds);

                SetTargetOrthographicSize_Core(
                    Mathf.Max(targetVisibleArea.height / 2.0f, targetVisibleArea.width / 2.0f / mainCamera.aspect));
            }
        }

        /// <summary>
        /// Helper function for calculating the final position of a given tracking target data.
        /// </summary>
        /// <returns>
        /// A vector represents the calculated final position of the given tracking target data.
        /// </returns>
        internal Vector3 GetTrackingTargetFinalPosition(TrackData _Data)
        {
            Vector2 targetPosition = _Data.TargetPosition;

            // Handle fixed offset
            if (_Data.offsetMode == TrackData.Offset.Fixed)
            {
                targetPosition += _Data.fixedOffset;
            }
            else
            // Handle movement tracking offset
            if (_Data.trackingMode == TrackData.Tracking.Transform && 
                _Data.offsetMode == TrackData.Offset.MovementPrediction && 
                _Data.trackingTransform != null &&
                velocityTrackMap.ContainsKey(_Data.trackingTransform))
            {
                var movementPreVec = velocityTrackMap[_Data.trackingTransform] * _Data.movementPredictionWeight;
                if (_Data.maxOffsetMagnitude > 0)
                    movementPreVec = Vector2.ClampMagnitude(movementPreVec, _Data.maxOffsetMagnitude);

                targetPosition += movementPreVec;
            }

            return targetPosition;
        }

        /// <summary>
        /// Get the tracking target visible area as a world space rect.
        /// </summary>
        /// <param name="_Data"> Tracking target data. </param>
        /// <returns>
        /// A rect represents the visible area of the tracking target in world space.
        /// </returns>
        internal Rect GetTrackingTargetVisibleRect(TrackData _Data, Vector2? _OffsetPosition = null)
        {
            if (!_OffsetPosition.HasValue)
                _OffsetPosition = GetTrackingTargetFinalPosition(_Data);

            Rect rect = new();

            switch (_Data.visibleAreaMode)
            {
                case TrackData.VisibleAreaMode.Point:
                    rect.size = Vector2.zero;
                    rect.center = _OffsetPosition.Value;
                    break;

                case TrackData.VisibleAreaMode.Box:
                    rect.size = _Data.visibleBoxSize;
                    rect.center = _OffsetPosition.Value;
                    break;

                case TrackData.VisibleAreaMode.Rect:
                    rect = _Data.visibleRect;
                    rect.position += _OffsetPosition.Value;
                    break;
            }

            return rect;
        }



        // Helper function to update all tracking transforms with Offset.MovementPrediction
        private void UpdateTrackingTransformsMovement()
        {
            foreach (var transform in velocityTrackMap.Keys.ToList())
            {
                velocityTrackMap[transform] = ((Vector2)transform.position - lastPosTrackMap[transform]) / Time.deltaTime;
                lastPosTrackMap[transform] = transform.position;
            }
        }
        #endregion

        #region Bounds Restriction
        public Rect OuterBounds { get { return outerBounds; } }
        public Rect ValidBounds { get { return validBounds; } }

        private readonly List<CameraBoundsData> boundsStack = new();



        /// <inheritdoc cref="AddBoundsToStack_Core(CameraBoundsData)"/>
        public static bool AddBoundsToStack(CameraBoundsData _BoundsData)
        {
            return Instance.AddBoundsToStack_Core(_BoundsData);
        }

        /// <summary>
        /// Add a camera bounds data to the bounds stack. <br/>
        /// If valid, this will be the new camera bounds.
        /// </summary>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        private bool AddBoundsToStack_Core(CameraBoundsData _BoundsData)
        {
            if (boundsStack.Contains(_BoundsData)) return false;

            boundsStack.Add(_BoundsData);

            UpdateBounds(_BoundsData);

            return true;
        }

        /// <inheritdoc cref="RemoveBoundsFromStack_Core(CameraBoundsData)"/>
        public static bool RemoveBoundsFromStack(CameraBoundsData _BoundsData)
        {
            return Instance.RemoveBoundsFromStack_Core(_BoundsData);
        }

        /// <summary>
        /// Remove a camera bounds data from the bounds stack. <br/>
        /// If this is equals to the current bounds, move down the bounds stack to find new valid bounds.
        /// </summary>
        /// <returns>
        /// Whether the operation was successful or not.
        /// </returns>
        private bool RemoveBoundsFromStack_Core(CameraBoundsData _BoundsData)
        {
            if (!boundsStack.Contains(_BoundsData)) return false;

            boundsStack.Remove(_BoundsData);

            UpdateBounds(boundsStack.Count > 0 ? boundsStack.Last() : null);

            return true;
        }



        private void UpdateBounds(CameraBoundsData? _BoundsData)
        {
            enableBounds = _BoundsData.HasValue;

            if (!_BoundsData.HasValue) return;

            SetOuterBounds(_BoundsData.Value.WorldBounds);
            if (_BoundsData.Value.forceZoom)
            {
                SetOrthographicSizeRange_Core(_BoundsData.Value.minSize, _BoundsData.Value.maxSize);
            }
        }

        private void SetOuterBounds(Rect _Bounds)
        {
            outerBounds = _Bounds;

            RecalculateValidBounds();

            if (enableHardLock)
                RecalculateZoomConstrains();
        }

        internal void RecalculateValidBounds()
        {
            Vector2 cameraSize = 2 * mainCamera.orthographicSize * new Vector2(mainCamera.aspect, 1.0f);
            validBounds = new Rect
            {
                size = Vector2.Max(outerBounds.size - cameraSize, Vector2.zero),
                center = outerBounds.center
            };
        }
        #endregion

        #region Zoom
        private Coroutine zoomEaseCoroutine;

        private float currentTargetOrthoSize;

        private float WorldToScreenRatio { get { return Screen.height / mainCamera.orthographicSize; } }



        /// <inheritdoc cref="SetTargetOrthographicSize_Core(float)"/>
        public static void SetTargetOrthographicSize(float _TargetOrthoSize)
        {
            Instance.SetTargetOrthographicSize_Core(_TargetOrthoSize);
        }

        /// <summary>
        /// Update orthographic size (zoom) of the controlled camera.
        /// </summary>
        /// <param name="_TargetOrthoSize"> The given size to set the controlled camera orthographic size to. </param>
        /// <remarks>
        /// <b>NOTE:</b> The given size will be clamped to the controller's zoom constrains.
        /// </remarks>
        private void SetTargetOrthographicSize_Core(float _TargetOrthoSize)
        {
            UpdateOrthographicSize(Mathf.Clamp(_TargetOrthoSize, minOrthographicSize, maxOrthographicSize));
        }

        /// <inheritdoc cref="SetOrthographicSizeRange_Core(float, float)"/>
        public static void SetOrthographicSizeRange(float _Min, float _Max)
        {
            Instance.SetOrthographicSizeRange_Core(_Min, _Max);
        }

        /// <summary>
        /// Set orthographic size (zoom) limits for the controlled camera.
        /// </summary>
        /// <param name="_Min"> Min orthographic size. <br/>
        ///                     This will be clamped to <paramref name="_Max"/>? or <see cref="maxOrthographicSize"/>. </param>
        /// <param name="_Max"> Max orthographic size. <br/>
        ///                     If <see cref="enableBounds"/> and <see cref="enableHardLock"/> is true, this value is ignored. </param>
        private void SetOrthographicSizeRange_Core(float? _Min, float? _Max)
        {
            if (_Max.HasValue && _Max.Value > 0 && !(enableBounds && enableHardLock))
            {
                maxOrthographicSize = _Max.Value;
            }

            if (_Min.HasValue && _Min.Value > 0)
            {
                minOrthographicSize = Mathf.Min(_Min.Value, maxOrthographicSize);
            }

            UpdateOrthographicSize(Mathf.Clamp(currentTargetOrthoSize, minOrthographicSize, maxOrthographicSize));
        }



        private void InitializeZoomProperties()
        {
            currentTargetOrthoSize = mainCamera.orthographicSize;
        }

        private void RecalculateZoomConstrains()
        {
            if (!enableBounds || !enableHardLock) return;

            float minBoundsSide = Mathf.Min(outerBounds.height / 2.0f, outerBounds.width / 2.0f / mainCamera.aspect);

            maxOrthographicSize = minBoundsSide; // If controlled by bounds, always snap max zoom to the allowed size
            minOrthographicSize = Mathf.Min(minOrthographicSize, maxOrthographicSize);

            UpdateOrthographicSize(Mathf.Clamp(currentTargetOrthoSize, minOrthographicSize, maxOrthographicSize));
        }

        private void UpdateOrthographicSize(float _NewSize)
        {
            if (_NewSize == currentTargetOrthoSize) return;

            currentTargetOrthoSize = _NewSize;

            // Only start coroutine while playing to avoid disabled script.
            if (Application.isPlaying)
            {
                if (zoomEaseCoroutine != null)
                    StopCoroutine(zoomEaseCoroutine);

                zoomEaseCoroutine = StartCoroutine(ZoomEaseCoroutine(_NewSize));
            }
            else
            {
                mainCamera.orthographicSize = _NewSize;
            }
        }

        private IEnumerator ZoomEaseCoroutine(float _TargetSize)
        {
            float orgSize = mainCamera.orthographicSize;
            float elapsedDuration = 0;

            while (elapsedDuration < zoomEaseDuration)
            {
                elapsedDuration += Time.deltaTime;
                mainCamera.orthographicSize = Mathf.Lerp(orgSize, _TargetSize, zoomEaseCurve.Evaluate(elapsedDuration / zoomEaseDuration));

                RecalculateValidBounds();

                yield return null;
            }

            mainCamera.orthographicSize = _TargetSize;

            RecalculateValidBounds();
        }
        #endregion
    }
}

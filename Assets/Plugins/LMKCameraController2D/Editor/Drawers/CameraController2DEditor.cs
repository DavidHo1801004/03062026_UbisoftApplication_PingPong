using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;

using LMK.Editor;

namespace LMK.CameraController2D.Editor
{
    [CustomEditor(typeof(CameraController2D))]
    public class CameraController2DEditor : CustomBaseEditor
    {
        private CameraController2D asTarget;

        private SerializedProperty zOffset;
        private SerializedProperty trackingTargets;

        private SerializedProperty enableSnapping;
        private SerializedProperty snapEaseCurve;
        private SerializedProperty snapEaseDuration;

        private SerializedProperty leashRadius;
        private SerializedProperty speedMultiplier;
        private SerializedProperty maxSpeed;
        private SerializedProperty minSpeed;

        private SerializedProperty enableBounds;
        private SerializedProperty outerBounds;

        private SerializedProperty enableHardLock;
        private SerializedProperty zoomEaseCurve;
        private SerializedProperty zoomEaseDuration;

        private SerializedProperty minOrthographicSize;
        private SerializedProperty maxOrthographicSize;

        private AnimBool expandTracking;
        private AnimBool expandBounds;
        private AnimBool expandDebug;

        private static bool debugBounds = true;
        private static bool debugTrackingTargets = true;
        private static bool debugTargetVisibleArea = true;

        private bool IsTrackingExpanded
        {
            get => SessionState.GetBool($"Tracking {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", false);
            set => SessionState.SetBool($"Tracking {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", value);
        }

        private bool IsBoundsExpanded
        {
            get => SessionState.GetBool($"Bounds {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", false);
            set => SessionState.SetBool($"Bounds {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", value);
        }

        private bool IsDebugExpanded
        {
            get => SessionState.GetBool($"Debug {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", false);
            set => SessionState.SetBool($"Debug {typeof(CameraController2DEditor)} : {serializedObject.targetObject.GetInstanceID()}", value);
        }




        private void OnEnable()
        {
            asTarget = (CameraController2D)target;

            zOffset = AssignToProperty("zOffset");
            trackingTargets = AssignToProperty("trackingTargets");

            enableSnapping = AssignToProperty("enableSnapping");
            snapEaseCurve = AssignToProperty("snapEaseCurve");
            snapEaseDuration = AssignToProperty("snapEaseDuration");

            leashRadius = AssignToProperty("leashRadius");
            speedMultiplier = AssignToProperty("speedMultiplier");
            maxSpeed = AssignToProperty("maxSpeed");
            minSpeed = AssignToProperty("minSpeed");

            enableBounds = AssignToProperty("enableBounds");
            outerBounds = AssignToProperty("outerBounds");

            enableHardLock = AssignToProperty("enableHardLock");
            zoomEaseCurve = AssignToProperty("zoomEaseCurve");
            zoomEaseDuration = AssignToProperty("zoomEaseDuration");

            minOrthographicSize = AssignToProperty("minOrthographicSize");
            maxOrthographicSize = AssignToProperty("maxOrthographicSize");


            expandTracking = new(IsTrackingExpanded);
            expandTracking.valueChanged.AddListener(Repaint);

            expandBounds = new(IsBoundsExpanded);
            expandBounds.valueChanged.AddListener(Repaint);

            expandDebug = new(IsDebugExpanded);
            expandDebug.valueChanged.AddListener(Repaint);

            var icon = Resources.Load<Texture2D>("Sprites/CameraController_icon_32");
            if (icon)
            {
                EditorGUIUtility.SetIconForObject(target, icon);
            }
        }

        private void OnSceneGUI()
        {
            if (!asTarget.enabled) return;

            // Constant handle size since we only drawing in 2D.
            float scaleHandleSize = HandleUtility.GetHandleSize(Vector3.zero);

            #region Bounds Controls
            if (asTarget.IsBoundsEnabled)
            {
                // Bounds size controls
                Handles.color = Color.yellow;

                Vector2 sidePos;
                Vector2 sizeDiff = Vector2.zero;
                Vector2 extents = asTarget.OuterBounds.size / 2;
                Vector2 center = asTarget.OuterBounds.center;

                sidePos = Offset.sides[0] * extents + center;
                sizeDiff.x -= ((Vector2)Handles.FreeMoveHandle(sidePos, scaleHandleSize / 25.0f, Vector3.zero, Handles.DotHandleCap) - sidePos).x;

                sidePos = Offset.sides[1] * extents + center;
                sizeDiff.x += ((Vector2)Handles.FreeMoveHandle(sidePos, scaleHandleSize / 25.0f, Vector3.zero, Handles.DotHandleCap) - sidePos).x;

                sidePos = Offset.sides[2] * extents + center;
                sizeDiff.y -= ((Vector2)Handles.FreeMoveHandle(sidePos, scaleHandleSize / 25.0f, Vector3.zero, Handles.DotHandleCap) - sidePos).y;

                sidePos = Offset.sides[3] * extents + center;
                sizeDiff.y += ((Vector2)Handles.FreeMoveHandle(sidePos, scaleHandleSize / 25.0f, Vector3.zero, Handles.DotHandleCap) - sidePos).y;

                Undo.RecordObject(asTarget, "Change Bounds Size of CameraController2D");
                asTarget.outerBounds.center -= sizeDiff / 2;
                asTarget.outerBounds.size += sizeDiff;
                EditorUtility.SetDirty(asTarget);

                // Bounds position controls
                Handles.color = Color.yellow;

                Vector2 centerDiff = (Vector2)Handles.FreeMoveHandle(center, scaleHandleSize / 25.0f, Vector3.zero, Handles.DotHandleCap) - center;

                Undo.RecordObject(asTarget, "Change Bounds Position of CameraController2D");
                asTarget.outerBounds.center += centerDiff;
                EditorUtility.SetDirty(asTarget);

                // Draw bounds
                HandleUtils.DrawRect(asTarget.OuterBounds, ValidBounds(asTarget.OuterBounds) ? Color.yellow : Color.red, "(Outer Bounds)");

                if (debugBounds)
                {
                    asTarget.RecalculateValidBounds();
                    HandleUtils.DrawRect(asTarget.ValidBounds, ValidBounds(asTarget.ValidBounds) ? Color.green : Color.red, "(Valid Bounds)", true);
                }
            }
            #endregion

            if (debugTrackingTargets)
            {
                #region Tracking Targets
                var trackingTargets = asTarget.TrackingTargets;
                for (int i = 0; i < trackingTargets.Length; i++)
                {
                    var targetPos = asTarget.GetTrackingTargetFinalPosition(trackingTargets[i]);

                    Handles.color = Color.HSVToRGB(i * 0.1f, 1f, 0.8f);
                    Handles.DrawLine(trackingTargets[i].TargetPosition, targetPos, 1f);
                    Handles.DrawSolidDisc(targetPos, Vector3.forward, 0.1f * scaleHandleSize);
                    Handles.DrawDottedLine(targetPos, (Vector2)asTarget.UnboundedTargetPosition, 5f);

                    Handles.color = Color.white;
                    Handles.Label(
                        targetPos + 0.2f * scaleHandleSize * Vector3.right, 
                        $"#{i} ({trackingTargets[i].weight}W)");
                }
                #endregion

                #region Final Target Location
                Handles.color = Color.white;
                Vector2 size = 0.2f * scaleHandleSize * Vector2.one;

                Handles.DrawSolidRectangleWithOutline(
                    new Rect((Vector2)asTarget.BoundedTargetPosition - size / 2f, size),
                    Color.green, Color.white);

                Handles.DrawSolidRectangleWithOutline(
                    new Rect((Vector2)asTarget.UnboundedTargetPosition - size / 2f, size),
                    Color.cyan, Color.white);
                #endregion
            }

            #region Visible Area
            if (debugTargetVisibleArea)
            {
                // Individual targets visible area
                var trackingTargets = asTarget.TrackingTargets;
                for (int i = 0; i < trackingTargets.Length; i++)
                {
                    HandleUtils.DrawRect(
                        asTarget.GetTrackingTargetVisibleRect(trackingTargets[i]), 
                        Color.HSVToRGB(1f - i * 0.1f, 0.8f, 1f), null, true);
                }

                // Overall target visible area
                HandleUtils.DrawRect(asTarget.TargetVisibleArea, Color.cyan, "Target View", false);
            }
            #endregion
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();

            base.OnInspectorGUI();

            EditorGUILayout.Space();

            #region Tracking
            IsTrackingExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(IsTrackingExpanded, "Tracking", CustomGUIStyles.foldoutHeader);
            EditorGUILayout.EndFoldoutHeaderGroup();
            expandTracking.target = IsTrackingExpanded;

            if (EditorGUILayout.BeginFadeGroup(expandTracking.faded))
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(zOffset);
                EditorGUILayout.PropertyField(trackingTargets);

                EditorGUILayout.PropertyField(enableSnapping);

                if (enableSnapping.boolValue)
                {
                    EditorGUILayout.PropertyField(snapEaseDuration);
                    EditorGUILayout.PropertyField(snapEaseCurve);
                }
                else
                {
                    EditorGUILayout.PropertyField(leashRadius);
                    EditorGUILayout.PropertyField(speedMultiplier);
                    EditorGUILayout.PropertyField(maxSpeed);
                    EditorGUILayout.PropertyField(minSpeed);
                }

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFadeGroup();
            #endregion

            #region Bounds
            IsBoundsExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(IsBoundsExpanded, "Bounds", CustomGUIStyles.foldoutHeader);
            EditorGUILayout.EndFoldoutHeaderGroup();
            expandBounds.target = IsBoundsExpanded;

            if (EditorGUILayout.BeginFadeGroup(expandBounds.faded))
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(enableBounds);

                if (enableBounds.boolValue)
                {
                    EditorGUILayout.PropertyField(outerBounds);

                    EditorGUILayout.Space();
                    EditorGUILayout.PropertyField(enableHardLock);

                    if (enableHardLock.boolValue)
                    {
                        EditorGUILayout.PropertyField(zoomEaseCurve);
                        EditorGUILayout.PropertyField(zoomEaseDuration);
                    }
                }

                #region Zoom Constrains
                float minValue = minOrthographicSize.floatValue;
                float maxValue = maxOrthographicSize.floatValue;

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PrefixLabel("Zoom Constrains");

                EditorGUI.indentLevel--;
                minValue = EditorGUILayout.FloatField(minValue, GUILayout.Width(40));
                EditorGUILayout.MinMaxSlider(
                    ref minValue,
                    ref maxValue,
                    0.0f,
                    GetMaxZoomConstrains());
                maxValue = EditorGUILayout.FloatField(maxValue, GUILayout.Width(40));
                EditorGUI.indentLevel++;

                EditorGUILayout.EndHorizontal();

                minOrthographicSize.floatValue = minValue;
                maxOrthographicSize.floatValue = maxValue;

                if (enableBounds.boolValue && enableHardLock.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "Max constrain is limited by current outer bounds.\n" +
                        $"Disable '{enableHardLock.displayName}' to use this freely.",
                        MessageType.Warning);
                }
                #endregion

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFadeGroup();
            #endregion

            #region Debug
            IsDebugExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(IsDebugExpanded, "Debug", CustomGUIStyles.foldoutHeader);
            EditorGUILayout.EndFoldoutHeaderGroup();
            expandDebug.target = IsDebugExpanded;

            if (EditorGUILayout.BeginFadeGroup(expandDebug.faded))
            {
                EditorGUILayout.Space();
                EditorGUI.indentLevel++;

                debugBounds = EditorGUILayout.Toggle("Bounds", debugBounds);
                debugTrackingTargets = EditorGUILayout.Toggle("Tracking Targets", debugTrackingTargets);
                debugTargetVisibleArea = EditorGUILayout.Toggle("Target Visible Area", debugTargetVisibleArea);

                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndFadeGroup();
            #endregion

            serializedObject.ApplyModifiedProperties();
        }



        private bool ValidBounds(Rect _Bounds)
        {
            return _Bounds.size.x >= 0 && _Bounds.size.y >= 0;
        }

        private float GetMaxZoomConstrains()
        {
            if (enableBounds.boolValue && enableHardLock.boolValue)
            {
                return Mathf.Min(outerBounds.rectValue.height / 2.0f, outerBounds.rectValue.width / 2.0f / CameraController2D.MainCamera.aspect);
            }
            else
            {
                return 10.0f;
            }
        }
    }
}

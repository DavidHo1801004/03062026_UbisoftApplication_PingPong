using UnityEditor;
using UnityEngine;

namespace LMK.CameraController2D.Editor
{
    /// <summary>
    /// Static helper class for drawing and handling Unity's handles.
    /// </summary>
    internal static class HandleUtils
    {
        /// <summary>
        /// Draw a named rect.
        /// </summary>
        public static void DrawRect(Rect _Rect, Color _Color, string _Name = null, bool _Dotted = false)
        {
            Handles.color = _Color;

            // Draw box
            Vector2 extents = _Rect.size / 2;
            for (int i = 0; i < 4; i++)
            {
                Vector2 start = Offset.corners[i] * extents + _Rect.center;
                Vector2 end = Offset.corners[(i + 1) % 4] * extents + _Rect.center;
                if (_Dotted)
                    Handles.DrawDottedLine(start, end, 4.0f);
                else
                    Handles.DrawLine(start, end);
            }

            // Draw label
            if (!string.IsNullOrWhiteSpace(_Name))
            {
                GUIStyle labelStyle = new();
                labelStyle.normal.textColor = _Color;
                Vector2 labelPos = Offset.GetBottomLeftCornerOffset(_Rect) * _Rect.size / 2 + _Rect.center;
                Handles.Label(
                    labelPos + 0.1f * HandleUtility.GetHandleSize(labelPos) * Vector2.down,
                    _Name,
                    labelStyle);
            }
        }

        /// <summary>
        /// Draw a named rect with size controls
        /// </summary>
        public static Rect DrawRectHandles(Rect _Rect, Color _Color, float _HandleSize, string _Name = null, bool _Dotted = false)
        {
            Handles.color = _Color;

            Vector2 min = _Rect.min;
            Vector2 max = _Rect.max;
            Vector2 center = _Rect.center;

            Vector3 snap = EditorSnapSettings.move;

            // Draw rectangle
            DrawRect(_Rect, _Color, _Name, _Dotted);

            EditorGUI.BeginChangeCheck();

            // Left
            Vector3 leftPos = new(min.x, center.y);
            leftPos = Handles.FreeMoveHandle(leftPos, _HandleSize, snap, Handles.DotHandleCap);
            min.x = leftPos.x;

            // Right
            Vector3 rightPos = new(max.x, center.y);
            rightPos = Handles.FreeMoveHandle(rightPos, _HandleSize, snap, Handles.DotHandleCap);
            max.x = rightPos.x;

            // Top
            Vector3 topPos = new(center.x, min.y);
            topPos = Handles.FreeMoveHandle(topPos, _HandleSize, snap, Handles.DotHandleCap);
            min.y = topPos.y;

            // Down
            Vector3 bottomPos = new(center.x, max.y);
            bottomPos = Handles.FreeMoveHandle(bottomPos, _HandleSize, snap, Handles.DotHandleCap);
            max.y = bottomPos.y;

            // Clamp to prevent flipping
            if (min.x > max.x) (min.x, max.x) = (max.x, min.x);
            if (min.y > max.y) (min.y, max.y) = (max.y, min.y);

            Vector3 newCenter = Handles.FreeMoveHandle(center, _HandleSize, snap, Handles.DotHandleCap);

            if (EditorGUI.EndChangeCheck())
            {
                Vector2 size = max - min;

                // If center moved, move entire rect
                if ((Vector2)newCenter != center)
                {
                    _Rect.center = newCenter;
                }
                // Otherwise resize
                else
                {
                    _Rect.min = min;
                    _Rect.max = max;
                }
            }

            return _Rect;
        }
    }
}

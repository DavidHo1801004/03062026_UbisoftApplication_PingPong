using UnityEngine;

namespace LMK.CameraController2D.Editor
{
    /// <summary>
    /// Static helper class for calculating rect offsets.
    /// </summary>
    internal static class Offset
    {
        public const int TOP_RIGHT = 0;
        public const int BOTTOM_RIGHT = 1;
        public const int BOTTOM_LEFT = 2;
        public const int TOP_LEFT = 3;
        public static readonly Vector2[] corners = new Vector2[]
        {
                new(1, 1),      // Top Right
                new(1, -1),     // Bottom Right
                new(-1, -1),    // Bottom Left
                new(-1, 1),     // Top Left
        };

        public const int TOP = 0;
        public const int RIGHT = 1;
        public const int BOTTOM = 2;
        public const int LEFT = 3;
        public static readonly Vector2[] sides = new Vector2[]
        {
                new(0, 1),      // Top
                new(1, 0),      // Right
                new(0, -1),     // Bottom
                new(-1, 0),     // Left
        };



        public static Vector2 GetBottomLeftCornerOffset(Rect _Bounds)
        {
            if (_Bounds.size.x < 0)
                if (_Bounds.size.y < 0)
                    return corners[TOP_RIGHT];
                else
                    return corners[BOTTOM_RIGHT];
            else
                if (_Bounds.size.y < 0)
                    return corners[TOP_LEFT];
                else
                    return corners[BOTTOM_LEFT];
        }
    }
}

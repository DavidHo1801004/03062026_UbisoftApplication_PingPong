using UnityEngine;

namespace LMK.CameraController2D.Extensions
{
    /// <summary>
    /// Extensions for <see cref="Rect"/>
    /// </summary>
    public static class RectExtension
    {
        /// <summary>
        /// Get the closest point on the bounding rect.
        /// </summary>
        /// <param name="_Point"> The point on the bounding box or inside the bounding box. </param>
        /// <returns>
        /// The closest point on the bounding rect. <br/>
        /// If the point is inside the bounding rect, unmodified point position will be returned.
        /// </returns>
        public static Vector2 ClosestPoint(this Rect _Rect, Vector2 _Point)
        {
            return new Vector2(
                Mathf.Clamp(_Point.x, _Rect.xMin, _Rect.xMax),
                Mathf.Clamp(_Point.y, _Rect.yMin, _Rect.yMax));
        }

        /// <summary>
        /// Get the overlap area between 2 <see cref="Rect"/>s.
        /// </summary>
        /// <returns>
        /// A new <see cref="Rect"/> that define the overlapped area.
        /// </returns>
        public static Rect OverlapRect(this Rect _A, Rect _B)
        {
            return new Rect()
            {
                min = Vector2.Max(_A.min, _B.min),
                max = Vector2.Min(_A.max, _B.max)
            };
        } 
    }
}

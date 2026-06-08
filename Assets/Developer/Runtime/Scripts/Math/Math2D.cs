using Unity.Mathematics;

namespace Developer.Mathematics
{
    /// <summary>
    /// Define mathematics functions for basic algebra & 2D topology.
    /// </summary>
    public static class Math2D
    {
        const float EPSILON = 1e-6f;

        /// <summary>
        /// Get the furthest point on the perimeter of a polygon in a given direction. <br/>
        /// Prefer points closer to the line defined by <paramref name="_Direction"/> at <paramref name="_Origin"/>.
        /// </summary>
        public static float2 FurthestPerimeterPoint(
            in this Polygon _Polygon,
            float2 _Direction,
            float2 _Origin)
        {
            int count = _Polygon.Count;
            if (count == 0) return float2.zero; // invalid polygon

            float dirLenSq = math.lengthsq(_Direction);
            if (dirLenSq < EPSILON) return _Polygon[0]; // invalid direction

            float2 dir = math.normalize(_Direction);
            float2 normal = new(-dir.y, dir.x);
            float maxP = float.NegativeInfinity;

            // Pass 1, get max projection value
            for (int i = 0; i < count; i++)
            {
                maxP = math.max(
                    maxP,
                    math.dot(_Polygon[i], _Direction));
            }

            // Pass 2, solve for edge and point cases
            float bestDistance = float.PositiveInfinity;
            float2 bestPoint = _Polygon[0];

            for (int i = 0; i < count; i++)
            {
                float2 a = _Polygon[i];
                float da = math.dot(a, _Direction);
                if (math.abs(da - maxP) <= EPSILON) continue;

                float2 b = _Polygon[(i + 1) % count];
                float db = math.dot(b, _Direction);

                if (math.abs(db - maxP) <= EPSILON) // -> A is support point
                {
                    float dist = math.abs(math.dot(a - _Origin, normal));
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestPoint = a;
                    }
                    continue;
                }

                float2 e = b - a;
                float t = math.dot(_Origin - a, e) / math.lengthsq(e);

                // misses by B
                if (t <= 0)
                {
                    float dist = math.abs(math.dot(a - _Origin, normal));
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestPoint = a;
                    }
                    continue;
                }
                // misses by A
                if (t >= 1)
                {
                    float dist = math.abs(math.dot(b - _Origin, normal));
                    if (dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestPoint = b;
                    }
                    continue;
                }

                // intersects -> best point is intersection
                float2 p = a + e * t;
                return p;
            }

            return bestPoint;
        }
    }
}

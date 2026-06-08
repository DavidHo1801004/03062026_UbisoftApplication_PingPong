using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Developer.Mathematics
{
    /// <summary>
    /// Represents a simple collection of vertices.
    /// </summary>
    public struct Polygon : IDisposable
    {
        private NativeArray<float2> vertices;
        public int Count => vertices.Length;
        public bool IsCreated => vertices.IsCreated;

        public Polygon(Allocator _Allocator, int _Capacity)
        {
            vertices = new NativeArray<float2>(_Capacity, _Allocator);
        }

        public Polygon(NativeArray<float2> _Vertices)
        {
            vertices = _Vertices;
        }

        public Polygon(Allocator _Allocator, params Vector2[] _Vertices)
        {
            vertices = new NativeArray<float2>(_Vertices.Length, _Allocator);

            for (int i = 0; i < _Vertices.Length; i++)
            {
                vertices[i] = _Vertices[i];
            }
        }

        public float2 this[int index] 
            => vertices[index];

        // Get vertices with auto-wrapped index
        public float2 GetWrapped(int _Index)
        {
            int count = vertices.Length;

            if (count == 0)
                return float2.zero;

            _Index %= count;

            if (_Index < 0)
                _Index += count;

            return vertices[_Index];
        }

        // Copy vertices from list to polygon
        // If given vertices exceed polygon capacity, the remaining elements will be ignored
        // If given vertices is less than polygon capacity, the extra vertices will remain the same
        public void CopyFrom(params Vector2[] _Vertices)
        {
            int len = math.min(_Vertices.Length, Count);
            for (int i = 0; i < len; i++)
            {
                vertices[i] = _Vertices[i];
            }
        }

        public void Dispose()
        {
            if (vertices.IsCreated)
                vertices.Dispose();
        }
    }
}

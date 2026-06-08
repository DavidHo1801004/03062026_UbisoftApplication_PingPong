using UnityEngine;

namespace Developer.General.Settings
{
    [System.Serializable]
    public struct GameplaySettings
    {
        [Header("PADDLE")]
        [Min(0)]
        public float paddleSpeed;
        [Min(0)]
        public float paddleAcceleration;
        [Min(0)]
        public float paddleDeceleration;
        [Min(1)]
        public int paddleMaxHealth;
        [Min(1)]
        public int paddleSize;

        [Header("BALL")]
        [Min(0)]
        public float ballSpeed;
        [Tooltip("Speed increases per second. Reset on goal.")]
        [Min(0)]
        public float ballSpeedIncrement;
        [Range(0, 45)]
        [Tooltip("Value used to get pseudo-random angle on bounce")]
        public float maxRandomAngle;

        [Header("FIELD")]
        public Vector2 fieldSize;
        [Range(0f, 0.4f)]
        public float teamZonePercentage;
        [Min(3)]
        public float goalSize;
    }
}

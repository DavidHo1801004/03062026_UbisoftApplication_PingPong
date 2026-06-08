using UnityEngine;

namespace Developer.General.Managers
{
    /// <summary>
    /// Static class for controlling custom time scale, separated from <see cref="UnityEngine.Time"/>.
    /// </summary>
    public static class TimeManager
    {
        /// <summary>
        /// Use this property instead of directly using <see cref="Time.deltaTime"/> to enable
        /// other features of <see cref="TimeManager"> like time pause or slow-mo.
        /// </summary>
        public static float DeltaTime { get { return UnityEngine.Time.deltaTime * TimeScale; } }

        /// <summary>
        /// Use this property instead of directly using <see cref="Time.fixedDeltaTime"/> to enable
        /// other features of <see cref="TimeManager"> like time pause or slow-mo.
        /// </summary>
        public static float FixedDeltaTime { get { return UnityEngine.Time.fixedDeltaTime * TimeScale; } }

        private static float timeScale = 1.0f;

        /// <summary>
        /// Clamped to [0..1]. Value of 1 will run at normal speed. Value of 0 will completely pause time.
        /// </summary>
        public static float TimeScale
        {
            get => timeScale;
            set
            {
                if (value != timeScale)
                {
                    elapsedTime += (UnityEngine.Time.unscaledTime - unscaledElapsedTime) * timeScale;
                    unscaledElapsedTime = UnityEngine.Time.unscaledTime;
                }

                timeScale = value;
            }
        }

        private static float unscaledElapsedTime;

        private static float elapsedTime;

        /// <summary>
        /// Time in seconds since the start of the game, dependent on <see cref="TimeScale"/>.
        /// </summary>
        public static float Time
        {
            get => elapsedTime + (UnityEngine.Time.unscaledTime - unscaledElapsedTime) * timeScale;
            private set => elapsedTime = value;
        }



        static TimeManager()
        {
            unscaledElapsedTime = elapsedTime = UnityEngine.Time.unscaledTime;
        }
    }
}

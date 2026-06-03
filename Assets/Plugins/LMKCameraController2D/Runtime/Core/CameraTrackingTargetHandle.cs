namespace LMK.CameraController2D.Extensions
{
    /// <summary>
    /// Internal handle class for <see cref="CameraTrackingTargetData"/>. <br/>
    /// Generated from <see cref="CameraController2D"/>.
    /// </summary>
    public class CameraTrackingTargetHandle
    {
        /// <summary>
        /// Copy of the original data (readonly) <br/>
        /// </summary>
        public readonly CameraTrackingTargetData data;

        internal int id;

        internal CameraTrackingTargetHandle(CameraTrackingTargetData _Data, int _ID)
        {
            this.data = _Data;
            this.id = _ID;
        }
    }
}

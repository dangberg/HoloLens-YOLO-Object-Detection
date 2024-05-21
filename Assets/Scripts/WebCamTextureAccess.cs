using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Singleton for camera access.
    /// </summary>
    public class WebCamTextureAccess
    {
#if UNITY_EDITOR
        private const string ObsCameraName = "OBS Virtual Camera";
#endif  

        private const int CameraFps = 4;
        private readonly Vector2Int requestedCameraResolution = new(896, 504);

        /// <summary>
        ///     Singleton instance of <see cref="WebCamTextureAccess"/>.
        /// </summary>
        public static WebCamTextureAccess Instance = new();

        /// <summary>
        ///     Creates a new instance of <see cref="WebCamTextureAccess" />.
        /// </summary>
        public WebCamTextureAccess()
        {
#if UNITY_EDITOR
            // prioritize OBS camera if available
            if (WebCamTexture.devices.Any(x => x.name.Equals(ObsCameraName)))
            {
                this.WebCamTexture = new WebCamTexture(ObsCameraName, this.requestedCameraResolution.x, this.requestedCameraResolution.y, CameraFps);
            }
            else
            {
                this.WebCamTexture = new WebCamTexture(this.requestedCameraResolution.x, this.requestedCameraResolution.y, CameraFps);
            }
#else
            // use front camera of HoloLens
            this.WebCamTexture = new WebCamTexture(this.requestedCameraResolution.x, this.requestedCameraResolution.y, CameraFps);
#endif
            this.ActualCameraSize = new Vector2Int(this.WebCamTexture.width, this.WebCamTexture.height);
        }

        /// <summary>
        ///     The current texture of the camera.
        /// </summary>
        public WebCamTexture WebCamTexture { get; }

        /// <summary>
        ///     The resolution of the camera.
        /// </summary>
        public Vector2Int ActualCameraSize { get; private set; }

        /// <summary>
        ///     Starts the webcam.
        /// </summary>
        public void Play()
        {
            if (WebCamTexture.devices.Length <= 0)
            {
                return;
            }
            this.WebCamTexture.Play();
            this.ActualCameraSize = new Vector2Int(this.WebCamTexture.width, this.WebCamTexture.height);
        }

        /// <summary>
        ///     Stops the webcam.
        /// </summary>
        public void Stop()
        {
            this.WebCamTexture.Stop();
        }
    }
}
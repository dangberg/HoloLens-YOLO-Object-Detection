using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Struct containing position information of the camera.
    /// </summary>
    public struct CameraTransform
    {
        /// <summary>
        ///     Creates a new <see cref="CameraTransform" />.
        /// </summary>
        /// <param name="camera">The camera for which the position information should be stored.</param>
        public CameraTransform(Component camera)
        {
            Transform originalTransform = camera.transform;
            this.Position = originalTransform.position;
            this.Forward = originalTransform.forward;
            this.Right = originalTransform.right;
            this.Up = originalTransform.up;
        }

        /// <summary>
        ///     Current position of the camera.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        ///     Forward direction of the camera.
        /// </summary>
        public Vector3 Forward;

        /// <summary>
        ///     Right direction of the camera.
        /// </summary>
        public Vector3 Right;

        /// <summary>
        ///     Up direction of the camera.
        /// </summary>
        public Vector3 Up;
    }
}
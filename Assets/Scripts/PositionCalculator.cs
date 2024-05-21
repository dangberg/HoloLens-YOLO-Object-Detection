using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Helper class for transforming detected positions in an image to world coordinate system coordinates.
    /// </summary>
    public class PositionCalculator
    {
        private static WebCamTextureAccess WebCamTextureAccess => WebCamTextureAccess.Instance;

        /// <summary>
        ///     Calculates the center or bottom point position of the yolo object in world space.
        /// </summary>
        /// <param name="yoloItem">Yolo item from the model.</param>
        /// <param name="cameraTransform">Current camera position.</param>
        /// <returns>Position in world space.</returns>
        public static Vector3? CalculatePointInSpace(YoloItem yoloItem, CameraTransform cameraTransform)
        {
            Vector2 positionInImage = ScaleBack(new Vector2(yoloItem.Center.x, yoloItem.Center.y));
            Vector3 positionInSpace = GetPositionInSpace(cameraTransform, positionInImage);
            return CastOnSpatialMap(positionInSpace, cameraTransform);
        }

        /// <summary>
        ///     Calculates the corner point positions of the yolo object in world space.
        /// </summary>
        /// <param name="yoloItem">Yolo item from the model.</param>
        /// <param name="cameraTransform">Current camera position.</param>
        /// <returns>The four corner points.</returns>
        public static Vector3[] CalculateCornerPoints(YoloItem yoloItem, CameraTransform cameraTransform)
        {
            Vector3[] cornerPoints = new Vector3[4];
            int i = 0;
            Vector2 topRight = yoloItem.TopLeft + new Vector2(yoloItem.Size.x, 0);
            Vector2 bottomLeft = yoloItem.BottomRight - new Vector2(yoloItem.Size.x, 0);
            foreach (Vector2 cornerPoint in new[] { yoloItem.TopLeft, topRight, yoloItem.BottomRight, bottomLeft })
            {
                Vector2 scaled = ScaleBack(cornerPoint);
                Vector3 posInSpace = GetPositionInSpace(cameraTransform, scaled);
                cornerPoints[i++] = posInSpace;
            }

            return cornerPoints;
        }

        private static Vector2 ScaleBack(Vector2 detectedPosition)
        {
            // ML model uses different resolution than the actual camera resolution => scale the detected position back to camera resolution.
            // After scaling the coordinates are in the range (-0.5, -0.5) - (0.5, 0.5) (=> center coordinate (0, 0)).
            int cameraResolutionX = WebCamTextureAccess.ActualCameraSize.x;
            int cameraResolutionY = WebCamTextureAccess.ActualCameraSize.y;
            return new Vector2(
                (detectedPosition.x / Parameters.ModelImageResolution.x * cameraResolutionX - (float)cameraResolutionX / 2) / cameraResolutionX,
                (detectedPosition.y / Parameters.ModelImageResolution.y * cameraResolutionY - (float)cameraResolutionY / 2) / cameraResolutionY
            );
        }

        /// <summary>
        ///     Converts a position inside the image to a position in space.
        /// </summary>
        /// <param name="cameraTransform">Current camera position.</param>
        /// <param name="positionInImage">Relative position inside the image (range (-0.5, -0.5) - (0.5, 0.5))</param>
        /// <returns>Position of the detected object one unit in front of the camera.</returns>
        public static Vector3 GetPositionInSpace(CameraTransform cameraTransform, Vector2 positionInImage)
        {
            // Move from the camera origin towards the position inside the image.
            // Image is placed one unit in front of the camera => add forward vector.
            // Size of the image is given by the virtualProjectionPlaneSize vector.
            return cameraTransform.Position + cameraTransform.Up * Parameters.HeightOffset + cameraTransform.Forward +
                   cameraTransform.Right * (positionInImage.x * Parameters.VirtualProjectionPlane.x) -
                   cameraTransform.Up * (positionInImage.y * Parameters.VirtualProjectionPlane.y);
        }

        private static Vector3? CastOnSpatialMap(Vector3 positionInSpace, CameraTransform cameraTransform)
        {
            // Try to send a cast to the (invisible) spatial mesh.
            // Cast origin is slightly above the camera position since the front camera of the HoloLens is placed above the eyes.
            Vector3 sphereCastOrigin = cameraTransform.Position + Parameters.SphereCastOffset * cameraTransform.Up;
            Vector3 direction = positionInSpace - sphereCastOrigin;

            if (PhysicsCaller.SphereCastOnSpatialMesh(sphereCastOrigin, direction, out RaycastHit hitInfo))
            {
                return hitInfo.point;
            }

            return null;
        }

        /// <summary>
        ///     Determines whether the object is visible in the current camera view.
        /// </summary>
        /// <param name="position">Position of the object.</param>
        /// <returns>Whether the object is visible in the current camera view.</returns>
        public static bool IsObjectInCameraView(Vector3 position)
        {
            Vector3 viewPos = Camera.main.WorldToViewportPoint(position);
            return viewPos.x is <= 1f and >= 0f && viewPos.y is <= 1 and >= 0 && viewPos.z >= 0;
        }
    }
}
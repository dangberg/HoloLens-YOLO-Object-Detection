using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Parameters for the YOLO model executor.
    /// </summary>
    public static class Parameters
    {
        // Resolution of the model input image.
        public static Vector2Int ModelImageResolution = new(640, 640);

        // Performance parameters.
        internal const int LayersHigh = 10;
        internal const int LayersLow = 5;
        internal const float ThresholdHigh = 0.6f;
        internal const float ThresholdMedium = 0.3f;
        internal const float ThresholdLow = 0.1f;

        // Parameter for filtering overlapping objects in model output.
        internal const float OverlapThreshold = 0.15f;

        // Position calculator parameters (2D => 3D).
        internal const float VirtualProjectionPlaneWidth = 1.3f;
        public static Vector2 VirtualProjectionPlane = new(VirtualProjectionPlaneWidth, (float)ModelImageResolution.y / ModelImageResolution.x);
        internal const int MaxSphereLength = 10;
        internal const float SphereCastSize = 0.05f;
        internal const float HeightOffset = -0.06f;
        internal const float SphereCastOffset = 0.15f;

        // Parameters for features based on Object Recognition.
        internal const float MinTimesSeen = 4;
        internal const float ObjectTimeOut = 3f;
        internal const float MaxIdenticalObject = 0.4f;
    }
}
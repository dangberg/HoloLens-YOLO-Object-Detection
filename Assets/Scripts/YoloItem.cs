using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Represents a Yolo item in version v8.
    /// </summary>
    public struct YoloItem
    {
        /// <summary>
        ///     Stores the data from the output tensor.
        ///     Processes the output of the Yolov8.
        /// </summary>
        /// <param name="center">Center position of the recognized object.</param>
        /// <param name="size">Size of the recognized object.</param>
        /// <param name="confidence">Confidence of most likely class.</param>
        /// <param name="classIndex">Index of most likely class.</param>
        public static YoloItem FromVersion8(Vector2 center, Vector2 size, float confidence, int classIndex)
        {
            return new YoloItem
            {
                Center = center,
                Size = size,
                Confidence = confidence,
                MostLikelyClass = (ObjectClass)classIndex,
                TopLeft = center - size / 2,
                BottomRight = center + size / 2
            };
        }

        /// <summary>
        ///     Stores the data from the output tensor.
        ///     Processes the output of the Yolov10.
        /// </summary>
        /// <param name="topLeft">Top left position of the recognized object.</param>
        /// <param name="bottomRight">Bottom right position of the recognized object.</param>
        /// <param name="confidence">Confidence of most likely class.</param>
        /// <param name="classIndex">Index of most likely class.</param>
        public static YoloItem FromVersion10(Vector2 topLeft, Vector2 bottomRight, float confidence, int classIndex)
        {
            YoloItem yoloItem = new()
            {
                TopLeft = topLeft,
                BottomRight = bottomRight,
                Size = bottomRight - topLeft,
                Confidence = confidence,
                MostLikelyClass = (ObjectClass)classIndex
            };
            yoloItem.Center = topLeft + yoloItem.Size / 2;

            return yoloItem;
        }

        /// <summary>
        ///     Center position of the recognized object.
        /// </summary>
        public Vector2 Center { get; private set; }

        /// <summary>
        ///     Size of the recognized object.
        /// </summary>
        public Vector2 Size { get; private set; }

        /// <summary>
        ///     Top left position of the recognized object.
        /// </summary>
        public Vector2 TopLeft { get; private set; }

        /// <summary>
        ///     Bottom right position of the recognized object.
        /// </summary>
        public Vector2 BottomRight { get; private set; }

        /// <summary>
        ///     Confidence that the bounding box contains the <see cref="MostLikelyClass"/>.
        /// </summary>
        public float Confidence { get; private set; }

        /// <summary>
        ///     Name of the most likely class for this object.
        /// </summary>
        public ObjectClass MostLikelyClass { get; private set; }
    }
}
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Represents a Yolo item in version v8.
    /// </summary>
    public readonly struct YoloItem
    {
        /// <summary>
        ///     Reads the data from the output tensor.
        /// </summary>
        /// <param name="center">Center position of the recognized object.</param>
        /// <param name="size">Size of the recognized object.</param>
        /// <param name="confidence">Confidence of most likely class.</param>
        /// <param name="classIndex">Index of most likely class.</param>
        public YoloItem(Vector2 center, Vector2 size, float confidence, int classIndex)
        {
            // read dimension data from tensor
            this.Center = center;
            this.Size = size;

            // get most likely class
            this.Confidence = confidence;
            this.MostLikelyClass = (ObjectClass)classIndex;

            // calculate bounding box positions
            this.TopLeft = this.Center - this.Size / 2;
            this.BottomRight = this.Center + this.Size / 2;
        }

        /// <summary>
        ///     Center position of the recognized object.
        /// </summary>
        public Vector2 Center { get; }

        /// <summary>
        ///     Size of the recognized object.
        /// </summary>
        public Vector2 Size { get; }

        /// <summary>
        ///     Top left position of the recognized object.
        /// </summary>
        public Vector2 TopLeft { get; }

        /// <summary>
        ///     Bottom right position of the recognized object.
        /// </summary>
        public Vector2 BottomRight { get; }

        /// <summary>
        ///     Confidence that the bounding box contains the <see cref="MostLikelyClass"/>.
        /// </summary>
        public float Confidence { get; }

        /// <summary>
        ///     Name of the most likely class for this object.
        /// </summary>
        public ObjectClass MostLikelyClass { get; }
    }
}
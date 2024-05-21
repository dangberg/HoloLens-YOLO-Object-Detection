using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Class that contains information about a recognized item.
    /// </summary>
    internal class DisplayedItem
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="DisplayedItem" /> class.
        /// </summary>
        /// <param name="yoloItem">Detected yolo item.</param>
        /// <param name="positionInSpace">Calculated position of the item in space.</param>
        public DisplayedItem(YoloItem yoloItem, Vector3 positionInSpace)
        {
            this.YoloItem = yoloItem;
            this.TimeLastSeen = Time.time;
            this.TimesSeen = 1;
            this.PositionInSpace = positionInSpace;
            this.IsInCameraView = true;
        }

        /// <summary>
        ///     The recognized item.
        /// </summary>
        public YoloItem YoloItem { get; private set; }

        /// <summary>
        ///     Position of the object in the world space.
        /// </summary>
        public Vector3 PositionInSpace { get; private set; }

        /// <summary>
        ///     The last time this item was recognized.
        /// </summary>
        public float TimeLastSeen { get; set; }

        /// <summary>
        ///     A counter how often this object has been seen.
        /// </summary>
        public int TimesSeen { get; private set; }

        /// <summary>
        ///     Tells whether the object is currently visible or not.
        /// </summary>
        public bool IsInCameraView { get; set; }

        /// <summary>
        ///     Tracking marker for the recognized item.
        /// </summary>
        public GameObject TrackingMarker { get; set; }

        /// <summary>
        ///     Updates the item with new information from the current frame.
        /// </summary>
        /// <param name="yoloItem">Detected yolo item.</param>
        /// <param name="positionInSpace">Calculated position of the item in space.</param>
        public void UpdateItem(YoloItem yoloItem, Vector3 positionInSpace)
        {
            this.YoloItem = yoloItem;
            this.TimeLastSeen = Time.time;
            this.TimesSeen++;
            this.PositionInSpace = positionInSpace;
            this.IsInCameraView = true;
        }
    }
}
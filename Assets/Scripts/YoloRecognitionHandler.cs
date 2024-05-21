using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Handles the recognitions of the yolo model.
    /// </summary>
    public class YoloRecognitionHandler : MonoBehaviour
    {
        private readonly List<DisplayedItem> yoloItems = new();

        [SerializeField]
        private GameObject labelObject;

        private YoloDebugOutput yoloDebugOutput;

        private void Start()
        {
            this.yoloDebugOutput = gameObject.GetComponent<YoloDebugOutput>();
        }

        /// <summary>
        ///     Post process the recognitions and show them.
        /// </summary>
        /// <param name="recognitions">Recognitions of the model.</param>
        /// <param name="cameraTransform">The current camera position.</param>
        public void ShowRecognitions(List<YoloItem> recognitions, CameraTransform cameraTransform)
        {
            this.AddNewlyRecognizedObjects(recognitions, cameraTransform);
            this.RemoveOutdatedObjects();
            this.TriggerDetectionActions();
        }

        private void AddNewlyRecognizedObjects(List<YoloItem> recognitions, CameraTransform cameraTransform)
        {
            List<DisplayedItem> unmatchedExistingItems = new(this.yoloItems);
            foreach (YoloItem newItem in recognitions)
            {
                // Calculate center point of object in space
                Vector3? positionInSpace = PositionCalculator.CalculatePointInSpace(newItem, cameraTransform);
                if (positionInSpace == null)
                {
                    continue;
                }

                // Create new item or update closest, existing item
                DisplayedItem item = this.GetClosestExistingItem(unmatchedExistingItems, newItem, positionInSpace.Value);
                if (item == null)
                {
                    item = new DisplayedItem(newItem, positionInSpace.Value);
                    this.yoloItems.Add(item);
                }
                else
                {
                    unmatchedExistingItems.Remove(item);
                    item.UpdateItem(newItem, positionInSpace.Value);
                }
            }
        }

        /// <summary>
        /// Checks if given item has been seen previously.
        /// If so, returns the closest item of the same class.
        /// </summary>
        /// <param name="oldItems">All previously recognized objects that are unmatched.</param>
        /// <param name="item">The newly recognized item.</param>
        /// <param name="positionInSpace">The position in space of the new item.</param>
        /// <returns>Closest existing item.</returns>
        private DisplayedItem GetClosestExistingItem(List<DisplayedItem> oldItems, YoloItem item, Vector3 positionInSpace)
        {
            DisplayedItem closestItem = null;
            float closestDist = float.MaxValue;

            // Find item of the same class that is closest to the new object, below a certain threshold
            foreach (DisplayedItem oldItem in oldItems)
            {
                if (!oldItem.YoloItem.MostLikelyClass.Equals(item.MostLikelyClass))
                {
                    continue;
                }

                float distance = Vector3.Distance(oldItem.PositionInSpace, positionInSpace);

                if (distance > Parameters.MaxIdenticalObject || distance >= closestDist)
                {
                    continue;
                }

                //Update closest element
                closestItem = oldItem;
                closestDist = distance;
            }

            return closestItem;
        }

        private void RemoveOutdatedObjects()
        {
            for (int i = this.yoloItems.Count - 1; i >= 0; i--)
            {
                bool wasInCameraView = this.yoloItems[i].IsInCameraView;
                bool isInCameraView = PositionCalculator.IsObjectInCameraView(this.yoloItems[i].PositionInSpace);
                this.yoloItems[i].IsInCameraView = isInCameraView;

                if (!isInCameraView)
                {
                    continue;
                }

                if (!wasInCameraView)
                {
                    // Reset time last seen, so that the object is not removed immediately when it is in the camera view again.
                    this.yoloItems[i].TimeLastSeen = Time.time;
                    continue;
                }

                // Remove object if it is not visible anymore for a certain time.
                if (Time.time - this.yoloItems[i].TimeLastSeen <= Parameters.ObjectTimeOut)
                {
                    continue;
                }

                Destroy(yoloItems[i].TrackingMarker);
                this.yoloItems.RemoveAt(i);
            }
        }

        private void TriggerDetectionActions()
        {
            // Only apply actions if item have been seen multiple times.
            foreach (DisplayedItem item in this.yoloItems.Where(item => item.IsInCameraView && item.TimesSeen >= Parameters.MinTimesSeen))
            {
                // Show marker
                this.ManageTrackingMarker(item);

                // Show debug information
                yoloDebugOutput.ShowDebugInformationForItem(item);
            }
        }

        /// <summary>
        ///     Create debug marker if it does not exist or move it to the correct position
        /// </summary>
        /// <param name="item">Item whose marker should be managed</param>
        private void ManageTrackingMarker(DisplayedItem item)
        {
            if (item.TrackingMarker == null)
            {
                item.TrackingMarker = Instantiate(this.labelObject, item.PositionInSpace, Quaternion.identity);
            }

            ObjectLabelController labelController = item.TrackingMarker.GetComponent<ObjectLabelController>();
            labelController.Text = $"{item.YoloItem.MostLikelyClass} ({Math.Round(item.YoloItem.Confidence * 100, 3)}%)";
            labelController.UpdatePosition(item.PositionInSpace);
   
        }
    }
}
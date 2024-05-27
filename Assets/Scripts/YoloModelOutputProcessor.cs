using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Sentis;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Model output processor for YOLO object detection.
    /// </summary>
    public class YoloModelOutputProcessor
    {
        /// <summary>
        ///     Processes the output tensor of the model.
        /// </summary>
        /// <param name="tensor">Output tensor of the model.</param>
        /// <param name="threshold">Threshold for the confidence level.</param>
        /// <returns>Parsed model output.</returns>
        public static List<YoloItem> ProcessModelOutput(TensorFloat tensor, float threshold)
        {
            List<YoloItem> boxesMeetingConfidenceLevel = Parameters.ModelVersion == YoloModelVersion.V8 ?
                ProcessModelOutputVersion8(tensor, threshold) :
                ProcessModelOutputVersion10(tensor, threshold);
            return FindMostLikelyObjects(boxesMeetingConfidenceLevel);
        }

        private static List<YoloItem> ProcessModelOutputVersion8(TensorFloat tensor, float threshold)
        {
            List<YoloItem> boxesMeetingConfidenceLevel = new();
            for (int boxIndex = 0; boxIndex < tensor.shape[2]; boxIndex++)
            {
                // get most likely class
                List<float> classProbabilities = new();
                for (int i = 4; i < tensor.shape[1]; i++)
                {
                    classProbabilities.Add(tensor[0, i, boxIndex]);
                }

                float confidence = classProbabilities.Max();
                if (confidence < threshold)
                {
                    continue;
                }

                // read dimension data from tensor
                Vector2 center = new(tensor[0, 0, boxIndex], tensor[0, 1, boxIndex]);
                Vector2 size = new(tensor[0, 2, boxIndex], tensor[0, 3, boxIndex]);
                int maxIndex = classProbabilities.IndexOf(confidence);
                boxesMeetingConfidenceLevel.Add(YoloItem.FromVersion8(center, size, confidence, maxIndex));
            }

            return boxesMeetingConfidenceLevel;
        }

        private static List<YoloItem> ProcessModelOutputVersion10(TensorFloat tensor, float threshold)
        {
            List<YoloItem> boxesMeetingConfidenceLevel = new();
            for (int boxIndex = 0; boxIndex < tensor.shape[1]; boxIndex++)
            {
                float confidence = tensor[0, boxIndex, 4];
                if (confidence < threshold)
                {
                    continue;
                }

                // read dimension data from tensor
                Vector2 topLeft = new(tensor[0, boxIndex, 0], tensor[0, boxIndex, 1]);
                Vector2 bottomRight = new(tensor[0, boxIndex, 2], tensor[0, boxIndex, 3]);
                int classIndex = (int)tensor[0, boxIndex, 5];
                boxesMeetingConfidenceLevel.Add(YoloItem.FromVersion10(topLeft, bottomRight, confidence, classIndex));
            }
            return boxesMeetingConfidenceLevel;
        }

        private static List<YoloItem> FindMostLikelyObjects(IEnumerable<YoloItem> boxesMeetingConfidenceLevel)
        {
            List<YoloItem> result = new();
            IEnumerable<IGrouping<ObjectClass, YoloItem>> groupedItems = boxesMeetingConfidenceLevel.GroupBy(b => b.MostLikelyClass);
            foreach (IGrouping<ObjectClass, YoloItem> yoloItems in groupedItems)
            {
                result.AddRange(RemoveOverlappingBoxes(yoloItems.ToList()));
            }

            return result;
        }

        private static IEnumerable<YoloItem> RemoveOverlappingBoxes(List<YoloItem> boxesMeetingConfidenceLevel)
        {
            // sort the boxesMeetingsConfidenceLevel by their confidence score in descending order  
            boxesMeetingConfidenceLevel.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
            List<YoloItem> selectedBoxes = new();

            // loop through each box and check for overlap with higher-confidence boxesMeetingsConfidenceLevel  
            while (boxesMeetingConfidenceLevel.Count > 0)
            {
                YoloItem currentBox = boxesMeetingConfidenceLevel[0];
                selectedBoxes.Add(currentBox);
                boxesMeetingConfidenceLevel.RemoveAt(0);

                // compare the current box with all remaining boxesMeetingsConfidenceLevel  
                for (int i = 0; i < boxesMeetingConfidenceLevel.Count; i++)
                {
                    YoloItem otherBox = boxesMeetingConfidenceLevel[i];
                    float overlap = ComputeIoU(currentBox, otherBox);
                    if (overlap > Parameters.OverlapThreshold)
                    {
                        // remove the box if it has a high overlap with the current box  
                        boxesMeetingConfidenceLevel.RemoveAt(i);
                        i--;
                    }
                }
            }

            return selectedBoxes;
        }

        private static float ComputeIoU(YoloItem boxA, YoloItem boxB)
        {
            float xA = Math.Max(boxA.TopLeft.x, boxB.TopLeft.x);
            float yA = Math.Max(boxA.TopLeft.y, boxB.TopLeft.y);
            float xB = Math.Min(boxA.BottomRight.x, boxB.BottomRight.x);
            float yB = Math.Min(boxA.BottomRight.y, boxB.BottomRight.y);

            float intersectionArea = Math.Max(0, xB - xA) * Math.Max(0, yB - yA);
            float boxAArea = boxA.Size.x * boxA.Size.y;
            float boxBArea = boxB.Size.x * boxB.Size.y;
            float unionArea = boxAArea + boxBArea - intersectionArea;

            return intersectionArea / unionArea;
        }
    }
}
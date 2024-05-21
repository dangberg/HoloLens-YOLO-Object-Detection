using MixedReality.Toolkit.UX;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Script managing the advanced settings menu.
    /// </summary>
    public class AdvancedSettings : MonoBehaviour
    {
        [SerializeField]
        private PressableButton[] performancePresets, recognitionThreshold;

        [SerializeField]
        private PressableButton boundingBoxButton, debugImageButton, projectionCubesButton, rayCastButton;

        private static SettingsProvider SettingsProvider => SettingsProvider.Instance;

        private void Start()
        {
            // Set the buttons to the current settings
            switch (SettingsProvider.ModelExecutionOR)
            {
                case ModelExecutionMode.High:
                    this.performancePresets[0].ForceSetToggled(true);
                    break;
                case ModelExecutionMode.Low:
                    this.performancePresets[1].ForceSetToggled(true);
                    break;
                case ModelExecutionMode.Full:
                    this.performancePresets[2].ForceSetToggled(true);
                    break;
            }

            switch (SettingsProvider.ThresholdOR)
            {
                case RecognitionThreshold.Low:
                    this.recognitionThreshold[0].ForceSetToggled(true);
                    break;
                case RecognitionThreshold.Medium:
                    this.recognitionThreshold[1].ForceSetToggled(true);
                    break;
                case RecognitionThreshold.High:
                    this.recognitionThreshold[2].ForceSetToggled(true);
                    break;
            }
            this.boundingBoxButton.ForceSetToggled(SettingsProvider.DebugBoundingBoxes);
            this.debugImageButton.ForceSetToggled(SettingsProvider.DebugImage);
            this.projectionCubesButton.ForceSetToggled(SettingsProvider.DebugCubes);
            this.rayCastButton.ForceSetToggled(SettingsProvider.DebugSphereCast);

        }

        /// <summary>
        ///     Toggles the buttons in the performance preset menu and sets the performance mode.
        /// </summary>
        /// <param name="index">index of the button</param>
        public void SetPerformancePreset(int index)
        {
            for (int i = 0; i < this.performancePresets.Length; i++)
            {
                this.performancePresets[i].ForceSetToggled(i == index);
            }

            SettingsProvider.ModelExecutionOR = index switch
            {
                0 => ModelExecutionMode.High,
                1 => ModelExecutionMode.Low,
                2 => ModelExecutionMode.Full,
                _ => SettingsProvider.ModelExecutionOR
            };
        }

        /// <summary>
        ///     Toggles the buttons in the recognition threshold menu and sets the recognition threshold.
        /// </summary>
        /// <param name="index">index of the button</param>
        public void SetRecognitionThreshold(int index)
        {
            for (int i = 0; i < this.recognitionThreshold.Length; i++)
            {
                this.recognitionThreshold[i].ForceSetToggled(i == index);
            }

            SettingsProvider.ThresholdOR = index switch
            {
                0 => RecognitionThreshold.Low,
                1 => RecognitionThreshold.Medium,
                2 => RecognitionThreshold.High,
                _ => SettingsProvider.ThresholdOR
            };
        }

        /// <summary>
        ///     Toggles the bounding boxes.
        /// </summary>
        public void ToggleBoundingBoxes()
        {
            SettingsProvider.DebugBoundingBoxes = !SettingsProvider.DebugBoundingBoxes;
        }

        /// <summary>
        ///     Toggles the debug image.
        /// </summary>
        public void ToggleDebugImage()
        {
            SettingsProvider.DebugImage = !SettingsProvider.DebugImage;
        }

        /// <summary>
        ///    Toggles the projection cubes.
        /// </summary>
        public void ToggleProjectionCubes()
        {
            SettingsProvider.DebugCubes = !SettingsProvider.DebugCubes;
        }

        /// <summary>
        ///     Toggles the sphere cast.
        /// </summary>
        public void ToggleSphereCast()
        {
            SettingsProvider.DebugSphereCast = !SettingsProvider.DebugSphereCast;
        }
    }
}
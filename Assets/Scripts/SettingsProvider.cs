using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Assets.Scripts
{
    /// <summary>
    ///     Singleton that manages the current settings of the app.
    /// </summary>
    public class SettingsProvider : INotifyPropertyChanged
    {
        /// <summary>
        ///     Singleton instance of <see cref="SettingsProvider"/>.
        /// </summary>
        public static SettingsProvider Instance = new();

        private bool debugImage;

        private bool debugBoundingBoxes;

        private bool debugCubes;

        private bool debugSphereCast;

        private ModelExecutionMode modelExecutionOR;

        private RecognitionThreshold thresholdOR;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SettingsProvider" /> with default settings.
        /// </summary>
        public SettingsProvider()
        {
            // Set default settings
            this.DebugImage = false;
            this.DebugBoundingBoxes = false;
            this.DebugCubes = false;
            this.DebugSphereCast = false;
#if UNITY_EDITOR
            this.ModelExecutionOR = ModelExecutionMode.Full;
#else
            this.ModelExecutionOR = ModelExecutionMode.Low;
#endif
            this.ThresholdOR = RecognitionThreshold.Medium;
        }

        /// <summary>
        ///     Certainty threshold for the OR model.
        /// </summary>
        public RecognitionThreshold ThresholdOR
        {
            get => this.thresholdOR;
            set => this.SetField(ref this.thresholdOR, value);
        }

        /// <summary>
        ///     Whether the debug image is active.
        /// </summary>
        public bool DebugImage
        {
            get => this.debugImage;
            set => this.SetField(ref this.debugImage, value);
        }

        /// <summary>
        ///     Whether the debug bounding boxes are displayed.
        /// </summary>
        public bool DebugBoundingBoxes
        {
            get => this.debugBoundingBoxes;
            set => this.SetField(ref this.debugBoundingBoxes, value);
        }

        /// <summary>
        ///     Whether the debug cubes should be rendered.
        /// </summary>
        public bool DebugCubes
        {
            get => this.debugCubes;
            set => this.SetField(ref this.debugCubes, value);
        }

        /// <summary>
        ///     Whether the debug ray casts should be displayed.
        /// </summary>
        public bool DebugSphereCast
        {
            get => this.debugSphereCast;
            set => this.SetField(ref this.debugSphereCast, value);
        }

        /// <summary>
        ///     Number of model layers executed in the OR model every frame.
        /// </summary>
        public ModelExecutionMode ModelExecutionOR
        {
            get => this.modelExecutionOR;
            set => this.SetField(ref this.modelExecutionOR, value);
        }

        /// <inheritdoc />
        public event PropertyChangedEventHandler PropertyChanged;

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            this.OnPropertyChanged(propertyName);
          }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    ///     An enum representing the categories for thresholds for the Object Recognition.
    /// </summary>
    public enum RecognitionThreshold
    {
        Low,
        Medium,
        High
    }

    /// <summary>
    ///     An enum representing the number of layer executions for the Object Recognition.
    /// </summary>
    public enum ModelExecutionMode
    {
        Low,
        High,
        Full
    }
}
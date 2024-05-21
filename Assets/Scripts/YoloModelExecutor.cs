using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Sentis;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    ///     Model executor using a YOLO object detection model.
    /// </summary>
    public class YoloModelExecutor : MonoBehaviour
    {
        /// <summary>
        ///     Object detection model that should be executed.
        /// </summary>
        public ModelAsset ModelAsset;

        /// <summary>
        ///     Material with shader that is used for scaling the input image to the correct aspect ratio.
        /// </summary>
        public Material ShaderForScaling;

        private TextureTransform textureTransform;

        private bool disposed;

        private IWorker worker;

        private TensorFloat inputTensor;

        private TensorFloat outputTensor;

        private ModelState modelState = ModelState.PreProcessing;

        private CameraTransform cameraTransform;

        private bool hasMoreModelToRun = true;

        private IEnumerator modelEnumerator;

        private RenderTexture intermediateRenderTexture;

        private YoloDebugOutput yoloDebugOutput;

        private int layerCount;

        private float threshold;

        private YoloRecognitionHandler yoloRecognitionHandler;

        private static WebCamTextureAccess WebCamTextureAccess => WebCamTextureAccess.Instance;

        private static SettingsProvider SettingsProvider => SettingsProvider.Instance;

        private void Start()
        {
            // Get other components
            this.yoloDebugOutput = gameObject.GetComponent<YoloDebugOutput>();
            this.yoloRecognitionHandler = gameObject.GetComponent<YoloRecognitionHandler>();

            // Initialize settings
            this.SettingsProviderOnPropertyChanged(null, new PropertyChangedEventArgs(nameof(SettingsProvider.ModelExecutionOR)));
            this.SettingsProviderOnPropertyChanged(null, new PropertyChangedEventArgs(nameof(SettingsProvider.ThresholdOR)));
            SettingsProvider.PropertyChanged += this.SettingsProviderOnPropertyChanged;

            // Load the model from the provided NNModel asset
            Model model = ModelLoader.Load(this.ModelAsset);
             
            // Create a Barracuda worker to run the model on the GPU
            this.worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);

            // Initialize model input
            WebCamTextureAccess.Play();
            this.intermediateRenderTexture = new RenderTexture(Parameters.ModelImageResolution.x, Parameters.ModelImageResolution.y, 24);
            this.ShaderForScaling.SetFloat("_Aspect",
                (float)WebCamTextureAccess.ActualCameraSize.x / WebCamTextureAccess.ActualCameraSize.y * Parameters.ModelImageResolution.y / Parameters.ModelImageResolution.x);
            this.textureTransform = new TextureTransform().SetDimensions(Parameters.ModelImageResolution.x, Parameters.ModelImageResolution.y, 3);
        }

        private void Update()
        {
            switch (this.modelState)
            {
                case ModelState.Idle:
                    break;
                case ModelState.PreProcessing:
                    this.inputTensor?.Dispose();
                    this.cameraTransform = new CameraTransform(Camera.main);

                    Graphics.Blit(WebCamTextureAccess.WebCamTexture, this.intermediateRenderTexture, this.ShaderForScaling);
                    this.inputTensor = TextureConverter.ToTensor(this.intermediateRenderTexture, this.textureTransform);

                    this.modelState = ModelState.Executing;
                    break;
                case ModelState.Executing:
                    this.modelEnumerator ??= this.worker.StartManualSchedule(this.inputTensor);

                    int i = 0;
                    while (i++ < this.layerCount && this.hasMoreModelToRun)
                    {
                        this.hasMoreModelToRun = this.modelEnumerator.MoveNext();
                    }

                    if (!this.hasMoreModelToRun)
                    {
                        // reset model states
                        this.modelEnumerator = null;
                        this.hasMoreModelToRun = true;
                        this.modelState = ModelState.ReadOutput;
                    }

                    break;
                case ModelState.ReadOutput:
                    this.outputTensor = (TensorFloat)this.worker.PeekOutput();
                    this.modelState = ModelState.Idle;
                    this.outputTensor.AsyncReadbackRequest(_ => this.modelState = ModelState.PostProcessing);
                    break;
                case ModelState.PostProcessing:
                    this.outputTensor.MakeReadable();
                    List<YoloItem> result = YoloModelOutputProcessor.ProcessModelOutput(this.outputTensor, this.threshold);
                    this.yoloDebugOutput.ShowDebugInformation(this.inputTensor, result, this.cameraTransform);
                    yoloRecognitionHandler.ShowRecognitions(result, this.cameraTransform);
                    this.modelState = ModelState.PreProcessing;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Method that is called when the object is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            SettingsProvider.PropertyChanged -= this.SettingsProviderOnPropertyChanged;
            WebCamTextureAccess.Stop();
            this.inputTensor?.Dispose();
            this.worker?.Dispose();
        }

        private void SettingsProviderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsProvider.ModelExecutionOR):
                    this.UpdateModelPerformance();
                    break;
                case nameof(SettingsProvider.ThresholdOR):
                    this.UpdateThreshold();
                    break;
            }
        }

        private void UpdateModelPerformance()
        {
            this.layerCount = SettingsProvider.ModelExecutionOR switch
            {
                ModelExecutionMode.High => Parameters.LayersHigh,
                ModelExecutionMode.Low => Parameters.LayersLow,
                ModelExecutionMode.Full => int.MaxValue,
                _ => this.layerCount
            };
        }

        private void UpdateThreshold()
        {
            this.threshold = SettingsProvider.ThresholdOR switch
            {
                RecognitionThreshold.High => Parameters.ThresholdHigh,
                RecognitionThreshold.Medium => Parameters.ThresholdMedium,
                RecognitionThreshold.Low => Parameters.ThresholdLow,
                _ => this.threshold
            };
        }
    }
}
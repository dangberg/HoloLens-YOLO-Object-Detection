using System;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Sentis;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Scripts
{
    /// <summary>
    ///     Shows the debug output of the yolo model.
    /// </summary>
    public class YoloDebugOutput : MonoBehaviour
    {
        private static readonly Color[] CubeColors =
        {
            Color.green,
            Color.blue,
            Color.red,
            Color.yellow,
            Color.gray,
            Color.magenta,
            Color.gray,
            Color.yellow,
            Color.red,
            Color.blue,
            Color.green
        };

        private Texture2D outputTexture;

        private RenderTexture intermediateTexture;

        private GameObject quad;

        private GameObject[] cubes;

        private bool updatedImage;

        private readonly List<GameObject> lineRendererObjects = new();

        [SerializeField]
        private Material lineRendererMaterial;

        private CameraTransform cameraTransform;

        private static SettingsProvider SettingsProvider => SettingsProvider.Instance;

        public void Start()
        {
            // Initialize settings
            this.SettingsProviderOnPropertyChanged(null, new PropertyChangedEventArgs(nameof(SettingsProvider.DebugCubes)));
            this.SettingsProviderOnPropertyChanged(null, new PropertyChangedEventArgs(nameof(SettingsProvider.DebugImage)));
            SettingsProvider.PropertyChanged += SettingsProviderOnPropertyChanged;
        }

        public void LateUpdate()
        {
            if (!SettingsProvider.DebugImage)
            {
                return;
            }

            this.quad.transform.SetPositionAndRotation(Camera.main.transform.position + 2 * Camera.main.transform.forward, Camera.main.transform.rotation);

            if (updatedImage)
            {
                this.outputTexture.Apply();
                updatedImage = false;
            }
        }

        private void SettingsProviderOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsProvider.DebugCubes))
            {
                if (SettingsProvider.DebugCubes)
                {
                    this.CreateCubes();
                }
                else
                {
                    this.DestroyCubes();
                }
            }
            else if (e.PropertyName == nameof(SettingsProvider.DebugImage))
            {
                if (SettingsProvider.DebugImage)
                {
                    this.InitDebugImageOutput();
                }
                else
                {
                    this.DestroyDebugImageOutput();
                }
            }
        }

        private void InitDebugImageOutput()
        {
            // Initialize textures
            this.intermediateTexture = new RenderTexture(Parameters.ModelImageResolution.x, Parameters.ModelImageResolution.y, 24);
            this.outputTexture = new Texture2D(Parameters.ModelImageResolution.x, Parameters.ModelImageResolution.y, TextureFormat.BGRA32, false);
            this.quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            this.quad.transform.localScale = new Vector3(1f, (float)Parameters.ModelImageResolution.y / Parameters.ModelImageResolution.x, 1);
            Renderer quadRenderer = this.quad.GetComponent<Renderer>();
            quadRenderer.material.mainTexture = this.outputTexture;
        }

        private void DestroyDebugImageOutput()
        {
            if (this.quad != null)
            {
                Object.DestroyImmediate(this.quad);
            }

            if (this.outputTexture != null)
            {
                Object.DestroyImmediate(this.outputTexture);
            }

            if (this.intermediateTexture != null)
            {
                Object.DestroyImmediate(this.intermediateTexture);
            }
        }

        private void CreateCubes()
        {
            this.cubes = new GameObject[121];
            for (int i = 0; i < 121; i++)
            {
                this.cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                this.cubes[i].transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                this.cubes[i].GetComponent<Renderer>().material.SetColor("_Color", (i - 5) % 11 == 0 ? Color.cyan : CubeColors[i / 11]);
            }
        }

        private void DestroyCubes()
        {
            if (this.cubes == null)
            {
                return;
            }

            foreach (GameObject cube in this.cubes)
            {
                Object.DestroyImmediate(cube);
            }

            this.cubes = null;
        }

        /// <summary>
        /// Shows debug information for the processed item.
        /// </summary>
        /// <param name="item">Processed item.</param>
        internal void ShowDebugInformationForItem(DisplayedItem item)
        {
            if (SettingsProvider.DebugBoundingBoxes)
            {
                // Draw bounding box in world space
                Vector3[] cornerPositions = PositionCalculator.CalculateCornerPoints(item.YoloItem, this.cameraTransform);
                LineRenderer lineRenderer = this.CreateLineRenderer();
                lineRenderer.positionCount = 5;
                for (int i = 0; i < 4; i++)
                {
                    lineRenderer.SetPosition(i, cornerPositions[i]);
                }
                lineRenderer.SetPosition(4, cornerPositions[0]);
            }

            if (SettingsProvider.DebugSphereCast)
            {
                // Visualize ray cast
                LineRenderer lineRenderer = this.CreateLineRenderer();
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, this.cameraTransform.Position + Parameters.SphereCastOffset * this.cameraTransform.Up);
                lineRenderer.SetPosition(1, item.PositionInSpace);

            }
        }

        private LineRenderer CreateLineRenderer()
        {
            GameObject o = new();
            this.lineRendererObjects.Add(o);
            LineRenderer lineRenderer = o.AddComponent<LineRenderer>();
            lineRenderer.material = this.lineRendererMaterial;
            lineRenderer.startWidth = 0.005f;
            return lineRenderer;
        }

        /// <summary>
        ///     Shows debug information for the yolo model input.
        /// </summary>
        /// <param name="inputTensor">Input tensor of the model execution.</param>
        /// <param name="detections">Detections of the model inside the input tensor.</param>
        /// <param name="cameraPosition">The current camera position.</param>
        internal void ShowDebugInformation(TensorFloat inputTensor, List<YoloItem> detections, CameraTransform cameraPosition)
        {
            this.cameraTransform = cameraPosition;

            if (SettingsProvider.DebugImage)
            {
                TextureConverter.RenderToTexture(inputTensor, this.intermediateTexture,
                    new TextureTransform().SetCoordOrigin(CoordOrigin.TopLeft));
                this.intermediateTexture.ToTexture2D(this.outputTexture);

                foreach (YoloItem yoloItem in detections)
                {
                    // Draw bounding box in image
                    int x1 = (int)yoloItem.TopLeft.x;
                    int x2 = (int)yoloItem.BottomRight.x;
                    int y1 = (int)(Parameters.ModelImageResolution.y - yoloItem.BottomRight.y);
                    int y2 = (int)(Parameters.ModelImageResolution.y - yoloItem.TopLeft.y);

                    for (int x = x1; x <= x2; x++)
                    {
                        this.outputTexture.SetPixel(x, y1, Color.red);
                        this.outputTexture.SetPixel(x, y2, Color.red);
                    }

                    for (int y = y1; y <= y2; y++)
                    {
                        this.outputTexture.SetPixel(x1, y, Color.red);
                        this.outputTexture.SetPixel(x2, y, Color.red);
                    }
                }

                updatedImage = true;
            }
            
            if (SettingsProvider.DebugCubes)
            {
                int counter = 0;
                for (float i = -0.5f; i <= 0.5f; i += 0.1f)
                {
                    for (float j = -0.5f; j <= 0.5f; j += 0.1f)
                    {
                        Vector3 pos = PositionCalculator.GetPositionInSpace(cameraPosition, new Vector2(j, i));
                        this.cubes[counter].transform.position = pos;
                        counter++;
                    }
                }
            }

            if (SettingsProvider.DebugImage && SettingsProvider.DebugCubes)
            {
                int colorIndex = 0;
                for (float j = 0f; j <= Parameters.ModelImageResolution.y; j += Parameters.ModelImageResolution.y / 10.0f)
                {
                    for (float i = 0; i <= Parameters.ModelImageResolution.x; i += Parameters.ModelImageResolution.x / 10.0f)
                    {
                        Color color = Math.Abs(i - Parameters.ModelImageResolution.x / 2.0f) < 0.01 ? Color.cyan : CubeColors[colorIndex];

                        this.outputTexture.SetPixel((int)i - 1, (int)j, color);
                        this.outputTexture.SetPixel((int)i, (int)j, color);
                        this.outputTexture.SetPixel((int)i + 1, (int)j, color);
                        this.outputTexture.SetPixel((int)i - 1, (int)j - 1, color);
                        this.outputTexture.SetPixel((int)i, (int)j - 1, color);
                        this.outputTexture.SetPixel((int)i + 1, (int)j - 1, color);
                        this.outputTexture.SetPixel((int)i - 1, (int)j + 1, color);
                        this.outputTexture.SetPixel((int)i, (int)j + 1, color);
                        this.outputTexture.SetPixel((int)i + 1, (int)j + 1, color);
                    }

                    colorIndex++;
                }
            }

            // Reset line renderer
            foreach (GameObject boundingBox in this.lineRendererObjects)
            {
                Object.DestroyImmediate(boundingBox);
            }
            this.lineRendererObjects.Clear();
        }
    }
}
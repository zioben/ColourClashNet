using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using ColourClashProcessing.Blob;
using ModuleTester;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ColourClashNet.Components
{
    /// <summary>
    /// Designer component that drives the processing pipeline of the UI.
    /// <para/>
    /// Pipeline: source image, background removal, quantization (pre-processing, cached until <see cref="InvalidatePreProcess"/>),
    /// then the selected color transformation. Every stage is kept both as <see cref="ImageData"/> and as GDI+ image for display.
    /// </summary>
    public partial class ColorTransformManager : Component
    {

        static string sClass = nameof(ColorTransformManager);

        /// <summary>
        /// Creates the component.
        /// </summary>
        public ColorTransformManager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates the component, adds it to <paramref name="container"/> and resets it.
        /// </summary>
        public ColorTransformManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();

            Reset();
        }



        #region Properties


        /// <summary>
        /// Settings applied to every stage.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public ColorTransformConfig TransformConfig { get; set; } = new ColorTransformConfig();


        ColorTransformInterface transformSource;
        ColorTransformInterface transformBkgRemover;
        ColorTransformInterface transformQuantizer;
        ColorTransformInterface transformProcessing;

        /// <summary>
        /// Source image.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataSourceX { get; set; }

        /// <summary>
        /// Image after background removal.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataBkgRemoved { get; set; }

        /// <summary>
        /// Image after quantization.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataQuantized { get; set; }

        /// <summary>
        /// Final processed image.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(null)]
        public ImageData DataProcessed { get; set; }

        /// <summary>
        /// Display bitmap of <see cref="DataSourceX"/>.
        /// </summary>
        [DefaultValue(null)]
        public Image ImageSource { get; protected set; }

        /// <summary>
        /// Display bitmap of <see cref="DataBkgRemoved"/>.
        /// </summary>
        [DefaultValue(null)]
        public Image ImageBkgRemoved { get; protected set; }

        /// <summary>
        /// Display bitmap of <see cref="DataQuantized"/>.
        /// </summary>
        [DefaultValue(null)]
        public Image ImageQuantized { get; protected set; }

        /// <summary>
        /// Display bitmap of <see cref="DataProcessed"/>.
        /// </summary>
        [DefaultValue(null)]
        public Image ImageProcessed { get; protected set; }

        /// <summary>
        /// Number of colors of the source image.
        /// </summary>
        public int ImageSourceColors => transformSource?.ImageOutput?.Colors ?? 0;
        /// <summary>
        /// Number of colors after background removal.
        /// </summary>
        public int ImageBkgRemovedColors => transformBkgRemover?.ImageOutput?.Colors ?? 0;
        /// <summary>
        /// Number of colors after quantization.
        /// </summary>
        public int ImageQuantizedColors => transformQuantizer?.ImageOutput?.Colors ?? 0;
        /// <summary>
        /// Number of colors of the processed image.
        /// </summary>
        public int ImageProcessedColors => transformProcessing?.ImageOutput?.Colors ?? 0;

        /// <summary>
        /// When true the pre-processing is run again before the next transformation.
        /// </summary>
        [Browsable(false)]
        [DefaultValue(true)]
        public bool InvalidatePreProcess { get; set; } = true;


        #endregion

        #region Events

        /// <summary>
        /// Raised after a source image is loaded.
        /// </summary>
        public event EventHandler OnCreate;
        /// <summary>
        /// Raised after a reset.
        /// </summary>
        public event EventHandler OnReset;
        /// <summary>
        /// Raised before a transformation starts.
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnPreProcess;
        /// <summary>
        /// Raised after background removal (reserved).
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnBkgRemoved;
        /// <summary>
        /// Raised after the pre-processing (background removal and quantization).
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnQuantize;
        /// <summary>
        /// Raised after a transformation completes.
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnProcess;

        #endregion


        /// <summary>
        /// Clears configuration, images and transformations.
        /// </summary>
        public void Reset()
        {
            string sMethod = nameof(Reset);
            try
            {
                TransformConfig = new ColorTransformConfig();
                transformSource = new ColorTransformIdentity();
                transformBkgRemover = new ColorTransformBkgRemover();
                transformQuantizer = new ColorTransformQuantization();
                transformProcessing = null;
                //
                DataSourceX = null;
                ImageSource?.Dispose();
                ImageSource = null;
                //
                ResetProcessingData();
                OnReset?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
            }
        }

        /// <summary>
        /// Clears the pre-processed and processed images.
        /// </summary>
        void ResetProcessingData()
        {
            InvalidatePreProcess = true;
            //
            DataBkgRemoved = null;
            DataQuantized = null;
            DataProcessed = null;
            //
            ImageBkgRemoved?.Dispose();
            ImageBkgRemoved = null;
            ImageQuantized?.Dispose();
            ImageQuantized = null;
            ImageProcessed?.Dispose();
            ImageProcessed = null;
            //
        }


        /// <summary>
        /// Loads a source color matrix and runs the pre-processing.
        /// </summary>
        /// <returns>False when the data is null or invalid.</returns>
        public bool Create(int[,] data)
        {
            string sMethod = nameof(Create);
            Reset();
            if (data == null)
            {
                LogMan.Error(sClass, sMethod, "No source data");
                return false;
            }
            try
            {
                DataSourceX = new ImageData().Create(data);
                ImageSource = ImageToolsGDI.ImageDataToGdiImage(DataSourceX);
                OnCreate?.Invoke(this, EventArgs.Empty);
                PreProcess();
                return true;
            }
            catch (Exception ex)
            {
                Reset();
                return false;
            }
        }
        /// <summary>
        /// Loads a source image and runs the pre-processing.
        /// </summary>
        public bool Create(ImageData imageData)
            => Create(imageData?.GetMatrix());

        /// <summary>
        /// Runs background removal and quantization.
        /// </summary>
        /// <returns>Always true.</returns>
        public bool PreProcess()
        {
            RemoveBkgAndQuantize(true);
            InvalidatePreProcess = false;
            return true;
        }

        /// <summary>
        /// Loads a GDI+ image and runs the pre-processing.
        /// </summary>
        public void Create(System.Drawing.Image image) => Create(ImageToolsGDI.GdiImageToMatrix(image as Bitmap));


        /// <summary>
        /// Runs background removal and quantization (without dithering); the quantized image becomes the current output.
        /// </summary>
        /// <param name="raiseEvent">Raises <see cref="OnQuantize"/> at the end</param>
        /// <returns>The quantized image, or null on error.</returns>
        ImageData RemoveBkgAndQuantize(bool raiseEvent)
        {
            string sMethod = nameof(RemoveBkgAndQuantize);
            if (DataSourceX == null)
            {
                LogMan.Error(sClass, sMethod, "No source data");
                InvalidatePreProcess = true;
                return null;
            }
            try
            {
                ResetProcessingData();
                LogMan.Message(sClass, sMethod, "Starting Processing");
                LogMan.Trace(sClass, sMethod, "Process Identity Transformation");
                transformSource.CreateAndProcessColors(DataSourceX);

                CancellationTokenSource cts = new CancellationTokenSource();

                // Pre-processing steps must not dither: dithering is applied only by the final transformation
                var noDitherConfig = TransformConfig.Clone().WithDithering(ColorDithering.None, 1, ColorDitheringFx.None);

                LogMan.Trace(sClass, sMethod, "Process Bkg Remover");
                transformBkgRemover
                    .SetProperties(noDitherConfig)
                    .Create(DataSourceX);
                var bkgRemovedResult = transformBkgRemover.ProcessColors(cts.Token);
                if (!bkgRemovedResult.IsSuccess)
                {
                    // Error results may carry a partial image: never use it as a pipeline stage
                    LogMan.Error(sClass, sMethod, $"Background removal failed : {bkgRemovedResult.Message}");
                    InvalidatePreProcess = true;
                    return null;
                }
                DataBkgRemoved = bkgRemovedResult.DataOut;
                ImageBkgRemoved = ImageToolsGDI.ImageDataToGdiImage(DataBkgRemoved);

                LogMan.Trace(sClass, sMethod, "Process Quantizer");
                transformQuantizer
                    .SetProperties(noDitherConfig)
                    .Create(DataBkgRemoved);

                var quantizedResult = transformQuantizer.ProcessColors(cts.Token);
                if (!quantizedResult.IsSuccess)
                {
                    LogMan.Error(sClass, sMethod, $"Quantization failed : {quantizedResult.Message}");
                    InvalidatePreProcess = true;
                    return null;
                }
                DataQuantized = quantizedResult.DataOut;
                ImageQuantized = ImageToolsGDI.ImageDataToGdiImage(DataQuantized);

                LogMan.Trace(sClass, sMethod, "Cloning Quantizer Output");
                DataProcessed = new ImageData().Create(DataQuantized);
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                transformProcessing = transformQuantizer;

                LogMan.Trace(sClass, sMethod, "Calling Event");
                if (raiseEvent)
                {
                    OnQuantize?.Invoke(this, new ColorManagerProcessEventArgs
                    {
                        DataDest = DataProcessed,
                        DataSource = DataSourceX,
                        Transformation = transformQuantizer
                    });
                }
                LogMan.Message(sClass, sMethod, "Process End");
                return DataProcessed;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                InvalidatePreProcess = true;
                return null;
            }
        }


        /// <summary>
        /// Runs the transformation <paramref name="transformType"/> on the quantized image, showing a progress form.
        /// </summary>
        /// <returns>The processed image, or null on error.</returns>
        public ImageData ProcessColors(ColorTransformType transformType)
        {
            string sMethod = nameof(ProcessColors);
            try
            {
                OnPreProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = transformSource
                });

                if (InvalidatePreProcess)
                {
                    PreProcess();
                }
                transformProcessing = ColorTransformBase.CreateColorTransformInterface(transformType, TransformConfig);
                CancellationTokenSource cts = new CancellationTokenSource();
                ProcessingForm.CreateProcessingForm(transformProcessing, cts);
                transformProcessing.Create(DataQuantized);
                var processedResult = transformProcessing.ProcessColors(cts.Token);
                if (!processedResult.IsSuccess)
                {
                    LogMan.Error(sClass, sMethod, $"{transformType} failed : {processedResult.Message}");
                    DataProcessed = null;
                    ImageProcessed = null;
                    return null;
                }
                DataProcessed = processedResult.DataOut;
                ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
                OnProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = DataProcessed,
                    DataSource = DataSourceX,
                    Transformation = transformProcessing,
                });
            }
            catch (Exception ex)
            {
                LogMan.Exception(sClass, sMethod, ex);
                DataProcessed = null;
                ImageProcessed = null;
            }
            return DataProcessed;
        }

        /// <summary>
        /// Runs <see cref="ProcessColors"/> on a worker thread.
        /// </summary>
        public async Task<ImageData> ProcessColorsAsync(ColorTransformType transformType)
            => await Task.Run(() => ProcessColors(transformType));

        /// <summary>
        /// Replaces the processed image with an externally computed one (e.g. a partial result).
        /// </summary>
        internal void InjectOutput(ImageData dataOut)
        {
            DataProcessed = dataOut;
            ImageProcessed = ImageToolsGDI.ImageDataToGdiImage(DataProcessed);
        }
    }
}
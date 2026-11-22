using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Drawing;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using ColourClashProcessing.Blob;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text;

namespace ColourClashNet.Components
{
    /// <summary>
    /// Designer component that runs the blob detection on an image and produces the stacked blob image.
    /// </summary>
    public partial class BlobManager : Component
    {

        static string sC = nameof(BlobManager);

        /// <summary>
        /// Blob detection settings, applied to <see cref="BlobAnalysis"/> at every run.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public BlobOptions BlobOptions { get; set; } = new BlobOptions();

        /// <summary>
        /// Blob detector (results of the last run).
        /// </summary>
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Blob BlobAnalysis { get; set; } = new Blob();


        /// <summary>
        /// Raised when the manager is created (reserved).
        /// </summary>
        public event EventHandler OnCreate;
        /// <summary>
        /// Raised when the manager is reset (reserved).
        /// </summary>
        public event EventHandler OnReset;
        /// <summary>
        /// Raised before the blob detection.
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnPreProcess;
        /// <summary>
        /// Raised after background removal (reserved).
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnBkgRemoved;
        /// <summary>
        /// Raised after quantization (reserved).
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnQuantize;
        /// <summary>
        /// Raised after the blob detection, with the output image.
        /// </summary>
        public event EventHandler<ColorManagerProcessEventArgs> OnProcess;


        /// <summary>
        /// Creates the component.
        /// </summary>
        public BlobManager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates the component and adds it to <paramref name="container"/>.
        /// </summary>
        public BlobManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }




       

        /// <summary>
        /// Detects the blobs of <paramref name="sourceImage"/> and stacks them vertically in a new image.
        /// </summary>
        /// <returns>The stacked blob image, or null on error.</returns>
        public ImageData ProcessBlob(ImageData sourceImage)
        {
            string sM = nameof(ProcessBlob);

            if (sourceImage == null)
            {
                LogMan.Error(sC, sM, "Source image is null");
                return null;
            }
            var options = BlobOptions;
            if (options?.Threshold?.BackgroundColors == null || options.Threshold.BackgroundColors.Count == 0)
            {
                LogMan.Error(sC, sM, "No background colors selected");
                return null;
            }
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                OnPreProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataSource = sourceImage,
                });

                BlobAnalysis.Reset();
                BlobAnalysis.Options = options;
                var result = BlobAnalysis.Process(sourceImage, cts.Token);
                if (!result.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"Blob analysis failed : {result.Message}");
                    return null;
                }
                LogMan.Message(sC, sM, $"{BlobAnalysis.BlobList.Count} blobs found");
                // Output: all blobs stacked vertically, with 1 pixel of padding
                var imageDataOutput = BlobTools.ReshapeVertical(BlobAnalysis, 0, 1, 1, 0, 1);
                OnProcess?.Invoke(this, new ColorManagerProcessEventArgs
                {
                    DataDest = imageDataOutput,
                    DataSource = sourceImage,
                    Transformation = null,
                });
                return imageDataOutput;
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "blob processing failed", ex);
                return null;
            }
        }

        /// <summary>
        /// Runs <see cref="ProcessBlob"/> on a worker thread.
        /// </summary>
        public async Task<ImageData> ProcessBlobAsync(ImageData sourceImage)
            => await Task.Run(() => ProcessBlob(sourceImage));
    }
}

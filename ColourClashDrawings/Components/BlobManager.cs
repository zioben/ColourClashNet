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
    public partial class BlobManager : Component
    {

        static string sC = nameof(BlobManager);

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [DefaultValue(null)]
        public BlobConfig blobConfig { get; set; } = new BlobConfig();

        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Blob BlobAnalysis { get; set; } = new Blob();


        public event EventHandler OnCreate;
        public event EventHandler OnReset;
        public event EventHandler<ColorManagerProcessEventArgs> OnPreProcess;
        public event EventHandler<ColorManagerProcessEventArgs> OnBkgRemoved;
        public event EventHandler<ColorManagerProcessEventArgs> OnQuantize;
        public event EventHandler<ColorManagerProcessEventArgs> OnProcess;


        public BlobManager()
        {
            InitializeComponent();
        }

        public BlobManager(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }




        public BlobManager WithProcessingParams( Palette bkgPalette, int bkgReplacementColorRGB )
        {
            blobConfig.WithThreshold(bkgPalette, (int)blobConfig.ThresholdRGB.ToleranceFactor, blobConfig.ThresholdRGB.InvertResults)
            .WithColors(bkgReplacementColorRGB);
            return this;
        }
        public BlobManager WithProcessingParams(BlobConfig config, int bkgReplacementColorRGB)
        {
            blobConfig.WithThreshold(config)
            .WithFiltering(config)
            .WithProcessing(config)
            .WithColors(bkgReplacementColorRGB);
            return this;
        }
       

        public ImageData ProcessBlob(ImageData sourceImage)
        {
            string sM = nameof(ProcessBlob);

            if (sourceImage == null)
            {
                LogMan.Error(sC, sM, "Source image is null");
                return null;
            }
            if (blobConfig.ThresholdRGB.ThresholdPalette.Count == 0)
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
                BlobAnalysis
                    .WithThreshold(blobConfig)
                    .WithFiltering(blobConfig)
                    .WithProcessing(blobConfig);
                var oRes = BlobAnalysis.Process(sourceImage, cts.Token);
                if (!oRes.IsSuccess)
                {
                    LogMan.Error(sC, sM, "Blob analysis failed");
                    return null;
                }
                var imageDataOutput = BlobTools.ReshapeVertical(BlobAnalysis, 0, 1, 1, 0, 1);
     //           imageDataOutput = this.BlobAnalysis.GetBlobSegmentedImage();
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
                LogMan.Exception(sC, sM, ex);
                return null;
            }
        }

        public async Task<ImageData> ProcessBlobAsync(ImageData sourceImage)
            => await Task.Run(() => ProcessBlob(sourceImage));
    }
}

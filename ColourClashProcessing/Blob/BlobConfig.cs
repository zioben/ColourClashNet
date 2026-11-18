using ColourClashNet;
using ColourClashNet.Color;
using ColourClashProcessing.Threshold;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ColourClashProcessing.Blob
{
    /// <summary>
    /// Blob detection settings: threshold, connectivity, size filters and rebuild colors.
    /// </summary>
    public class BlobConfig
    {

        /// <summary>
        /// Pixel connectivity.
        /// </summary>
        [Browsable(true), Category("Processing")]
        public BlobConnectionModes ConnectionMode { get; set; } = BlobConnectionModes.EightConnected;

        /// <summary>
        /// Minimum blob width (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobWidth { get; private set; } = 0;

        /// <summary>
        /// Minimum blob height (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobHeight { get; private set; } = 0;

        /// <summary>
        /// Minimum blob area (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobArea { get; private set; } = 0;

        /// <summary>
        /// Maximum blob width (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobWidth { get; private set; } = 0;

        /// <summary>
        /// Maximum blob height (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobHeight { get; private set; } = 0;

        /// <summary>
        /// Maximum blob area (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobArea { get; private set; } = 0;

        /// <summary>
        /// Threshold that separates foreground from background.
        /// </summary>
        [Browsable(true), Category("Threshold")]
        public GlobalThreshold ThresholdRGB { get; set; } = new();

        /// <summary>
        /// Background color used when blobs are rebuilt as images.
        /// </summary>
        [Browsable(true), Category("Rebuild")]
        public int BlobBkgColor { get; set; } = ColorDefaults.DefaultBkgColorInt;

        /// <summary>
        /// Creates a deep copy.
        /// </summary>
        public BlobConfig Clone()
        {
            return new BlobConfig()
            {
                ConnectionMode = this.ConnectionMode,
                MinBlobWidth = this.MinBlobWidth,
                MinBlobHeight = this.MinBlobHeight,
                MinBlobArea = this.MinBlobArea,
                MaxBlobWidth = this.MaxBlobWidth,
                MaxBlobHeight = this.MaxBlobHeight,
                MaxBlobArea = this.MaxBlobArea,
                ThresholdRGB = this.ThresholdRGB.Clone(),
                BlobBkgColor = this.BlobBkgColor
            };
        }   


        /// <summary>
        /// Sets the threshold (fluent).
        /// </summary>
        public BlobConfig WithThreshold(Palette thresholdPalette, double toleranceFactor, bool invertResults)
        {
            ThresholdRGB.WithThreshold(thresholdPalette, toleranceFactor, invertResults);
            return this;
        }
        /// <summary>
        /// Sets the threshold from another threshold (fluent).
        /// </summary>
        public BlobConfig WithThreshold(GlobalThreshold thresholdRGB)
            => WithThreshold(thresholdRGB.ThresholdPalette, thresholdRGB.ToleranceFactor, thresholdRGB.InvertResults);
        
        /// <summary>
        /// Copies the threshold settings (fluent).
        /// </summary>
        public BlobConfig WithThreshold(BlobConfig blobConfig)
             =>WithThreshold(blobConfig.ThresholdRGB);


        /// <summary>
        /// Sets the size filters (fluent).
        /// </summary>
        public BlobConfig WithFiltering(int minBlobWidth, int minBlobHeight, int minBlobArea, int maxBlobWidth, int maxBlobHeight, int maxBlobArea)
        {
            MinBlobWidth = minBlobWidth;
            MinBlobHeight = minBlobHeight;
            MinBlobArea = minBlobArea;
            MaxBlobWidth = maxBlobWidth;
            MaxBlobHeight = maxBlobHeight;
            MaxBlobArea = maxBlobArea;
            return this;
        }

        /// <summary>
        /// Copies the size filters (fluent).
        /// </summary>
        public BlobConfig WithFiltering(BlobConfig blobConfig)
            => WithFiltering(blobConfig.MinBlobWidth, blobConfig.MinBlobHeight, blobConfig.MinBlobArea, blobConfig.MaxBlobWidth, blobConfig.MaxBlobHeight, blobConfig.MaxBlobArea);

        /// <summary>
        /// Sets the connectivity (fluent).
        /// </summary>
        public BlobConfig WithProcessing(BlobConnectionModes connectionMode )
        {
            ConnectionMode = connectionMode;
            return this;
        }

        /// <summary>
        /// Copies the connectivity (fluent).
        /// </summary>
        public BlobConfig WithProcessing(BlobConfig blobConfig)
            => WithProcessing(blobConfig.ConnectionMode);


        /// <summary>
        /// Sets the rebuild background color (fluent).
        /// </summary>
        public BlobConfig WithColors(int blobBkgColor)
        {

            BlobBkgColor = blobBkgColor;
            return this;
        }

        /// <summary>
        /// Copies the rebuild background color (fluent).
        /// </summary>
        public BlobConfig WithColors(BlobConfig blobConfig)
             => WithColors(blobConfig.BlobBkgColor);

        /// <summary>
        /// Restores the default settings (the background color is kept).
        /// </summary>
        public BlobConfig Reset()
        {
            ConnectionMode = BlobConnectionModes.EightConnected;
            MinBlobWidth = 0;
            MinBlobHeight = 0;
            MinBlobArea = 0;
            MaxBlobWidth = 0;
            MaxBlobHeight = 0;
            MaxBlobArea = 0;
            ThresholdRGB = new GlobalThreshold();
            return this;
        }

    }
}

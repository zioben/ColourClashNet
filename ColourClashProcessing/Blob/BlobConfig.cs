using ColourClashNet;
using ColourClashNet.Color;
using ColourClashProcessing.Threshold;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ColourClashProcessing.Blob
{
    public class BlobConfig
    {

        [Browsable(true), Category("Processing")]
        public BlobConnectionModes ConnectionMode { get; set; } = BlobConnectionModes.EightConnected;

        [Browsable(true), Category("Filtering")]
        public int MinBlobWidth { get; private set; } = 0;

        [Browsable(true), Category("Filtering")]
        public int MinBlobHeight { get; private set; } = 0;

        [Browsable(true), Category("Filtering")]
        public int MinBlobArea { get; private set; } = 0;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobWidth { get; private set; } = 0;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobHeight { get; private set; } = 0;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobArea { get; private set; } = 0;

        [Browsable(true), Category("Threshold")]
        public GlobalThreshold ThresholdRGB { get; set; } = new();

        [Browsable(true), Category("Rebuild")]
        public int BlobBkgColor { get; set; } = ColorDefaults.DefaultShapeForeColorInt;

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


        public BlobConfig WithThreshold(Palette thresholdPalette, double toleranceFactor, bool invertResults)
        {
            ThresholdRGB.WithThreshold(thresholdPalette, toleranceFactor, invertResults);
            return this;
        }
        public BlobConfig WithThreshold(GlobalThreshold thresholdRGB)
            => WithThreshold(thresholdRGB.ThresholdPalette, thresholdRGB.ToleranceFactor, thresholdRGB.InvertResults);
        
        public BlobConfig WithThreshold(BlobConfig blobConfig)
             =>WithThreshold(blobConfig.ThresholdRGB);


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

        public BlobConfig WithFiltering(BlobConfig blobConfig)
            => WithFiltering(blobConfig.MinBlobWidth, blobConfig.MinBlobHeight, blobConfig.MinBlobArea, blobConfig.MaxBlobWidth, blobConfig.MaxBlobHeight, blobConfig.MaxBlobArea);

        public BlobConfig WithProcessing(BlobConnectionModes connectionMode )
        {
            ConnectionMode = connectionMode;
            return this;
        }

        public BlobConfig WithProcessing(BlobConfig blobConfig)
            => WithProcessing(blobConfig.ConnectionMode);


        public BlobConfig WithColors(int blobBkgColor)
        {

            BlobBkgColor = blobBkgColor;
            return this;
        }

        public BlobConfig WithColors(BlobConfig blobConfig)
             => WithColors(blobConfig.BlobBkgColor);

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

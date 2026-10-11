using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using ColourClashProcessing.Threshold;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace ColourClashProcessing.Blob
{
    public class Blob
    {
        static String sC = nameof(Blob);

        GlobalThreshold threshold = new GlobalThreshold();
        
        Shape shape = new Shape();


        [Browsable(true),Category("Input")]
        public Palette ThresholdPalette { get; set; } = new();

        [Browsable(true), Category("Input")]
        public double ThresholdTolerance { get; set; } = 0.0;

        [Browsable(true), Category("Input")]
        public bool ThresholdInvertResults { get; set; } = false;

        [Browsable(true), Category("Input")]
        public BlobConnectionModes ConnectionMode { get; private set; } = BlobConnectionModes.FourConnected;


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

        [Browsable(true), Category("Data")]
        int SourceWidth => shape.Valid ? shape.SourceWidth : 0;

        [Browsable(true), Category("Data")]
        int SourceHeight => shape.Valid ? shape.SourceHeight : 0;
    

        /// <summary>Blobs found by the last <see cref="Process"/>, numbered 1..N.</summary>
        public IReadOnlyList<BlobItem> Blobs { get; private set; } = Array.Empty<BlobItem>();

        public bool Valid => shape.Valid && _connectedComponents != null;

        Dictionary<int, List<RunLength>>? _connectedComponents = null;

        public ImageData ImageSource { get;private set; }
        public ImageData ImageShape { get; private set; }
        public ImageData ImageBlob { get; private set; }


        public Blob()
        {
            Reset();
        }

        /// <summary>Restores the default configuration and clears the results.</summary>
        public void Reset()
        {
            threshold = new GlobalThreshold();
            ThresholdPalette = new Palette().Create(threshold.ThresholdPalette);
            ThresholdTolerance= threshold.ToleranceFactor;
            ThresholdInvertResults = threshold.InvertResults;

            shape = new Shape();
            ConnectionMode = BlobConnectionModes.FourConnected;
            MinBlobArea = 0;
            MinBlobWidth = 0;
            MinBlobHeight = 0;
            MaxBlobArea = 0;
            MaxBlobWidth = 0;
            MaxBlobHeight = 0;
            ResetResults();
        }

        /// <summary>
        /// Clears only the results. Process() calls this one: calling Reset() there would
        /// throw away what WithThreshold()/WithProcessing() have just configured.
        /// </summary>
        void ResetResults()
        {
            shape.Reset();
            Blobs = Array.Empty<BlobItem>();
            ImageSource = null;
            ImageShape = null;
            ImageBlob = null;
            _connectedComponents = null;
        }

        public Blob WithThreshold(Palette thresholdPalette, double toleranceFactor = 0.0, bool invertResults = false)
        {
            ThresholdPalette= new Palette().Create(thresholdPalette);
            ThresholdTolerance = toleranceFactor;
            ThresholdInvertResults = invertResults;
            return this;
        }

        public Blob WithThreshold(int rgbThreshold, double toleranceFactor = 0.0, bool invertResults = false)
        {
            ThresholdPalette = new Palette().Create(new List<int> { rgbThreshold });
            ThresholdTolerance = toleranceFactor;
            ThresholdInvertResults = invertResults;
            return this;
        }

        public Blob WithProcessing(BlobConnectionModes connectionMode)
        {
            ConnectionMode = connectionMode;
            return this;
        }
        public Blob WithFiltering(int minArea, int maxArea, int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            MinBlobArea = minArea;
            MaxBlobArea = maxArea;
            MinBlobWidth = minWidth;
            MaxBlobWidth = maxWidth;
            MinBlobHeight = minHeight;
            MaxBlobHeight = maxHeight;
            return this;
        }

        public ColorTransformResult Process(ImageData image, CancellationToken cancellationToken = default)
        {
            string sM = nameof(Process);
            try
            {
                ResetResults();
                ImageData.AssertValid(image);
                ImageSource = new ImageData().Create(image);
                ImageData.AssertValid(ImageSource);

                // 1. foreground / background split
                var thresholdResult = threshold.Process(ImageSource, cancellationToken);
                if (!thresholdResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"Threshold processing failed: {thresholdResult.Message}", thresholdResult.Exception);
                    return thresholdResult;
                }

                // 2. horizontal run lengths of the foreground
                var shapeResult = shape.Process(thresholdResult.DataOut, cancellationToken);
                if (!shapeResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"Shape processing failed: {shapeResult.Message}", shapeResult.Exception);
                    return shapeResult;
                }

                // 3. run lengths -> blobs
                var blobs = BlobBuilder.Build(shape.RunLengths, ConnectionMode, cancellationToken);

                // 4. filter blobs by size
                if (MinBlobArea > 0)
                    blobs.RemoveAll(b => b.Area < MinBlobArea);
                if (MaxBlobArea > 0)
                    blobs.RemoveAll(b => b.Area > MaxBlobArea);
                if (MinBlobWidth > 0)
                    blobs.RemoveAll(b => b.Width < MinBlobWidth);
                if (MaxBlobWidth > 0)
                    blobs.RemoveAll(b => b.Width > MaxBlobWidth);
                if (MinBlobHeight > 0)
                    blobs.RemoveAll(b => b.Height < MinBlobHeight);
                if (MaxBlobHeight > 0)
                    blobs.RemoveAll(b => b.Height > MaxBlobHeight);

                Blobs = blobs;
                _connectedComponents = blobs.ToDictionary(b => b.BlobId, b => b.RunLengths);

                ImageShape = GetShapeImage();
                ImageBlob = GetBlobImage();

                return ColorTransformResult.CreateValidResult(
                    image,
                    thresholdResult.DataOut,
                    $"Blob processing completed: {blobs.Count} blobs from {shape.RunLengths.Count} run lengths ({ConnectionMode}).");
            }
            catch (OperationCanceledException cex)
            {
                LogMan.Exception(sC, sM, "Operation canceled", cex);
                return ColorTransformResult.CreateErrorResult(image, null, "Operation canceled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Blob processing failed", ex);
                ResetResults();
                return ColorTransformResult.CreateErrorResult(image, null, $"Exception during blob processing: {ex.Message}", ex);
            }
        }

        
        public ImageData GetShapeImage()
        {
            return shape.Valid ? shape.ToImageData() : throw new InvalidOperationException("Shape image is not available. Call Process() first.");
        }
        public ImageData GetBlobImage()
        {
            if(!Valid) 
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");

            List<RunLength> allRuns = _connectedComponents!.Values.SelectMany(runs => runs).ToList();
            var blobImage = RunLength.ToImageData(allRuns, SourceWidth, SourceHeight);
            return blobImage;
        }

        private BlobItem GetBlobItem(int iBlobID)
        {
            if (!Valid)
                throw new InvalidOperationException("Blob is not available. Call Process() first.");
            var blob = Blobs.FirstOrDefault(b => b.BlobId == iBlobID);
            if (blob == null)
            {
                return null;
            }
            return blob;
        }

        public ImageData GetBlobImage(int iBlobID)
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");
            var blob = GetBlobItem(iBlobID);
            if (blob == null)
            {
                return null;
            }
            var matrixSrc = ImageSource.GetMatrix();
            var matrixDst =MatrixTools.CreateMatrix(blob.Height, blob.Width, 0);  
            MatrixTools.Blit( matrixSrc , matrixDst, blob.XMin,blob.YMin, 0, 0, blob.Width, blob.Height);
            var imageData = new ImageData().Create(matrixDst);
            return imageData;
        }

/*        public bool SaveBlobItemImage(string basePath, int iBlobID  )
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");
            var imageData = GetBlobImage(iBlobID);
            return ImageTools.SaveImage(imageData, $"{basePath}_Blob_{iBlobID:D4}.png", ImageSaveFormat.Png);
        }
        public bool SaveBlobImages(string basePath)
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");
            bool bRet = true;
            foreach (var blob in Blobs)
            {
                bRet &= SaveBlobItemImage(basePath, blob.BlobId);
            }
            return bRet;
        }
*/
    }
}
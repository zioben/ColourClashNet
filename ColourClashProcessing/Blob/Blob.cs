using ColourClashNet;
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

        #region private fields

        BlobConfig config = new BlobConfig();

//        GlobalThreshold threshold = new GlobalThreshold();
        Dictionary<int, List<RunLength>>? _connectedComponents = null;
        Shape shape = new Shape();

        StopWatch timer=new();

        #endregion

        #region config properties

        [Browsable(true), Category("Input")]
        public Palette ThresholdPalette => config.ThresholdRGB.ThresholdPalette;

        [Browsable(true), Category("Input")]
        public double ThresholdTolerance => config.ThresholdRGB.ToleranceFactor;

        [Browsable(true), Category("Input")]
        public bool ThresholdInvertResults => config.ThresholdRGB.InvertResults;

        [Browsable(true), Category("Input")]
        public BlobConnectionModes ConnectionMode => config.ConnectionMode;

        #endregion

        #region filtering proeprties

        [Browsable(true), Category("Filtering")]
        public int MinBlobWidth => config.MinBlobWidth;

        [Browsable(true), Category("Filtering")]
        public int MinBlobHeight => config.MinBlobHeight;

        [Browsable(true), Category("Filtering")]
        public int MinBlobArea => config.MinBlobArea;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobWidth => config.MaxBlobWidth;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobHeight => config.MaxBlobHeight;

        [Browsable(true), Category("Filtering")]
        public int MaxBlobArea => config.MaxBlobArea;

        public int BlobBkgColor => config.BlobBkgColor;

        #endregion

        #region output

        /// <summary>Blobs found by the last <see cref="Process"/>, numbered 1..N.</summary>
        public IReadOnlyList<BlobItem> BlobList { get; private set; } = Array.Empty<BlobItem>();

        public bool Valid => shape.Valid && _connectedComponents != null;

        public ImageData ImageSource { get; private set; }

        public double ProcessingTime => timer.ElapsedMilliseconds;

        #endregion


        public Blob()
        {
            Reset();
        }

        /// <summary>Restores the default configuration and clears the results.</summary>
        public void Reset()
        {
            config = new BlobConfig();
            shape = new Shape();
            ResetResults();
        }

        /// <summary>
        /// Clears only the results. Process() calls this one: calling Reset() there would
        /// throw away what WithThreshold()/WithProcessing() have just configured.
        /// </summary>
        void ResetResults()
        {
            shape.Reset();
            BlobList = Array.Empty<BlobItem>();
            ImageSource = null;
            _connectedComponents = null;
        }

        #region setup fluent

        public Blob WithThreshold(Palette thresholdPalette, double toleranceFactor, bool invertResults)
        {
            config.ThresholdRGB.WithThreshold( new Palette().Create(thresholdPalette),toleranceFactor,invertResults);
            return this;
        }

        public Blob WithThreshold(int rgbThreshold, double toleranceFactor, bool invertResults)
        {
            config.ThresholdRGB.WithThreshold( new Palette().Create(new List<int> { rgbThreshold }), toleranceFactor, invertResults);
            return this;
        }

        public Blob WithThreshold(GlobalThreshold thresholdRGB)
            => WithThreshold(thresholdRGB.ThresholdPalette, thresholdRGB.ToleranceFactor, thresholdRGB.InvertResults);

        public Blob WithThreshold(BlobConfig blobConfig)
            => WithThreshold(blobConfig.ThresholdRGB);

        public Blob WithProcessing(BlobConnectionModes connectionMode)
        {
            config.WithProcessing(connectionMode);
            return this;
        }

        public Blob WithProcessing(BlobConfig blobConfig)
            => WithProcessing(blobConfig.ConnectionMode);

        public Blob WithColors( int bkgColorRGB)
        {
            config.WithColors( bkgColorRGB);
            return this;
        }

        public Blob WithColors(BlobConfig blobConfig)
        { 
            config.WithColors(blobConfig);
            return this;
        }

        public Blob WithFiltering(int minArea, int maxArea, int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            config.WithFiltering(minWidth, minHeight, minArea, maxWidth, maxHeight, maxArea);
            return this;
        }
        public Blob WithFiltering(BlobConfig config)
        {
            this.config.WithFiltering(config.MinBlobWidth, config.MinBlobHeight, config.MinBlobArea, config.MaxBlobWidth, config.MaxBlobHeight, config.MaxBlobArea);
            return this;
        }

       

        #endregion

        public ColorTransformResult Process(ImageData image, CancellationToken cancellationToken = default)
        {
            string sM = nameof(Process);
            try
            {
                timer.Start();
                ResetResults();
                ImageData.AssertValid(image);
                ImageSource = new ImageData().Create(image);
                ImageData.AssertValid(ImageSource);

                // 1. foreground / background split
                var thresholdResult = config.ThresholdRGB.Process(ImageSource, cancellationToken);
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
                //
                if (MinBlobWidth > 0)
                    blobs.RemoveAll(b => b.Width < MinBlobWidth);
                if (MaxBlobWidth > 0)
                    blobs.RemoveAll(b => b.Width > MaxBlobWidth);
                //
                if (MinBlobHeight > 0)
                    blobs.RemoveAll(b => b.Height < MinBlobHeight);
                if (MaxBlobHeight > 0)
                    blobs.RemoveAll(b => b.Height > MaxBlobHeight);

                BlobList = blobs;
                _connectedComponents = blobs.ToDictionary(b => b.BlobId, b => b.RunLengths);

                timer.Update();

                return ColorTransformResult.CreateValidResult(
                    image,
                    thresholdResult.DataOut,
                    $"Blob processing completed: {blobs.Count} blobs from {shape.RunLengths.Count} run lengths ({ConnectionMode}).");
            }
            catch (OperationCanceledException cex)
            {
                LogMan.Exception(sC, sM, "Operation canceled", cex);
                timer.Update();
                return ColorTransformResult.CreateErrorResult(image, null, "Operation canceled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Blob processing failed", ex);
                ResetResults();
                timer.Update();
                return ColorTransformResult.CreateErrorResult(image, null, $"Exception during blob processing: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Rebuild image containing ALL source shapes
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ImageData GetShapeImage()
        {
           if(!Valid) 
                throw new InvalidOperationException("Shaper image is not available. Call Process() first.");
            return shape.ToImageData();
        }

        /// <summary>
        /// Rebuild image containing only blobs
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ImageData GetBlobImage()
        {
            if(!Valid) 
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");

            List<RunLength> allRuns = _connectedComponents!.Values.SelectMany(runs => runs).ToList();
            var blobImage = RunLength.ToShapeImageData(allRuns, ImageSource);
            return blobImage;
        }

        /// <summary>
        /// Get blob item data
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private BlobItem GetBlobItem(int blobId)
        {
            if (!Valid)
                throw new InvalidOperationException("Blob is not available. Call Process() first.");
            var blob = BlobList.FirstOrDefault(b => b.BlobId == blobId);
            if (blob == null)
            {
                return null;
            }
            return blob;
        }

        /// <summary>
        /// Rebuild a single shape with original image associated to the Blob ID
        /// <para>Coockie cut algorithm is used to avoid partial overlapping with other shapes inside the blob bounding box</para>
        /// </summary>
        /// <param name="blobId"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ImageData GetBlobImage(int blobId)
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");
            var blob = GetBlobItem(blobId);
            if (blob == null)
            {
                return null;
            }
            var matrixDst = MatrixTools.CreateMatrix(blob.Width, blob.Height, config.BlobBkgColor);
            var imageMask = RunLength.ToShapeImageData(blob.RunLengths);
            MatrixTools.CoockieCut(ImageSource, imageMask, matrixDst, blob.XMin,blob.YMin,0,0, 0, 0, blob.Width, blob.Height);
            //MatrixTools.Blit(ImageSource, matrixDst, blob.XMin, blob.YMin, 0, 0, blob.Width, blob.Height);
            var imageData = new ImageData().Create(matrixDst);
            return imageData;
        }

        public ImageData GetBlobImage(BlobItem blobItem)
        {
            if (blobItem == null)
                throw new NullReferenceException($"{nameof(blobItem)}");
            return GetBlobImage(blobItem.BlobId);
        }

        /// <summary>
        /// Rebuild a shaped image. Segmentation is obtained associating every shape with the associated Blob ID
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public ImageData GetBlobSegmentedImage()
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");

            var matrix = MatrixTools.CreateMatrix(ImageSource.Width, ImageSource.Height);
            foreach (var item in BlobList)
            {
                RunLength.MergeToMatrix(item.RunLengths, matrix);
            }
            var blobImage = new ImageData().Create(matrix);
            return blobImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEnumerable<ImageData> ToEnumerable()
        {
            if (!Valid)
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");

            foreach (var item in BlobList)
            {
                var image = GetBlobImage(item);
                image.Name = $"{item.BlobId}";
                yield return image;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<ImageData> ToImageList()
            => ToEnumerable().ToList();

    }
}
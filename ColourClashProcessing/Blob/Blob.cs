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
    /// <summary>
    /// Blob (connected component) detector for sprite and tile extraction.
    /// <para/>
    /// Pipeline: global threshold (foreground / background split), horizontal run length extraction,
    /// run based connected component labeling, size filtering. The detected blobs can then be rebuilt
    /// as images cut from the source image.
    /// </summary>
    public class Blob
    {
        static String sC = nameof(Blob);

        #region private fields

        BlobConfig config = new BlobConfig();

        // Run lengths of each blob, by blob id
        Dictionary<int, List<RunLength>>? connectedComponents = null;
        Shape shape = new Shape();

        StopWatch timer=new();

        #endregion

        #region config properties

        /// <summary>
        /// Background reference colors.
        /// </summary>
        [Browsable(true), Category("Input")]
        public Palette ThresholdPalette => config.ThresholdRGB.ThresholdPalette;

        /// <summary>
        /// Threshold tolerance.
        /// </summary>
        [Browsable(true), Category("Input")]
        public double ThresholdTolerance => config.ThresholdRGB.ToleranceFactor;

        /// <summary>
        /// Swaps foreground and background.
        /// </summary>
        [Browsable(true), Category("Input")]
        public bool ThresholdInvertResults => config.ThresholdRGB.InvertResults;

        /// <summary>
        /// Pixel connectivity.
        /// </summary>
        [Browsable(true), Category("Input")]
        public BlobConnectionModes ConnectionMode => config.ConnectionMode;

        #endregion

        #region filtering properties

        /// <summary>
        /// Minimum blob width (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobWidth => config.MinBlobWidth;

        /// <summary>
        /// Minimum blob height (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobHeight => config.MinBlobHeight;

        /// <summary>
        /// Minimum blob area (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MinBlobArea => config.MinBlobArea;

        /// <summary>
        /// Maximum blob width (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobWidth => config.MaxBlobWidth;

        /// <summary>
        /// Maximum blob height (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobHeight => config.MaxBlobHeight;

        /// <summary>
        /// Maximum blob area (0 = no limit).
        /// </summary>
        [Browsable(true), Category("Filtering")]
        public int MaxBlobArea => config.MaxBlobArea;

        /// <summary>
        /// Background color of the rebuilt blob images.
        /// </summary>
        public int BlobBkgColor => config.BlobBkgColor;

        #endregion

        #region output

        /// <summary>Blobs found by the last <see cref="Process"/>, numbered 1..N.</summary>
        public IReadOnlyList<BlobItem> BlobList { get; private set; } = Array.Empty<BlobItem>();

        /// <summary>
        /// True after a successful <see cref="Process"/>.
        /// </summary>
        public bool Valid => shape.Valid && connectedComponents != null;

        /// <summary>
        /// Copy of the processed image.
        /// </summary>
        public ImageData ImageSource { get; private set; }

        /// <summary>
        /// Duration of the last <see cref="Process"/>, in milliseconds.
        /// </summary>
        public double ProcessingTime => timer.ElapsedMilliseconds;

        #endregion


        /// <summary>
        /// Creates a detector with the default configuration.
        /// </summary>
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
            connectedComponents = null;
        }

        #region setup fluent

        /// <summary>
        /// Sets the threshold (fluent).
        /// </summary>
        public Blob WithThreshold(Palette thresholdPalette, double toleranceFactor, bool invertResults)
        {
            config.ThresholdRGB.WithThreshold( new Palette().Create(thresholdPalette),toleranceFactor,invertResults);
            return this;
        }

        /// <summary>
        /// Sets a single threshold color (fluent).
        /// </summary>
        public Blob WithThreshold(int rgbThreshold, double toleranceFactor, bool invertResults)
        {
            config.ThresholdRGB.WithThreshold( new Palette().Create(new List<int> { rgbThreshold }), toleranceFactor, invertResults);
            return this;
        }

        /// <summary>
        /// Copies the threshold settings (fluent).
        /// </summary>
        public Blob WithThreshold(GlobalThreshold thresholdRGB)
            => WithThreshold(thresholdRGB.ThresholdPalette, thresholdRGB.ToleranceFactor, thresholdRGB.InvertResults);

        /// <summary>
        /// Copies the threshold settings from a configuration (fluent).
        /// </summary>
        public Blob WithThreshold(BlobConfig blobConfig)
            => WithThreshold(blobConfig.ThresholdRGB);

        /// <summary>
        /// Sets the connectivity (fluent).
        /// </summary>
        public Blob WithProcessing(BlobConnectionModes connectionMode)
        {
            config.WithProcessing(connectionMode);
            return this;
        }

        /// <summary>
        /// Copies the connectivity from a configuration (fluent).
        /// </summary>
        public Blob WithProcessing(BlobConfig blobConfig)
            => WithProcessing(blobConfig.ConnectionMode);

        /// <summary>
        /// Sets the background color of the rebuilt images (fluent).
        /// </summary>
        public Blob WithColors( int bkgColorRGB)
        {
            config.WithColors( bkgColorRGB);
            return this;
        }

        /// <summary>
        /// Copies the colors from a configuration (fluent).
        /// </summary>
        public Blob WithColors(BlobConfig blobConfig)
        { 
            config.WithColors(blobConfig);
            return this;
        }

        /// <summary>
        /// Sets the size filters; 0 disables a limit (fluent).
        /// </summary>
        public Blob WithFiltering(int minArea, int maxArea, int minWidth, int maxWidth, int minHeight, int maxHeight)
        {
            config.WithFiltering(minWidth, minHeight, minArea, maxWidth, maxHeight, maxArea);
            return this;
        }
        /// <summary>
        /// Copies the size filters from a configuration (fluent).
        /// </summary>
        public Blob WithFiltering(BlobConfig config)
        {
            this.config.WithFiltering(config.MinBlobWidth, config.MinBlobHeight, config.MinBlobArea, config.MaxBlobWidth, config.MaxBlobHeight, config.MaxBlobArea);
            return this;
        }

       

        #endregion

        /// <summary>
        /// Detects the blobs of <paramref name="image"/>.
        /// </summary>
        /// <returns>A result whose output is the thresholded (binary) image.</returns>
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
                connectedComponents = blobs.ToDictionary(b => b.BlobId, b => b.RunLengths);

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
        /// Rebuilds the binary image of all foreground shapes, before the size filtering.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Process"/> has not been called.</exception>
        public ImageData GetShapeImage()
        {
           if(!Valid) 
                throw new InvalidOperationException("Shape image is not available. Call Process() first.");
            return shape.ToImageData();
        }

        /// <summary>
        /// Rebuilds the binary image of the blobs that passed the size filtering.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Process"/> has not been called.</exception>
        public ImageData GetBlobImage()
        {
            if(!Valid) 
                throw new InvalidOperationException("Blob image is not available. Call Process() first.");

            List<RunLength> allRuns = connectedComponents!.Values.SelectMany(runs => runs).ToList();
            var blobImage = RunLength.ToShapeImageData(allRuns, ImageSource);
            return blobImage;
        }

        /// <summary>
        /// Returns the blob with the given id, or null.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Process"/> has not been called.</exception>
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
        /// Cuts the source pixels of a blob into a new image as large as its bounding box.
        /// <para/>
        /// A cookie cut with the blob shape is used, so other shapes overlapping the bounding box are not copied.
        /// </summary>
        /// <returns>The blob image, or null if the id does not exist.</returns>
        /// <exception cref="InvalidOperationException"><see cref="Process"/> has not been called.</exception>
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
            var imageData = new ImageData().Create(matrixDst);
            return imageData;
        }

        /// <summary>
        /// Cuts the source pixels of a blob into a new image.
        /// </summary>
        /// <exception cref="NullReferenceException"><paramref name="blobItem"/> is null.</exception>
        public ImageData GetBlobImage(BlobItem blobItem)
        {
            if (blobItem == null)
                throw new NullReferenceException($"{nameof(blobItem)}");
            return GetBlobImage(blobItem.BlobId);
        }

        /// <summary>
        /// Draws the shapes of all blobs on a black image of the source size.
        /// </summary>
        /// <exception cref="InvalidOperationException"><see cref="Process"/> has not been called.</exception>
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
        /// Enumerates the blob images; each image is named after its blob id.
        /// </summary>
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
        /// Returns the images of all blobs; each image is named after its blob id.
        /// </summary>
        public List<ImageData> ToImageList()
            => ToEnumerable().ToList();

    }
}
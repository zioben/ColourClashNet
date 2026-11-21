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

        BlobOptions options = new();

        // Run lengths of each blob, by blob id
        Dictionary<int, List<RunLength>>? connectedComponents = null;
        Shape shape = new Shape();

        StopWatch timer=new();

        #endregion



        /// <summary>
        /// Detection settings (threshold, connectivity, size filters, rebuild color).
        /// </summary>
        /// <exception cref="ArgumentNullException">Set to null.</exception>
        public BlobOptions Options
        {
            get => options;
            set => options = value ?? throw new ArgumentNullException(nameof(value));
        }

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

        /// <summary>Restores the default options and clears the results.</summary>
        public void Reset()
        {
            options = new BlobOptions();
            shape = new Shape();
            ResetResults();
        }

        /// <summary>
        /// Clears only the results. Process() calls this one: calling Reset() there would
        /// throw away the configured <see cref="Options"/>.
        /// </summary>
        void ResetResults()
        {
            shape.Reset();
            BlobList = Array.Empty<BlobItem>();
            ImageSource = null;
            connectedComponents = null;
        }


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

                // Snapshot of the options: records are immutable, later changes do not affect this run
                var opts = Options;
                var errors = opts.Validate().ToList();
                if (errors.Count > 0)
                {
                    LogMan.Error(sC, sM, $"invalid options : {string.Join("; ", errors)}");
                    timer.Update();
                    return ColorTransformResult.CreateErrorResult(sC, sM, image, null, $"invalid options : {string.Join("; ", errors)}");
                }

                LogMan.Debug(sC, sM, $"blob analysis of {ImageSource.Width}x{ImageSource.Height} : {opts.Threshold.BackgroundColors.Count} background colors, tolerance {opts.Threshold.Tolerance}, {opts.ConnectionMode}");

                // 1. foreground / background split
                var threshold = new GlobalThreshold { Options = opts.Threshold };
                var thresholdResult = threshold.Process(ImageSource, cancellationToken);
                if (!thresholdResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"Threshold processing failed : {thresholdResult.Message}", thresholdResult.Exception);
                    timer.Update();
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, image, "threshold stage failed", thresholdResult);
                }

                // 2. horizontal run lengths of the foreground
                var shapeResult = shape.Process(thresholdResult.DataOut, cancellationToken);
                if (!shapeResult.IsSuccess)
                {
                    LogMan.Error(sC, sM, $"Shape processing failed : {shapeResult.Message}", shapeResult.Exception);
                    timer.Update();
                    return ColorTransformResult.CreateErrorResultFrom(sC, sM, image, "run length stage failed", shapeResult);
                }

                // 3. run lengths -> blobs
                var blobs = BlobBuilder.Build(shape.RunLengths, opts.ConnectionMode, cancellationToken);
                int blobsFound = blobs.Count;
                LogMan.Trace(sC, sM, $"{shape.RunLengths.Count} run lengths merged into {blobsFound} blobs");

                // 4. filter blobs by size
                if (opts.MinArea > 0)
                    blobs.RemoveAll(b => b.Area < opts.MinArea);
                if (opts.MaxArea > 0)
                    blobs.RemoveAll(b => b.Area > opts.MaxArea);
                //
                if (opts.MinWidth > 0)
                    blobs.RemoveAll(b => b.Width < opts.MinWidth);
                if (opts.MaxWidth > 0)
                    blobs.RemoveAll(b => b.Width > opts.MaxWidth);
                //
                if (opts.MinHeight > 0)
                    blobs.RemoveAll(b => b.Height < opts.MinHeight);
                if (opts.MaxHeight > 0)
                    blobs.RemoveAll(b => b.Height > opts.MaxHeight);

                BlobList = blobs;
                connectedComponents = blobs.ToDictionary(b => b.BlobId, b => b.RunLengths);

                timer.Update();
                LogMan.Debug(sC, sM, $"blob analysis completed : {blobs.Count} blobs kept of {blobsFound} found by the size filters");

                return ColorTransformResult.CreateValidResult(
                    image,
                    thresholdResult.DataOut,
                    $"Blob processing completed: {blobs.Count} blobs from {shape.RunLengths.Count} run lengths ({opts.ConnectionMode}).");
            }
            catch (OperationCanceledException cex)
            {
                LogMan.Warning(sC, sM, "Operation cancelled");
                timer.Update();
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, "blob processing cancelled", cex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, "Blob processing failed", ex);
                ResetResults();
                timer.Update();
                return ColorTransformResult.CreateErrorResult(sC, sM, image, null, $"blob processing failed : {ex.Message}", ex);
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
            var matrixDst = MatrixTools.CreateMatrix(blob.Width, blob.Height, Options.BackgroundColor);
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
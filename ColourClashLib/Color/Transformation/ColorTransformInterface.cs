using ColourClashNet.Color.Dithering;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Contract of a color transformation (color reduction) algorithm.
    /// <para/>
    /// Typical usage: <see cref="SetOptions"/> (optional), <see cref="Create(ImageData)"/> with the source image,
    /// then <see cref="ProcessColors"/>; or the one-shot <see cref="CreateAndProcessColors(ImageData, CancellationToken)"/>.
    /// </summary>
    public interface ColorTransformInterface
    {
        //------------------------------------------------------------
        /// <summary>
        /// Algorithm identifier.
        /// </summary>
        ColorTransformType Type { get; }
        /// <summary>
        /// Instance name.
        /// </summary>
        String Name { get; }
        /// <summary>
        /// Human readable description of the algorithm.
        /// </summary>
        String Description { get; }

        //------------------------------------------------------------

        /// <summary>
        /// Image used to evaluate the color error and as dithering target; the source image when not specified.
        /// </summary>
        ImageData ImageReference { get; }   
        /// <summary>
        /// Image to be transformed.
        /// </summary>
        ImageData ImageSource { get; }
        /// <summary>
        /// Transformed (and optionally dithered) image.
        /// </summary>
        ImageData ImageOutput { get; }

        //------------------------------------------------------------
        /// <summary>
        /// Source color to output color map built during processing.
        /// </summary>
        ColorTransformationMap TransformationMap { get; }

        //------------------------------------------------------------
        /// <summary>
        /// When true, dithering is skipped even if configured.
        /// </summary>
        public bool BypassDithering { get; set; }

        //------------------------------------------------------------

        //------------------------------------------------------------

        /// <summary>
        /// Current options. The record type depends on the transformation (e.g. <see cref="ClusteringOptions"/>).
        /// </summary>
        TransformOptions Options { get; }

        /// <summary>
        /// Replaces the options. Records are immutable, so the instance is stored without copying.
        /// </summary>
        /// <returns>This instance (fluent).</returns>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not of the type expected by the transformation.</exception>
        ColorTransformInterface SetOptions(TransformOptions options);


        //------------------------------------------------------------

        /// <summary>
        /// Loads the source image and an optional reference image.
        /// </summary>
        /// <returns>This instance, or null on error.</returns>
        ColorTransformInterface Create(ImageData sourceImage, ImageData referenceImage);
        /// <summary>
        /// Loads the source image; it is also used as reference image.
        /// </summary>
        /// <returns>This instance, or null on error.</returns>
        ColorTransformInterface Create(ImageData sourceImage);

        /// <summary>
        /// Runs the transformation (map creation, color reduction, dithering) on the loaded image.
        /// </summary>
        ColorTransformResult ProcessColors(CancellationToken token = default);
        /// <summary>
        /// Loads the images and runs the transformation.
        /// </summary>
        ColorTransformResult CreateAndProcessColors(ImageData sourceImage, ImageData referenceImage,  CancellationToken token = default);
        /// <summary>
        /// Loads the source image and runs the transformation.
        /// </summary>
        ColorTransformResult CreateAndProcessColors(ImageData sourceImage, CancellationToken token = default);
        /// <summary>
        /// Cancels a running transformation.
        /// </summary>
        void AbortProcessing(CancellationTokenSource token);

        /// <summary>
        /// Loads the images and runs the transformation on a worker thread.
        /// </summary>
        Task<ColorTransformResult> CreateAndProcessColorsAsync(ImageData image, ImageData referenceImage = null, CancellationToken token = default);
        /// <summary>
        /// Cancels a running transformation asynchronously.
        /// </summary>
        Task AbortProcessingAsync(CancellationTokenSource token);


        /// <summary>
        /// Metric used to compare colors (from <see cref="Options"/>).
        /// </summary>
        ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; }
        /// <summary>
        /// Mean color error between the reference image and the output image; NaN when not computed.
        /// </summary>
        double TransformationError { get; }

        //------------------------------------------------------------

        /// <summary>
        /// Raised before the images are loaded.
        /// </summary>
        event EventHandler? Creating;
        /// <summary>
        /// Raised after the images are loaded.
        /// </summary>
        event EventHandler? Created;
        /// <summary>
        /// Raised when processing starts; the event args expose a cancellation token source.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Processing;
        /// <summary>
        /// Raised to report processing progress.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? ProcessAdvance;
        /// <summary>
        /// Raised to publish intermediate results (e.g. best result so far during auto-tuning).
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? ProcessPartial;
        /// <summary>
        /// Raised when processing ends, with the final result.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Processed;

    }
}

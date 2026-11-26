using ColourClashNet.Imaging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Contract of a color transformation (color reduction) algorithm.
    /// <para/>
    /// Usage: configure it with <see cref="SetOptions"/> (or the typed <c>Options</c> property),
    /// then call <see cref="Process"/> or <see cref="ProcessAsync"/> with the source image.
    /// <para/>
    /// Events of a run, in order: <see cref="StartProcessing"/>, <see cref="Creating"/>, <see cref="Created"/>,
    /// <see cref="Processing"/>, <see cref="Progress"/> (any number), <see cref="Dithering"/> (only when dithering is applied),
    /// <see cref="Completed"/> (always, also on error or cancellation).
    /// <see cref="Destroying"/> and <see cref="Destroyed"/> are raised by <see cref="IDisposable.Dispose"/>.
    /// </summary>
    public interface IColorTransform : IDisposable
    {
        //------------------------------------------------------------
        /// <summary>
        /// Algorithm identifier, taken from the options (<see cref="TransformOptions.TransformType"/>).
        /// </summary>
        ColorTransformType Type { get; }
        /// <summary>
        /// Instance name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Human readable description of the algorithm.
        /// </summary>
        string Description { get; }

        //------------------------------------------------------------
        /// <summary>
        /// Current options. The record type depends on the transformation (e.g. <see cref="ClusteringOptions"/>).
        /// </summary>
        TransformOptions Options { get; }

        /// <summary>
        /// Replaces the options. Records are immutable, so the instance is stored without copying.
        /// <para/>
        /// Can be called from a <see cref="StartProcessing"/> handler to change the settings of the run that is starting.
        /// </summary>
        /// <returns>This instance (fluent).</returns>
        /// <exception cref="ArgumentException"><paramref name="options"/> is not of the type expected by the transformation.</exception>
        IColorTransform SetOptions(TransformOptions options);

        /// <summary>
        /// Metric used to compare colors (from <see cref="Options"/>).
        /// </summary>
        ColorDistanceEvaluationMode ColorDistanceEvaluationMode { get; }

        //------------------------------------------------------------
        /// <summary>
        /// Runs the transformation: loads the images, validates the options, transforms, dithers and evaluates the error.
        /// <para/>
        /// Errors and cancellation are reported in the result, never thrown.
        /// </summary>
        /// <param name="sourceImage">Image to transform</param>
        /// <param name="referenceImage">Dithering target and error reference; the source image when null</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>The result; on success <see cref="ColorTransformResult.DataOut"/> is the final image.</returns>
        ColorTransformResult Process(ImageData sourceImage, ImageData? referenceImage = null, CancellationToken token = default);

        /// <summary>
        /// Runs <see cref="Process"/> on a worker thread.
        /// </summary>
        Task<ColorTransformResult> ProcessAsync(ImageData sourceImage, ImageData? referenceImage = null, CancellationToken token = default);

        //------------------------------------------------------------
        /// <summary>
        /// Source image of the last run.
        /// </summary>
        ImageData ImageSource { get; }
        /// <summary>
        /// Reference image of the last run (the source image when not specified).
        /// </summary>
        ImageData ImageReference { get; }
        /// <summary>
        /// Output image of the last successful run.
        /// </summary>
        ImageData ImageOutput { get; }
        /// <summary>
        /// Source color to output color map of the last run (empty for the transformations that do not use a map).
        /// </summary>
        ColorTransformationMap TransformationMap { get; }
        /// <summary>
        /// Mean color error between the reference and the output image of the last run; NaN when not computed.
        /// </summary>
        double TransformationError { get; }
        /// <summary>
        /// Duration of the last run, in milliseconds.
        /// </summary>
        double ProcessingTimeMilliseconds { get; }
        /// <summary>
        /// True when the last run skipped the final dithering step: the transformation disabled it
        /// because it dithers by itself or because dithering does not apply in its current mode.
        /// </summary>
        bool BypassDithering { get; }

        //------------------------------------------------------------
        /// <summary>
        /// Raised first, before anything is done: the handler can change the options with <see cref="SetOptions"/>
        /// or cancel the run with the token source of the event data.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? StartProcessing;
        /// <summary>
        /// Raised before the images of the run are loaded (reserved for future use).
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Creating;
        /// <summary>
        /// Raised after the images of the run are loaded (reserved for future use).
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Created;
        /// <summary>
        /// Raised when the options are valid and the transformation starts.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Processing;
        /// <summary>
        /// Raised to report progress and intermediate results, also from nested transformations and tiles.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Progress;
        /// <summary>
        /// Raised before the final dithering, with the undithered image in the result.
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Dithering;
        /// <summary>
        /// Raised at the end of every run, with the final result (also on error or cancellation).
        /// </summary>
        event EventHandler<ColorProcessingEventArgs>? Completed;
        /// <summary>
        /// Raised when the transformation is being disposed (reserved for future use).
        /// </summary>
        event EventHandler? Destroying;
        /// <summary>
        /// Raised after the transformation has been disposed (reserved for future use).
        /// </summary>
        event EventHandler? Destroyed;
    }
}

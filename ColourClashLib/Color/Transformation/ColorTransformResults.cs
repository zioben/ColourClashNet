using ColourClashNet.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Outcome of a transformation or dithering step: success flag, input/output images, message and exception.
    /// </summary>
    public class ColorTransformResult
    {
        static string sClass = nameof(ColorTransformResult);

        static string validMessage = "Ok";

        /// <summary>
        /// Input image of the step.
        /// </summary>
        public ImageData? DataIn { get; internal set; }
        /// <summary>
        /// Output image of the step.
        /// </summary>
        public ImageData? DataOut { get; internal set; }
        /// <summary>
        /// Optional score or progress value; NaN when not used.
        /// </summary>
        public double ProcessingScore { get; internal set; } = double.NaN;
        /// <summary>
        /// True when the step completed successfully.
        /// </summary>
        public bool IsSuccess { get; internal set; }
        /// <summary>
        /// Result or error message.
        /// </summary>
        public string Message { get; internal set; } = string.Empty;
        /// <summary>
        /// Exception that caused the failure, if any.
        /// </summary>
        public Exception? Exception { get; internal set; }
        /// <summary>
        /// Mean color error between the reference and the output image; NaN when not computed (set on the final result of a run).
        /// </summary>
        public double TransformationError { get; internal set; } = double.NaN;
        /// <summary>
        /// Duration of the run, in milliseconds; NaN when not measured (set on the final result of a run).
        /// </summary>
        public double ProcessingTimeMilliseconds { get; internal set; } = double.NaN;


        /// <summary>
        /// Creates a successful result.
        /// </summary>
        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData, string message, double processingScore) => new ColorTransformResult()
        {
            DataIn = inputData,
            DataOut = outputData,
            ProcessingScore = processingScore,
            IsSuccess = true,
            Message = message
        };


        /// <summary>
        /// Creates a successful result with a custom message.
        /// </summary>
        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData, string message)
            => CreateValidResult(inputData, outputData, message, double.NaN);

        /// <summary>
        /// Creates a successful result with the default "Ok" message.
        /// </summary>
        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData)
            => CreateValidResult(inputData, outputData, validMessage, double.NaN);
        /// <summary>
        /// Creates a successful result carrying a score/progress value.
        /// </summary>
        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData,double progressPercent)
            => CreateValidResult(inputData, outputData, validMessage, progressPercent);
        /// <summary>
        /// Creates a successful result without images.
        /// </summary>
        static public ColorTransformResult CreateValidResult() 
            => CreateValidResult(null, null, validMessage);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, string message, Exception ex) => new ColorTransformResult()
        {
            // Empty images (no pixel data) are not meaningful in an error report
            DataIn = inputData?.matrix != null ? inputData : null,
            DataOut = outputData?.matrix != null ? outputData : null,
            Message = message,
            Exception = ex
        };
        /// <summary>
        /// Creates a failed result without exception.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, string message) => CreateErrorResult(inputData,outputData, message, null);
        /// <summary>
        /// Creates a failed result whose message is taken from the exception.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, Exception ex) => CreateErrorResult(inputData, outputData,$"Exception : {ex?.Message ?? "null"}" , ex);
        /// <summary>
        /// Creates a failed result without images.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(string message, Exception ex) => CreateErrorResult(null,null, message, ex);
        /// <summary>
        /// Creates a failed result from an exception.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(Exception ex) => CreateErrorResult($"Exception : {ex?.Message ?? "null"}", ex);
        /// <summary>
        /// Creates a failed result with a message only.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(String message) => CreateErrorResult(message, null);

        /// <summary>
        /// Creates a failed result whose message identifies the failing operation as <c>sClass.sMethod : message</c>.
        /// <para/>
        /// When <paramref name="message"/> is empty, the exception message is used.
        /// </summary>
        /// <param name="sClass">Class of the failing operation</param>
        /// <param name="sMethod">Method of the failing operation</param>
        /// <param name="inputData">Input image of the operation, if available</param>
        /// <param name="outputData">Partial output image, if available</param>
        /// <param name="message">Error description</param>
        /// <param name="ex">Exception that caused the error, if any</param>
        static public ColorTransformResult CreateErrorResult(string sClass, string sMethod, ImageData? inputData, ImageData? outputData, string? message, Exception? ex)
        {
            var text = string.IsNullOrWhiteSpace(message)
                ? (ex != null ? $"Exception : {ex.Message}" : "Unknown error")
                : message;
            return CreateErrorResult(inputData, outputData, $"{sClass}.{sMethod} : {text}", ex);
        }

        /// <summary>
        /// Creates a failed result without exception, identifying the failing operation.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(string sClass, string sMethod, ImageData? inputData, ImageData? outputData, string? message)
            => CreateErrorResult(sClass, sMethod, inputData, outputData, message, null);

        /// <summary>
        /// Creates a failed result from an exception, identifying the failing operation.
        /// </summary>
        static public ColorTransformResult CreateErrorResult(string sClass, string sMethod, ImageData? inputData, ImageData? outputData, Exception? ex)
            => CreateErrorResult(sClass, sMethod, inputData, outputData, (string?)null, ex);

        /// <summary>
        /// Creates a failed result for an operation whose nested step failed.
        /// <para/>
        /// The message chains the operation description with the nested message; output image and
        /// exception are taken from <paramref name="innerResult"/>. A null inner result is reported as such.
        /// </summary>
        /// <param name="sClass">Class of the failing operation</param>
        /// <param name="sMethod">Method of the failing operation</param>
        /// <param name="inputData">Input image of the operation, if available</param>
        /// <param name="message">Description of the failed step</param>
        /// <param name="innerResult">Result of the nested step</param>
        static public ColorTransformResult CreateErrorResultFrom(string sClass, string sMethod, ImageData? inputData, string message, ColorTransformResult? innerResult)
            => CreateErrorResult(
                sClass,
                sMethod,
                inputData,
                innerResult?.DataOut,
                innerResult == null ? $"{message} : no result" : $"{message} : {innerResult.Message}",
                innerResult?.Exception);

    }

    /// <summary>
    /// Event data raised by <see cref="IColorTransform"/> during processing.
    /// </summary>
    public class ColorProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// Current or final result; null in the events raised before the transformation (start, loading, processing).
        /// </summary>
        public ColorTransformResult ProcessingResults { get; init; }
        /// <summary>
        /// Transformation that raised the event.
        /// </summary>
        public IColorTransform Transformation { get; init; }
        /// <summary>
        /// Token source linked to the processing: cancel it to abort the transformation.
        /// </summary>
        public CancellationTokenSource? CancellationTokenSource { get; init; }

        /// <summary>
        /// Current transformation map.
        /// </summary>
        public ColorTransformationMap? TransformationMap { get; init; }

        /// <summary>
        /// Completion percentage; NaN when unknown.
        /// </summary>
        public double CompletedPercent { get; set; } =  double.NaN;


    }
}

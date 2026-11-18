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
            DataIn = inputData,
            DataOut = outputData,
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

    }

    /// <summary>
    /// Event data raised by <see cref="ColorTransformInterface"/> during processing.
    /// </summary>
    public class ColorProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// Current or final result; null when processing starts.
        /// </summary>
        public ColorTransformResult ProcessingResults { get; init; }
        /// <summary>
        /// Transformation that raised the event.
        /// </summary>
        public ColorTransformInterface ColorTransformInterface { get; init; }
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

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
    public class ColorTransformResult
    {
        static string sClass = nameof(ColorTransformResult);

        static string validMessage = "Ok";

        public ImageData? DataIn { get; internal set; }
        public ImageData? DataOut { get; internal set; }
        public double ProcessingScore { get; internal set; } = double.NaN;
        public bool IsSuccess { get; internal set; }
        public string Message { get; internal set; } = string.Empty;
        public Exception? Exception { get; internal set; }


        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData, string message, double processingScore) => new ColorTransformResult()
        {
            DataIn = inputData,
            DataOut = outputData,
            ProcessingScore = processingScore,
            IsSuccess = true,
            Message = message
        };


        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData, string message)
            => CreateValidResult(inputData, outputData, message, double.NaN);

        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData)
            => CreateValidResult(inputData, outputData, validMessage, double.NaN);
        static public ColorTransformResult CreateValidResult(ImageData? inputData, ImageData? outputData,double progressPercent)
            => CreateValidResult(inputData, outputData, validMessage, progressPercent);
        static public ColorTransformResult CreateValidResult() 
            => CreateValidResult(null, null, validMessage);

        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, string message, Exception ex) => new ColorTransformResult()
        {
            DataIn = inputData,
            DataOut = outputData,
            Message = message,    
            Exception = ex
        };
        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, string message) => CreateErrorResult(inputData,outputData, message, null);
        static public ColorTransformResult CreateErrorResult(ImageData inputData, ImageData outputData, Exception ex) => CreateErrorResult(inputData, outputData,$"Exception : {ex?.Message ?? "null"}" , ex);
        static public ColorTransformResult CreateErrorResult(string message, Exception ex) => CreateErrorResult(null,null, message, ex);
        static public ColorTransformResult CreateErrorResult(Exception ex) => CreateErrorResult($"Exception : {ex?.Message ?? "null"}", ex);
        static public ColorTransformResult CreateErrorResult(String message) => CreateErrorResult(message, null);

    }

    public class ColorProcessingEventArgs : EventArgs
    {
        public ColorTransformResult ProcessingResults { get; init; }
        public ColorTransformInterface ColorTransformInterface { get; init; }
        public CancellationTokenSource? CancellationTokenSource { get; init; }

        public ColorTransformationMap? TransformationMap { get; init; }

        public double CompletedPercent { get; set; } =  double.NaN;


    }
}

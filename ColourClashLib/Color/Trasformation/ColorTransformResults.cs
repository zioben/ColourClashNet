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
    public class ColorTransformResults
    {
        static string sClass = nameof(ColorTransformResults);

        static string sValidMessage = "Ok";

        public ImageData? DataIn { get; internal set; }
        public ImageData? DataOut { get; internal set; }
        public double ProcessingScore { get; internal set; } = double.NaN;
        public bool IsSuccess { get; internal set; }
        public string Message { get; internal set; } = string.Empty;
        public Exception? Exception { get; internal set; }


        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData, string sMessage, double processingScore) => new ColorTransformResults()
        {
            DataIn = inputData,
            DataOut = outputData,
            ProcessingScore = processingScore,
            IsSuccess = true,
            Message = sMessage
        };


        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData, string sMessage)
            => CreateValidResult(inputData, outputData, sMessage, double.NaN);

        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData)
            => CreateValidResult(inputData, outputData, sValidMessage, double.NaN);
        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData,double dProgressPercent)
            => CreateValidResult(inputData, outputData, sValidMessage, dProgressPercent);
        static public ColorTransformResults CreateValidResult() 
            => CreateValidResult(null, null, sValidMessage);

        static public ColorTransformResults CreateErrorResult(ImageData inputData, ImageData outputData, string sMessage, Exception ex) => new ColorTransformResults()
        {
            DataIn = inputData,
            DataOut = outputData,
            Message = sMessage,    
            Exception = ex
        };
        static public ColorTransformResults CreateErrorResult(ImageData inputData, ImageData outputData, string sMessage) => CreateErrorResult(inputData,outputData, sMessage, null);
        static public ColorTransformResults CreateErrorResult(ImageData inputData, ImageData outputData, Exception ex) => CreateErrorResult(inputData, outputData,$"Exception : {ex?.Message ?? "null"}" , ex);
        static public ColorTransformResults CreateErrorResult(string sMessage, Exception ex) => CreateErrorResult(null,null, sMessage, ex);
        static public ColorTransformResults CreateErrorResult(Exception ex) => CreateErrorResult($"Exception : {ex?.Message ?? "null"}", ex);
        static public ColorTransformResults CreateErrorResult(String sMessage) => CreateErrorResult(sMessage, null);

    }

    public class ColorProcessingEventArgs : EventArgs
    {
        public ColorTransformResults ProcessingResults { get; init; }
        public ColorTransformInterface ColorTransformInterface { get; init; }
        public CancellationTokenSource? CancellationTokenSource { get; init; }

        public ColorTransformationMap? TransformationMap { get; init; }

        public double CompletedPercent { get; set; } =  double.NaN;


    }
}

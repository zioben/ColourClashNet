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

        public ImageData? DataIn { get; internal set; }
        //public int[,]? DataProcessed { get; internal set; }
        public ImageData? DataOut { get; internal set; }
        public double ProcessingError { get; internal set; } = double.NaN;

        public bool ProcessingValid { get; internal set; }
        public string Message { get; internal set; } = string.Empty;

        public Exception? Exception { get; internal set; }


        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData, double processingError) => new ColorTransformResults()
        {
            DataIn = inputData,
            DataOut = outputData,
            ProcessingError = processingError,
            ProcessingValid = true,
            Message = "Ok",
        };

        static public ColorTransformResults CreateValidResult(ImageData? inputData, ImageData? outputData) => new ColorTransformResults()
        {
            DataIn = inputData,
            DataOut = outputData,
            ProcessingValid = true,
            Message = "Ok",
        };
        //   static public ColorTransformResults CreateValidResult(int[,]? sourceData, int[,]? outputData) => CreateValidResult(sourceData, null, outputData);

        static public ColorTransformResults CreateValidResult() => CreateValidResult(null, null);

        static public ColorTransformResults CreateErrorResult(string sMessage, Exception ex) => new ColorTransformResults()
        {
             Message = sMessage,    
             Exception = ex
        };
        static public ColorTransformResults CreateErrorResult(Exception ex) => CreateErrorResult(ex?.Message, ex);
        static public ColorTransformResults CreateErrorResult(String sMessage) => CreateErrorResult(sMessage, null);

    }

    public class ColorTransformEventArgs : EventArgs
    {
        public ColorTransformResults ProcessingResults { get; init; }
        public string? Message { get; internal set; }
        public int[,]? TempImage { get; internal set; }
        public ColorTransformInterface? ColorTransformInterface { get; init; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public ColorTransformationMap? TransformationMap { get; set; }

        public double CompletedPercent { get; set; } =  double.NaN;


    }
}

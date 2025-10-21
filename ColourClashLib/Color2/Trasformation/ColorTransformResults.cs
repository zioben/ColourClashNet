using ColourClashNet.Color;
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

        public int[,]? DataSource { get; internal set; }
        public int[,]? DataProcessed { get; internal set; }
        public int[,]? DataOut { get; internal set; }
        public double DataError { get; internal set; } = double.NaN;

        public bool Valid { get; internal set; }
        public string Message { get; internal set; } = string.Empty;

        public Exception? Exception { get; internal set; }

        static public ColorTransformResults CreateValidResult(int[,] sourceData, int[,] processedData) => new ColorTransformResults()
        {
            DataSource = sourceData,
            DataProcessed = processedData,
            Valid = true, 
            Message = "Ok",
        };
    }

    public class ColorTransformEventArgs : EventArgs
    {
        public ColorTransformResults ProcessingResults { get; init; }
        public string? Message { get; internal set; }
        public int[,] TempImage { get; internal set; }
        public ColorTransformInterface? ColorTransformInterface { get; init; }
        public CancellationTokenSource? CancellationTokenSource { get; set; }
        public ColorTransformationMap? TransformationMap { get; set; }


    }
}

using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering
{
    /// <summary>
    /// Base class for Dithering algorithms
    /// </summary>
    public abstract partial class DitherBase : DitherInterface
    {
        static string sClass = nameof(DitherBase);
        public string Description { get; protected init; }
        public ColorDithering Type { get; protected init; }

        public double DitheringStrenght { get; set; } = 1.0;

        public abstract bool Create();


        public abstract Task<int[,]?> DitherAsync(int[,]? oDataSource, int[,]? oDataProcessed, Palette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken);

        //public int[,]? Dither(int[,]? oDataSource, int[,]? oDataProcessed, Palette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken? oToken)
        //{
        //    var cts = new CancellationTokenSource();
        //    return DitherAsync(oDataSource,oDataProcessed,oDataProcessedPalette,eDistanceMode,cts.Token).GetAwaiter().GetResult();
        //}
    }
}

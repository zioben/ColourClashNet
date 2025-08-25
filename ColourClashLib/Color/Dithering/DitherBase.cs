using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Dithering
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


        public abstract int[,]? Dither(int[,]? oDataSource, int[,]? oDataProcessed, ColorPalette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken oToken);

 
    }
}

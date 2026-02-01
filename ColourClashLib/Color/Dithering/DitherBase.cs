using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
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
        public abstract DitherInterface Create();
        public abstract ColorTransformResult Dither(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token=default);
    }
}

using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// IBM VGA 256 color mode: quantizes the image to RGB666, then reduces it to <see cref="MaxColorUsed"/> colors by clustering.
    /// </summary>
    public class ColorTransformVGA : ColorTransformBase
    {
        static string sC = nameof(ColorTransformVGA);

        /// <summary>
        /// Creates the VGA transformation.
        /// </summary>
        public ColorTransformVGA()
        {
            Type = ColorTransformType.ColorReductionVGA;
            Description = "Reduce colors to a VGA 256 color palette (RGB666)";
        }


        /// <summary>
        /// Maximum number of output colors.
        /// </summary>
        public int MaxColorUsed
        {
            get => config.MaxColorsWanted;
            set => config.MaxColorsWanted = value;
        }   

        /// <summary>
        /// Sets the maximum number of output colors (fluent).
        /// </summary>
        public ColorTransformVGA WithProperties(int maxColors)
        {
            MaxColorUsed = maxColors;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of output colors from a configuration (fluent).
        /// </summary>
        public ColorTransformVGA WithProperties(ColorTransformConfig cfg) => WithProperties(cfg.MaxColorsWanted);



        /// <summary>
        /// Runs the RGB666 quantization followed by the clustering reduction.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);

            var quantization = ForwardEventsFrom(new ColorTransformQuantization());
            quantization.WithQuantization(ColorQuantizationMode.RGB666)
                         .WithReferencePalette(ReferencePalette)
                         .WithDithering( ColorDithering.None);
            var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
            if (resultQuantized == null || !resultQuantized.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : RGB666 quantization failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : RGB666 quantization failed", resultQuantized);
            }

            var cluster = ForwardEventsFrom(new ColorTransformClustering());
            cluster.WithProcessingParams(MaxColorUsed, 10, ColorSelectionMode.EvaluateColorMean)
            .WithColorDistanceEvaluationMode(config)
            .WithDithering(config)
            .WithReferencePalette(config);

            var clusterResult = cluster.CreateAndProcessColors(resultQuantized.DataOut, token);
            if (clusterResult == null || !clusterResult.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : clustering failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : clustering failed", clusterResult);
            }
            return ColorTransformResult.CreateValidResult(ImageSource, clusterResult.DataOut);
        }

    }
}

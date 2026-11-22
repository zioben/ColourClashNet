using ColourClashNet.Color.Dithering;
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
    /// IBM VGA 256 color mode: quantizes the image to RGB666, then reduces it with <see cref="VgaOptions.Reduction"/>
    /// (clustering to 256 colors by default).
    /// </summary>
    public class ColorTransformVGA : ColorTransformBase<VgaOptions>
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
        /// Runs the RGB666 quantization followed by the palette reduction.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {
            string sM = nameof(ExecuteTransform);
            LogDebug(sC, sM, $"{Type} : RGB666 quantization, then {Options.Reduction.TransformType} to {Options.Reduction.MaxColors} colors");

            var quantization = ForwardEventsFrom(new ColorTransformQuantization
            {
                Options = new QuantizationOptions { Mode = ColorQuantizationMode.RGB666, DistanceMode = Options.DistanceMode },
            });
            var resultQuantized = quantization.CreateAndProcessColors(ImageSource, token);
            if (resultQuantized == null || !resultQuantized.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : RGB666 quantization failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : RGB666 quantization failed", resultQuantized);
            }

            // The nested reduction inherits metric and user palette; dithering is applied once, by this transformation
            var reductionOptions = Options.InheritCommon(Options.Reduction) with { Dithering = DitherOptions.None };
            var reduction = ForwardEventsFrom(ColorTransformInternal.Alloc(reductionOptions));

            LogTrace(sC, sM, $"{Type} : quantized image has {resultQuantized.DataOut.Colors} colors");
            var reductionResult = reduction.CreateAndProcessColors(resultQuantized.DataOut, token);
            if (reductionResult == null || !reductionResult.IsSuccess)
            {
                LogMan.Error(sC, sM, $"{Type} : palette reduction failed");
                return ColorTransformResult.CreateErrorResultFrom(sC, sM, ImageSource, $"{Type} : palette reduction failed", reductionResult);
            }
            return ColorTransformResult.CreateValidResult(ImageSource, reductionResult.DataOut);
        }

    }
}

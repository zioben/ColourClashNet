using ColourClashNet.Color.Conversion;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    /// <summary>
    /// Adjusts hue, saturation and brightness in HSV space. Not a color reduction: dithering is always bypassed.
    /// </summary>
    public class ColorTransformLumSat : ColorTransformBase<HsvOptions>
    {
        static readonly string sC = nameof(ColorTransformLumSat);

        /// <summary>
        /// Creates the HSV adjustment.
        /// </summary>
        public ColorTransformLumSat() 
        {
            Type = ColorTransformType.ColorReductionSaturation;
            Description = "Expand color chrominance";
        }



    


        /// <summary>
        /// Applies the adjustments to every pixel in parallel; non-color pixels become black.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);

            var processed = new int[ImageSource.Rows, ImageSource.Columns];
            var options = Options;
            BypassDithering = true;

            try
            {
                Parallel.For(0, ImageSource.Rows, new ParallelOptions { CancellationToken = token }, r =>
                {
                    for (int c = 0; c < ImageSource.Columns; c++)
                    {
                        var hsv = HSV.CreateFromIntRGB(ImageSource.matrix[r, c]);
                        if (hsv.IsValid)
                        {
                            // Wrap hue into [0,360): HSV.ToIntRGB expects a normalized hue
                            hsv.H = (float)((((hsv.H + options.HueShift) % 360.0) + 360.0) % 360.0);
                            hsv.S = (float)Math.Min(100, hsv.S * options.SaturationFactor);
                            hsv.V = (float)Math.Min(100, hsv.V * options.BrightnessFactor);
                            processed[r, c] = hsv.ToIntRGB();
                        }
                    }
                    token.ThrowIfCancellationRequested();
                });
            }
            catch (OperationCanceledException ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : operation cancelled", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : operation cancelled", ex);
            }
            catch (Exception ex)
            {
                LogMan.Exception(sC, sM, $"{Type} : HSV adjustment failed", ex);
                return ColorTransformResult.CreateErrorResult(sC, sM, ImageSource, null, $"{Type} : HSV adjustment failed", ex);
            }

            return ColorTransformResult.CreateValidResult(ImageSource, new ImageData().Create(processed));

        }
    }
}
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
        /// Applies the adjustments to every pixel in parallel; non-color pixels (e.g. background tags) are kept unchanged.
        /// <para/>
        /// The hue shift is a rotation on the color wheel: any value is wrapped into [0, 360),
        /// so shifting by 360 (or 0) gives back the original hues.
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
                        int rgb = ImageSource.matrix[r, c];
                        var hsv = HSV.CreateFromIntRGB(rgb);
                        if (!hsv.IsValid)
                        {
                            processed[r, c] = rgb;
                            continue;
                        }
                        hsv.H = (float)WrapHue(hsv.H + options.HueShift);
                        hsv.S = (float)Math.Min(100, hsv.S * options.SaturationFactor);
                        hsv.V = (float)Math.Min(100, hsv.V * options.BrightnessFactor);
                        processed[r, c] = hsv.ToIntRGB();
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

        /// <summary>
        /// Wraps a hue into [0, 360).
        /// <para/>
        /// The float conversion can round a value just below 360 up to 360: it is folded back to 0.
        /// </summary>
        public static double WrapHue(double hue)
        {
            double wrapped = ((hue % 360.0) + 360.0) % 360.0;
            return (float)wrapped >= 360f ? 0.0 : wrapped;
        }
    }
}
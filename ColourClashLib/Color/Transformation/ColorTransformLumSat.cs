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
    public class ColorTransformLumSat : ColorTransformBase
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
        /// Hue rotation in degrees.
        /// </summary>
        public double HueShift 
        { 
            get => config.HsvHueShift; 
            set => config.HsvHueShift = value;
        }
        /// <summary>
        /// Saturation multiplier; the result is capped at 100%.
        /// </summary>
        public double SaturationMultFactor 
        { 
            get => config.HsvSaturationMultFactor;
            set => config.HsvSaturationMultFactor = value;
        }
        /// <summary>
        /// Brightness multiplier; the result is capped at 100%.
        /// </summary>
        public double BrightnessMultFactor 
        {
            get => config.HsvBrightnessMultFactor;
            set => config.HsvBrightnessMultFactor = value;
        }


        /// <summary>
        /// Sets the HSV adjustments; <paramref name="hueShift"/> is clamped to [-180, 180] (fluent).
        /// </summary>
        public ColorTransformLumSat WithHueShiftValue(double hueShift, double saturationMultFactor, double brightnessMultFactor)
        {
            HueShift = ColourTools.Clamp(hueShift, -180, 180);
            SaturationMultFactor = saturationMultFactor;
            BrightnessMultFactor = brightnessMultFactor;
            return this;
        }
    
        /// <summary>
        /// Sets the HSV adjustments from a configuration (fluent).
        /// </summary>
        public ColorTransformLumSat WithHueShiftValue(ColorTransformConfig cfg) => WithHueShiftValue(cfg.HsvHueShift, cfg.HsvSaturationMultFactor, cfg.HsvBrightnessMultFactor);


        /// <summary>
        /// Applies the adjustments to every pixel in parallel; non-color pixels become black.
        /// </summary>
        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);

            var processed = new int[ImageSource.Rows, ImageSource.Columns];
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
                            hsv.H = (float)((((hsv.H + HueShift) % 360.0) + 360.0) % 360.0);
                            hsv.S = (float)Math.Min(100, hsv.S * SaturationMultFactor);
                            hsv.V = (float)Math.Min(100, hsv.V * BrightnessMultFactor);
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
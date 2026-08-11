using ColourClashNet.Color.Conversion;
using ColourClashNet.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformLumSat : ColorTransformBase
    {
        static readonly string sC = nameof(ColorTransformLumSat);

        public ColorTransformLumSat() 
        {
            Type = ColorTransformType.ColorReductionEga;
            Description = "Expand color crominance";
        }

        public double HueShift { get; set; } = 0;
        public double SaturationMultFactor { get; set; } = 1.0;
        public double BrightnessMultFactor { get; set; } = 1.0;


        public ColorTransformLumSat WithHueShiftValue(double hueShift, double saturationMultFactor, double brightnessMultFactor)
        {
            HueShift = ColourTools.Clamp(hueShift, -180, 180);
            SaturationMultFactor = saturationMultFactor;
            BrightnessMultFactor = brightnessMultFactor;
            return this;
        }
    
        public ColorTransformLumSat WithHueShiftValue(ColorTransformConfig cfg) => WithHueShiftValue(cfg.HsvHueShift, cfg.HsvSaturationMultFactor, cfg.HsvBrightnessMultFactor);

        public override ColorTransformInterface SetProperties(ColorTransformConfig cfg)
        {
            base.SetProperties(cfg);
            return WithHueShiftValue(cfg);
        }

        // Not Needed
        //protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)

        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);

            var oProcessed = new int[ImageSource.Rows, ImageSource.Columns];
            BypassDithering = true;

            // More Performant without Parallel ?
            //for (int r = 0; r < SourceData.Rows; r++ )
            Parallel.For(0, ImageSource.Rows, new ParallelOptions { CancellationToken = token }, r =>
            {
                for (int c = 0; c < ImageSource.Columns; c++)
                {
                    var hsv = HSV.CreateFromIntRGB(ImageSource.matrix[r, c]);
                    if (hsv.IsValid)
                    {
                        hsv.H = hsv.H + (float)HueShift;
                        hsv.S = (float)Math.Min(100, hsv.S * SaturationMultFactor);
                        hsv.V = (float)Math.Min(100, hsv.V * BrightnessMultFactor);
                        oProcessed[r, c] = hsv.ToIntRGB();
                    }
                }
                token.ThrowIfCancellationRequested();
            });
            //}

            return ColorTransformResult.CreateValidResult(ImageSource, new ImageData().Create(oProcessed));

        }
    }
}
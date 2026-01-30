using ColourClashLib.Color;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
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


        internal protected override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.HsvHueShift:
                    HueShift = Clamp(oValue, -180, 180);
                    break;
                case ColorTransformProperties.HsvBrightnessMultFactor:
                    BrightnessMultFactor = ToDouble(oValue);
                    break;
                case ColorTransformProperties.HsvSaturationMultFactor:
                    SaturationMultFactor = ToDouble(oValue);
                    break;
                default:
                    break;
            }
            return this;
        }

        // Not Needed
        //protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)

        protected override ColorTransformResult ExecuteTransform(CancellationToken token = default)
        {

            string sM = nameof(ExecuteTransform);

            var oProcessed = new int[SourceData.Rows, SourceData.Columns];
            BypassDithering = true;

            // More Performant without Parallel ?
            //for (int r = 0; r < SourceData.Rows; r++ )
            Parallel.For(0, SourceData.Rows, new ParallelOptions { CancellationToken = token }, r =>
            {
                for (int c = 0; c < SourceData.Columns; c++)
                {
                    var hsv = HSV.CreateFromIntRGB(SourceData.matrix[r, c]);
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

            return ColorTransformResult.CreateValidResult(SourceData, new ImageData().Create(oProcessed));

        }
    }
}
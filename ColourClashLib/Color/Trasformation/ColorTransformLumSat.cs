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


        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            base.SetProperty(eProperty, oValue);
            switch (eProperty)
            {
                case ColorTransformProperties.HsvHueShift:
                    if (double.TryParse(oValue?.ToString(), out var h))
                    {
                        HueShift = h;
                    }
                    break;
                case ColorTransformProperties.HsvBrightnessMultFactor:
                    if (double.TryParse(oValue?.ToString(), out var b))
                    {
                        BrightnessMultFactor = b;
                    }
                    break;
                case ColorTransformProperties.HsvSaturationMultFactor:
                    if (double.TryParse(oValue?.ToString(), out var s))
                    {
                        SaturationMultFactor = s;
                    }
                    break;
                default:
                    break;
            }
            return this;
        }

        // Not Needed
        //protected async override Task<ColorTransformResults> CreateTrasformationMapAsync(CancellationToken? oToken)

        protected async override Task<ColorTransformResults> ExecuteTransformAsync( CancellationToken oToken=default)
        {
            return await Task.Run(() =>
            {
                string sM = nameof(ExecuteTransformAsync);
             
                var oProcessed = new int[SourceData.Rows, SourceData.Columns];
                BypassDithering = true;

                Parallel.For(0, SourceData.Rows, r =>
                {
                    for (int c = 0; c < SourceData.Columns; c++)
                    {
                        var hsv = SourceData.Data[r, c].ToHSV();
                        if (hsv.Valid)
                        {
                            hsv.H = hsv.H + (float)HueShift;
                            hsv.S = (float)Math.Min(100, hsv.S * SaturationMultFactor);
                            hsv.V = (float)Math.Min(100, hsv.V * BrightnessMultFactor);
                            oProcessed[r, c] = hsv.ToIntRGB();
                        }
                    }
                    oToken.ThrowIfCancellationRequested();
                });

                return ColorTransformResults.CreateValidResult(SourceData, new ImageData().Create(oProcessed));
            });
        }

    }
}
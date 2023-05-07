using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformLumSat : ColorTransformReductionPalette
    {


        public ColorTransformLumSat() 
        {
            Name = ColorTransformType.ColorReductionEga;
            Description = "Expand color crominance";
        }

        public double HueShift { get; set; } = 0;
        public double SaturationMultFactor { get; set; } = 1.0;
        public double BrightnessMultFactor { get; set; } = 1.0;


        public override ColorTransformInterface SetProperty(ColorTransformProperties eProperty, object oValue)
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch (eProperty)
            {
                case ColorTransformProperties.HsbHueShift:
                    if (double.TryParse(oValue?.ToString(), out var h))
                    {
                        HueShift = h;
                        return this;
                    }
                    break;
                case ColorTransformProperties.HsvBrightnessMultFactor:
                    if (double.TryParse(oValue?.ToString(), out var b))
                    {
                        BrightnessMultFactor = b;
                        return this;
                    }
                    break;
                case ColorTransformProperties.HsvSaturationMultFactor:
                    if (double.TryParse(oValue?.ToString(), out var s))
                    {
                        SaturationMultFactor = s;
                        return this;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        protected override void CreateTrasformationMap()
        {
        }

        protected override int[,]? ExecuteTransform(int[,]? oSource)
        {
            if (oSource == null)
                return null;

            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            var oRet = new int[R,C];
            BypassDithering = true;

            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    int iRGB = oSource[r, c];
                    var h = (float)(iRGB.ToH() + HueShift);
                    if (h > 360)
                        h -= 360;
                    var s = (float)Math.Min(100, iRGB.ToS() * SaturationMultFactor);
                    var v = (float)Math.Min(100, iRGB.ToV() * BrightnessMultFactor);
                    int oRGB = ColorIntExt.FromHSV(h, s, v);
                    oRet[r, c] = oRGB;
                }
            });
            return oRet;
        }

    }
}
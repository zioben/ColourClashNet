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
            Type = ColorTransform.ColorReductionEga;
            Description = "Expand color crominance";
        }

        public double HueChange { get; set; } = 0;
        public double SaturationFactor { get; set; } = 1.0;
        public double BrightnessFactor { get; set; } = 1.0;

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
                    var h = (float)(iRGB.ToH() + HueChange);
                    if (h > 360)
                        h -= 360;
                    var s = (float)Math.Min(100, iRGB.ToS() * SaturationFactor);
                    var v = (float)Math.Min(100, iRGB.ToV() * BrightnessFactor);
                    int oRGB = ColorIntExt.FromHSV(h, s, v);
                    oRet[r, c] = oRGB;
                }
            });
            return oRet;
        }

    }
}
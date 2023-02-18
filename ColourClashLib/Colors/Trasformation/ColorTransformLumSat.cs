using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformLumSat : ColorTransformToPalette
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
        //    ColorPalette.ToList().ForEach(x =>
        //    {
        //        int r = (int)Math.Min(255, x.ToR() * dLumLevels );
        //        int g = (int)Math.Min(255, x.ToG() * dLumLevels);
        //        int b = (int)Math.Min(255, x.ToB() * dLumLevels);
        //        ColorTransformationMap.Add(x, ColorIntExt.FromRGB(r, g, b));
        //    });
        //}

        protected override int[,]? ExecuteTransform(int[,]? oSource)
        {
            if (oSource == null)
                return null;


            var R = oSource.GetLength(0);
            var C = oSource.GetLength(1);
            //var oH = new float[R*C];
            //var oS = new float[R*C];
            //var oV = new float[R*C];
            //var fS = (float)Math.Max(0, SaturationFactor);
            //var fV = (float)Math.Max(0, BrightnessFactor);
            //int i = 0;
            //foreach( var rgb in oSource) 
            //{
            //    oH[i] = rgb.ToH();
            //    oS[i] = rgb.ToS() * fS;
            //    oV[i] = rgb.ToV() * fV;
            //}
            
            var oRet = new int[R,C];  

            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    int iRGB = oSource[r, c];
                    var h = (float)(iRGB.ToH()+HueChange);
                    if (h > 360)
                        h -= 360;
                    var s = (float)Math.Min(100, iRGB.ToS() *SaturationFactor);
                    var v = (float)Math.Min(100, iRGB.ToV() *BrightnessFactor);
                    oRet[r,c]= ColorIntExt.FromHSV(h,s,v);
                }
            }
            return oRet;
        }

    }
}
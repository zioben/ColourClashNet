using ColourClashNet.Colors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorTransformReductionZxSpectrum : ColorTransformToPalette
    {


        public ColorTransformReductionZxSpectrum()
        {
            Type = ColorTransform.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        protected override void CreateTrasformationMap()
        {
            hashColorsPalette.Clear();
            hashColorsPalette.Add(0x00000000);
            hashColorsPalette.Add(0x000000ee);
            hashColorsPalette.Add(0x00ee0000);
            hashColorsPalette.Add(0x00ee00ee);
            hashColorsPalette.Add(0x0000ee00);
            hashColorsPalette.Add(0x0000eeee);
            hashColorsPalette.Add(0x00eeee00);
            hashColorsPalette.Add(0x00ffffff);
            hashColorsPalette.Add(0x000000ff);
            hashColorsPalette.Add(0x00ff0000);
            hashColorsPalette.Add(0x00ff00ff);
            hashColorsPalette.Add(0x0000ff00);
            hashColorsPalette.Add(0x0000ffff);
            hashColorsPalette.Add(0x00ffff00);
            hashColorsPalette.Add(0x00ffffff);
        }

        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);


            int R = oTmpData.GetLength(0);
            int C = oTmpData.GetLength(1);
            int[,] oRet = new int[R, C];

            //Parallel.For(0, R / 8, r =>
            for( int r =0; r < R/8; r++ )
            {
                for (int c = 0; c < C / 8; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    int[,] oClashIn = new int[8, 8];
                    int[,] oSourceIn = new int[8, 8];
                    for (int rr = 0; rr < 8; rr++)
                    {
                        for (int cc = 0; cc < 8; cc++)
                        {
                            oClashIn[rr, cc] = oTmpData[rr + r * 8, cc + c * 8];
                            oSourceIn[rr, cc] = oDataSource[rr + r * 8, cc + c * 8];
                        }
                    }
                    ColorTransformReductionCluster oFast = new ColorTransformReductionCluster()
                    {
                        ColorsMax = 2,
                        Dithering = Dithering,
                         ColorDistanceEvaluationMode= ColorDistanceEvaluationMode,
                          TrainingLoop = 10
                    };
                    oFast.Create(oClashIn);
                    var oClashOut = oFast.TransformAndDither(oClashIn);
                    for (int rr = 0; rr < 8; rr++)
                    {
                        for (int cc = 0; cc < 8; cc++)
                        {
                            oRet[rr + r * 8, cc + c * 8] = oClashOut[rr, cc];
                        }
                    }
                }
            }
            this.Dithering = new ColorDitherIdentity();
            return oRet;
        }
    }
}
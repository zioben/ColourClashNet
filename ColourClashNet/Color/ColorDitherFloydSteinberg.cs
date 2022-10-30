using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherFloysSteinberg : ColorDitherBase
    {

        public ColorDitherFloysSteinberg()
        {
            Name = "Floyd steinberg dithering";
            Description = "Quantization error diffusion";
        }

        double[,] oBase = { { 0, 0, 7.0/16 }, { 3.0 / 16, 5.0/16, 1.0/16 } };
      
        double[,] BuildNextThMatrix( double[,] oMatrix )
        {
            int iSize = oMatrix.GetLength(0)*2;
            double[,] oMat = new double[iSize, iSize];
            for (int r = 0; r < iSize; r++)
            {
                for (int c = 0; c < iSize; c++)
                {
                    oMat[r, c] = oMatrix[r / 2, c / 2] + 4 * oMatrix[r % 2, c % 2];  
                }
            }
            return oMat;
        }

        public override bool Create()
        {
            return true;            
        }

        void SetChannel(double[,] channel, int r, int c, int iValO, int iValQ)
        {
            var quant_error = iValO - iValQ;
            //channel[r, c] = iValQ;
            channel[r, c + 1] = channel[r, c + 1] + quant_error * 7.0 / 16;
            channel[r + 1, c - 1] = channel[r + 1, c - 1] + quant_error * 3.0 / 16;
            channel[r + 1, c] = channel[r + 1, c] + quant_error * 5.0 / 16;
            channel[r + 1, c + 1] = channel[r + 1, c + 1] + quant_error * 1.0 / 16;
        }

        int CreateColor(double dRO, double dRE, double dGO, double dGE, double dBO, double dBE)
        {
            var iR = (int)Math.Max(0, dRO + dRE);
            var iG = (int)Math.Max(0, dGO + dGE);
            var iB = (int)Math.Max(0, dBO + dBE);
            return ColorIntExt.FromRGB(iR, iG, iB); 
        }

        public override int[,] Dither(int[,] oDataProcessed, List<int> oDataProcessedPalette, int[,] oDataOriginal, ColorDistanceEvaluationMode eDistanceMode)
        {
            if (oDataProcessedPalette == null || oDataProcessedPalette.Count == 0)
            {
                return null;
            }
            if (!Create())
            {
                return null;
            }
            int R = oDataOriginal.GetLength(0);
            int C = oDataOriginal.GetLength(1);
            double[,] oRO = new double[R, C];
            double[,] oGO = new double[R, C];
            double[,] oBO = new double[R, C];
            double[,] oRQ = new double[R, C];
            double[,] oGQ = new double[R, C];
            double[,] oBQ = new double[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var oDatOrig = oDataOriginal[r, c];
                    oRO[r, c] = oDatOrig.ToR();
                    oGO[r, c] = oDatOrig.ToG();
                    oBO[r, c] = oDatOrig.ToB();
                    var oDatQuant = oDataProcessed[r, c];
                    oRQ[r, c] = oDatQuant.ToR();
                    oGQ[r, c] = oDatQuant.ToG();
                    oBQ[r, c] = oDatQuant.ToB();
                }
            }

            var oRT = oRO.Clone() as double[,];
            var oGT = oGO.Clone() as double[,];
            var oBT = oBO.Clone() as double[,];

            var oRet = oDataProcessed.Clone() as  int[,];
            for (int r = 0; r < R-1; r++)
            {
                for (int c = 1; c < C-1; c++)
                {
                    SpreadErrorDiffusion(oRO, oRQ, oRT, r, c);
                    SpreadErrorDiffusion(oGO, oGQ, oGT, r, c);
                    SpreadErrorDiffusion(oBO, oBQ, oBT, r, c);
                }
            }
            var oHashSet = new HashSet<int>();  
            for (int r = 0; r < R; r++)
            {
                for (int c = 1; c < C; c++)
                {
                    var iR = Math.Max(0, oRT[r, c]);
                    var iG = Math.Max(0, oGT[r, c]);
                    var iB = Math.Max(0, oBT[r, c]);
                    var oCol = ColorIntExt.FromRGB(iR, iG, iB);
                    oHashSet.Add(oCol);
                    oRet[r, c] = ColorIntExt.FromRGB(iR, iG, iB);
                }
            }
            
            var oColorTransformation = new Dictionary<int, int>();
            foreach (var col in oHashSet)
            {
                oColorTransformation.Add(col, ColorTransformBase.GetNearestColor(col, oDataProcessedPalette, eDistanceMode));
            }
            var oRet2 = new int[R, C];
            Parallel.For(0, R, r =>
            {
                for (int c = 0; c < C; c++)
                {
                    var col = oRet[r, c];
                    if (col < 0 || !oColorTransformation.ContainsKey(col))
                        oRet2[r, c] = -1;
                    else
                        oRet2[r, c] = oColorTransformation[col];

                }
            });

            return oRet;
            
        }

        private void SpreadErrorDiffusion(double[,] oChannelO, double[,] oChannelQ, double[,] oChannelT, int r, int c)
        {

            var error = oChannelO[r, c] - oChannelQ[r, c];
            oChannelT[r, c + 1] += (error * 7.0 / 16);
            oChannelT[r + 1, c - 1] += (error * 3.0 / 16);
            oChannelT[r + 1, c] += (error * 5.0 / 16);
            oChannelT[r + 1, c + 1] += (error * 1.0 / 16);
        }
    }
}

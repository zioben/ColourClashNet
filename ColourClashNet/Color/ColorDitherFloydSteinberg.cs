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
            double[,] oRP = new double[R, C];
            double[,] oGP = new double[R, C];
            double[,] oBP = new double[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var oDatOrig = oDataOriginal[r, c];
                    oRO[r, c] = oDatOrig.ToR();
                    oGO[r, c] = oDatOrig.ToG();
                    oBO[r, c] = oDatOrig.ToB();
                    var oDataProc = oDataProcessed[r, c];
                    oRP[r, c] = oDataProc.ToR();
                    oGP[r, c] = oDataProc.ToG();
                    oBP[r, c] = oDataProc.ToB();
                }
            }

            var oRT = (double[,])oRO.Clone();
            var oGT = (double[,])oGO.Clone();
            var oBT = (double[,])oBO.Clone();

            var oRet = oDataProcessed.Clone() as  int[,];
            for (int r = 0; r < R-1; r++)
            {
                for (int c = 1; c < C-1; c++)
                {
                    SpreadErrorDiffusion(oRO, oRP, oRT, r, c);
                    SpreadErrorDiffusion(oGO, oGP, oGT, r, c);
                    SpreadErrorDiffusion(oBO, oBP, oBT, r, c);
                }
            }
            var oHashSet = new HashSet<int>();  
            for (int r = 0; r < R; r++)
            {
                for (int c = 1; c < C; c++)
                {
                    var iR = Math.Max(0, oRO[r,c] + oRT[r, c]);
                    var iG = Math.Max(0, oGO[r, c] + oGT[r, c]);
                    var iB = Math.Max(0, oBO[r, c] + oBT[r, c]);
                    var oCol = ColorIntExt.FromRGB(iR, iG, iB);
                    oHashSet.Add(oCol);
                    oRet[r, c] = oCol;
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

            double error = oChannelT[r, c] - oChannelQ[r, c];
            //oChannelT[r, c] = oChannelO[r, c];
            if (error == 0)
                return;
            oChannelT[r, c + 1] += (error * 7.0 / 16);
            oChannelT[r + 1, c - 1] += (error * 3.0 / 16);
            oChannelT[r + 1, c] += (error * 5.0 / 16);
            oChannelT[r + 1, c + 1] += (error * 1.0 / 16);
        }
    }
}

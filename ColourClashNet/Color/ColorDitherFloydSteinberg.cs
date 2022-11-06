using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherFloysSteinberg : ColorDitherErrorDiffusion
    {

        public ColorDitherFloysSteinberg()
        {
            Name = "Floyd Steinberg dithering";
            Description = "Floyd Steinberg quantization error diffusion";
        }


        public override bool Create()
        {
            matErrorDiffusion = new double[,] { { 0, 0, 7.0 / 16 }, { 3.0 / 16, 5.0 / 16, 1.0 / 16 } };
            return true;
        }

        /*
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
            //
            //
            //
            List<double> lPaletteR = new List<double>();
            List<double> lPaletteG = new List<double>();
            List<double> lPaletteB = new List<double>();
            foreach (var col in oDataProcessedPalette)
            {
                lPaletteR.Add(col.ToR());
                lPaletteG.Add(col.ToG());
                lPaletteB.Add(col.ToB());
            }
            //
            //
            //
            double[,] oRO = new double[R, C];
            double[,] oGO = new double[R, C];
            double[,] oBO = new double[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var oDatOrig = oDataOriginal[r, c];
                    oRO[r, c] = oDatOrig.ToR();
                    oGO[r, c] = oDatOrig.ToG();
                    oBO[r, c] = oDatOrig.ToB();
                }
            }
 
            double[,] oRP = new double[R, C];
            double[,] oGP = new double[R, C];
            double[,] oBP = new double[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var oDataProc = oDataProcessed[r, c];
                    oRP[r, c] = oDataProc.ToR();
                    oGP[r, c] = oDataProc.ToG();
                    oBP[r, c] = oDataProc.ToB();
                }
            }

            var oRet = oDataProcessed.Clone() as int[,];
            for (int r = 0; r < R; r++)
            {
                for (int c = 1; c < C; c++)
                {
                    SpreadErrorDiffusionX(oRO, oRP, r, c, R,C, lPaletteR);
                    SpreadErrorDiffusionX(oGO, oGP, r, c, R,C, lPaletteG);
                    SpreadErrorDiffusionX(oBO, oBP, r, c, R,C, lPaletteB);
                }
            }

            var oHashSet = new HashSet<int>();
            for (int r = 0; r < R; r++)
            {
                for (int c = 1; c < C; c++)
                {
                    var iR = Math.Max(0, oRO[r, c]);
                    var iG = Math.Max(0, oGO[r, c]);
                    var iB = Math.Max(0, oBO[r, c]);
                    var oCol = ColorIntExt.FromRGB(iR, iG, iB);
                    oHashSet.Add(oCol);
                    oRet[r, c] = oCol;
                }
            }

            return oRet;

        }

        private void SpreadErrorDiffusionX(double[,] oChannelOrig, double[,] oChannelProc, int r, int c, int R, int C, List<double> oPalette)
        {

            double dOldPixel = Math.Max( 0, Math.Min(255, oChannelOrig[r, c]));

            var dMin = oPalette.Min(X => Math.Abs(X - dOldPixel));
            double dNewPixel = oPalette.FirstOrDefault(X => Math.Abs(X - dOldPixel) == dMin);

            double error = (dOldPixel - dNewPixel);
            oChannelOrig[r, c] = dNewPixel;

            if(  c < C-1 )
                oChannelOrig[r, c + 1] += (error * 7.0 / 16);
            if (r < R - 1)
            {
                oChannelOrig[r + 1, c] += (error * 5.0 / 16);
                if( c > 0 )
                    oChannelOrig[r + 1, c - 1] += (error * 3.0 / 16);
                if(c < C-1)
                    oChannelOrig[r + 1, c + 1] += (error * 1.0 / 16);
            }
        }

        public int[,] DitherXX(int[,] oDataProcessed, List<int> oDataProcessedPalette, int[,] oDataOriginal, ColorDistanceEvaluationMode eDistanceMode)
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
            //
            //
            //
            var oRet = oDataOriginal.Clone() as int[,];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    var iColO = oRet[r, c];
                    var iColP = ColorTransformBase.GetNearestColor(iColO, oDataProcessedPalette, eDistanceMode);

                    var iRO = iColO.ToR();
                    var iGO = iColO.ToG();
                    var iBO = iColO.ToB();
                    var iRP = iColP.ToR();
                    var iGP = iColP.ToG();
                    var iBP = iColP.ToB();
                    var iRE = iRO - iRP;
                    var iGE = iGO - iGP;
                    var iBE = iBO - iBP;
                    oRet[r, c] = iColP;
                    if (c < C - 1)
                    {
                        var iColX = oRet[r, c+1];
                        var iRX = Math.Min(255 ,Math.Max(0, iColX.ToR() + (iRE * 7) / 16));
                        var iGX = Math.Min(255, Math.Max(0, iColX.ToR() + (iGE * 7) / 16));
                        var iBX = Math.Min(255, Math.Max(0, iColX.ToR() + (iBE * 7) / 16));
                        oRet[r, c+1] = ColorIntExt.FromRGB(iRX, iGX, iBX);
                    }
                    if (r < R - 1)
                    {
                        if (c > 0)
                        {
                            var iColX = oRet[r+1, c-1];
                            var iRX = Math.Min(255, Math.Max(0, iColX.ToR() + (iRE * 3) / 16));
                            var iGX = Math.Min(255, Math.Max(0, iColX.ToR() + (iGE * 3) / 16));
                            var iBX = Math.Min(255, Math.Max(0, iColX.ToR() + (iBE * 3) / 16));
                            oRet[r+1, c-1] = ColorIntExt.FromRGB(iRX, iGX, iBX);
                        }
                        if (true)
                        {
                            var iColX = oRet[r+1, c];
                            var iRX = Math.Min(255, Math.Max(0, iColX.ToR() + (iRE * 5) / 16));
                            var iGX = Math.Min(255, Math.Max(0, iColX.ToR() + (iGE * 5) / 16));
                            var iBX = Math.Min(255, Math.Max(0, iColX.ToR() + (iBE * 5) / 16));
                            oRet[r+1, c] = ColorIntExt.FromRGB(iRX, iGX, iBX);
                        }
                        if (c < C - 1)
                        {
                            var iColX = oRet[r+1, c + 1 ];
                            var iRX = Math.Min(255, Math.Max(0, iColX.ToR() + (iRE * 1) / 16));
                            var iGX = Math.Min(255, Math.Max(0, iColX.ToR() + (iGE * 1) / 16));
                            var iBX = Math.Min(255, Math.Max(0, iColX.ToR() + (iBE * 1) / 16));
                            oRet[r+1, c+1] = ColorIntExt.FromRGB(iRX, iGX, iBX);
                        }
                    }

                }
            }

            var oHashSet = new HashSet<int>();
            for (int r = 0; r < R; r++)
            {
                for (int c = 1; c < C; c++)
                {
                    oHashSet.Add(oRet[r,c]);
                }
            }

            return oRet;

        }       
    }*/
    }
}

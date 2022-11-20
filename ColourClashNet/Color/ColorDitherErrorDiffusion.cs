using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract class ColorDitherErrorDiffusion : ColorDitherBase
    {
        static string sClass = nameof(ColorDitherErrorDiffusion);

        protected double[,] matErrorDiffusion = null;

        protected void Normalize(double dN )
        {
            if (matErrorDiffusion == null)
            {
                return;
            }
            if (dN == 0)
            {
                return;
            }
            int R = matErrorDiffusion.GetLength(0);
            int C = matErrorDiffusion.GetLength(1);
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    matErrorDiffusion[r, c] /= dN;
                }
            }
        }

        protected void Normalize()
        {
            if (matErrorDiffusion == null) 
            {
                return;
            }
            var dN = 0.0;
            foreach (var d in matErrorDiffusion)
            {
                dN += d;
            }
            Normalize(dN);
        }



        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, List<int>? oDataProcessedPalette,  ColorDistanceEvaluationMode eDistanceMode)
        {
            string sMethod = nameof(Dither);
            try
            {
                if (oDataProcessedPalette == null || oDataProcessedPalette.Count == 0)
                {
                    Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Invalid input data");
                    return null;
                }
                if (!Create())
                {
                    Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Creation Error");
                    return null;
                }

                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering");

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
                int R = oDataOriginal.GetLength(0);
                int C = oDataOriginal.GetLength(1);
            
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
                        SpreadErrorDiffusion(oRO, oRP, r, c, lPaletteR);
                        SpreadErrorDiffusion(oGO, oGP, r, c, lPaletteG);
                        SpreadErrorDiffusion(oBO, oBP, r, c, lPaletteB);
                    }
                }

                var oHashSet = new HashSet<int>();
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var iR = Math.Max(0, oRO[r, c]);
                        var iG = Math.Max(0, oGO[r, c]);
                        var iB = Math.Max(0, oBO[r, c]);
                        var oCol = ColorIntExt.FromRGB(iR, iG, iB);
                        oHashSet.Add(oCol);
                        oRet[r, c] = oCol;
                    }
                }

                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering completed");
                return oRet;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Exception raised : {ex.Message}");
                return null;
            }
        }

        private void SpreadErrorDiffusion(double[,] oChannelOrig, double[,] oChannelProc, int r, int c, List<double> oPalette)
        {

            double dOldPixel = Math.Max( 0, Math.Min(255, oChannelOrig[r, c]));

            var dMin = oPalette.Min(X => Math.Abs(X - dOldPixel));
            double dNewPixel = oPalette.FirstOrDefault(X => Math.Abs(X - dOldPixel) == dMin);
            oChannelOrig[r, c] = dNewPixel;

            double error = (dOldPixel - dNewPixel);
            if (error == 0)
                return;


            int R = oChannelOrig.GetLength(0);
            int C = oChannelOrig.GetLength(1);
            int RR = matErrorDiffusion.GetLength(0);
            int CC = matErrorDiffusion.GetLength(1);
            int CO = CC / 2;
            for (int rr = 0; rr < RR; rr++)
            {
                int rOffset = rr + r;
                if (rOffset >= R)
                    break;
                for (int cc = 0; cc < CC; cc++)
                {
                    int cOffset = c + cc - CO;
                    if ( cOffset < 0)
                        continue;
                    if ( cOffset >= C)
                        break;
                    oChannelOrig[rOffset, cOffset ] += (error * matErrorDiffusion[rr,cc] * DitheringStrenght);

                }
            }

        }

    }

}

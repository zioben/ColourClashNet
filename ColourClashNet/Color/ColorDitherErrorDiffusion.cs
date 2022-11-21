using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public abstract class ColorDitherErrorDiffusion : ColorDitherBase
    {
        internal class ErrorDiffusion
        {
            internal HashSet<double> hPalette { get; set; }
            internal double[,] oChannelOrig { get; set; }
            internal double[,] oChannelProc { get; set; }

            internal void Create(int R, int C)
            {
                hPalette = new HashSet<double>();
                oChannelOrig = new double[R, C];
                oChannelProc = new double[R, C];
            }

            internal void SpreadErrorDiffusion(double[,] matErrorDiffusion, double DitheringStrenght)
            {

                int R = oChannelOrig.GetLength(0);
                int C = oChannelOrig.GetLength(1);
                int RR = matErrorDiffusion.GetLength(0);
                int CC = matErrorDiffusion.GetLength(1);
                int CO = CC / 2;

                var oMap = new double[256];
                for (int i = 0; i < 256; i++)
                {
                    var dMin = hPalette.Min(X => Math.Abs(X - i));
                    oMap[i] = hPalette.FirstOrDefault(X => Math.Abs(X - i) == dMin);
                }

                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var dOldPixel = Math.Max(0, Math.Min(255, oChannelOrig[r, c]));
                        var dNewPixel = oMap[(int)dOldPixel];                    
                        //int dMin = hPalette.Min(X => Math.Abs(X - dOldPixel));
                        //int dNewPixel = hPalette.FirstOrDefault(X => Math.Abs(X - dOldPixel) == dMin);

                        oChannelOrig[r, c] = dNewPixel;

                        var error = dOldPixel - dNewPixel;
                        if (error == 0)
                            continue;
                        for (int rr = 0; rr < RR; rr++)
                        {
                            int rOffset = rr + r;
                            if (rOffset >= R)
                                break;
                            for (int cc = 0; cc < CC; cc++)
                            {
                                int cOffset = c + cc - CO;
                                if (cOffset < 0)
                                    continue;
                                if (c == CO)
                                    continue;
                                if (cOffset >= C)
                                    break;
                                oChannelOrig[rOffset, cOffset] += (error * matErrorDiffusion[rr, cc] * DitheringStrenght);

                            }
                        }
                    }
                }
            }
        }

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

                int R = oDataOriginal.GetLength(0);
                int C = oDataOriginal.GetLength(1);

                var oErrDiff = new ErrorDiffusion[3];
                for (int i = 0; i < 3; i++)
                {
                    oErrDiff[i] = new ErrorDiffusion();
                    oErrDiff[i].Create(R, C);
                }

                foreach (var col in oDataProcessedPalette)
                {
                    oErrDiff[0].hPalette.Add(col.ToR());
                    oErrDiff[1].hPalette.Add(col.ToG());
                    oErrDiff[2].hPalette.Add(col.ToB());
                }

                Parallel.For(0, R, r =>
                {
                    for (int c = 0; c < C; c++)
                    {
                        var oDatOrig = oDataOriginal[r, c];
                        oErrDiff[0].oChannelOrig[r, c] = oDatOrig.ToR();
                        oErrDiff[1].oChannelOrig[r, c] = oDatOrig.ToG();
                        oErrDiff[2].oChannelOrig[r, c] = oDatOrig.ToB();
                        var oDataProc = oDataProcessed[r, c];
                        oErrDiff[0].oChannelProc[r, c] = oDataProc.ToR();
                        oErrDiff[1].oChannelProc[r, c] = oDataProc.ToG();
                        oErrDiff[2].oChannelProc[r, c] = oDataProc.ToB();
                    }
                });

                Parallel.For(0, 3, RGB =>
                {
                    oErrDiff[RGB].SpreadErrorDiffusion(matErrorDiffusion, DitheringStrenght);
                });

                var oRet = new int[R, C];
                var oHashSet = new HashSet<int>();
                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var iR = Math.Max(0, oErrDiff[0].oChannelOrig[r, c]);
                        var iG = Math.Max(0, oErrDiff[1].oChannelOrig[r, c]);
                        var iB = Math.Max(0, oErrDiff[2].oChannelOrig[r, c]);
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

     



        private void SpreadErrorDiffusion(double[,] oChannelOrig, double[,] oChannelProc, int r, int c, HashSet<double> hPalette)
        {

            double dOldPixel = Math.Max( 0, Math.Min(255, oChannelOrig[r, c]));

            var dMin = hPalette.Min(X => Math.Abs(X - dOldPixel));
            double dNewPixel = hPalette.FirstOrDefault(X => Math.Abs(X - dOldPixel) == dMin);
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

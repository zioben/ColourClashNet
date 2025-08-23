using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Dithering
{
    public abstract class DitherErrorDiffusion : DitherBase
    {

        static string sClass = nameof(DitherErrorDiffusion);

        protected double[,] matErrorDiffusion = null;


        protected void Normalize(double dN)
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



        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, ColorPalette? oDataProcessedPalette, ColorDistanceEvaluationMode eDistanceMode, CancellationToken oToken )
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

                if (ColorDefaults.Trace) 
                    Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering");

                int R = oDataOriginal.GetLength(0);
                int C = oDataOriginal.GetLength(1);
                int RR = matErrorDiffusion.GetLength(0);
                int CC = matErrorDiffusion.GetLength(1);
                int CO = CC / 2;

                var oRO = new double[R, C];
                var oGO = new double[R, C];
                var oBO = new double[R, C];
                var oRP = new double[R, C];
                var oGP = new double[R, C];
                var oBP = new double[R, C];

                Parallel.For(0, R, r =>
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
                    oToken.ThrowIfCancellationRequested();
                });

                var oDataRet = new int[R, C];

                for (int r = 0; r < R; r++)
                {
                    for (int c = 0; c < C; c++)
                    {
                        var OldPixelR = Math.Max(0, Math.Min(255, oRO[r, c]));
                        var OldPixelG = Math.Max(0, Math.Min(255, oGO[r, c]));
                        var OldPixelB = Math.Max(0, Math.Min(255, oBO[r, c]));

                        var OldPixel = ColorIntExt.FromRGB(OldPixelR, OldPixelG, OldPixelB);
                        var NewPixel = ColorIntExt.GetNearestColor(OldPixel, oDataProcessedPalette, eDistanceMode);
                        oDataRet[r, c] = NewPixel;

                        var NewPixelR = NewPixel.ToR();
                        var NewPixelG = NewPixel.ToG();
                        var NewPixelB = NewPixel.ToB();
                        var ErrorR = OldPixelR - NewPixelR;
                        var ErrorG = OldPixelG - NewPixelG;
                        var ErrorB = OldPixelB - NewPixelB;

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
                                oRO[rOffset, cOffset] += ErrorR * matErrorDiffusion[rr, cc] * DitheringStrenght;
                                oGO[rOffset, cOffset] += ErrorG * matErrorDiffusion[rr, cc] * DitheringStrenght;
                                oBO[rOffset, cOffset] += ErrorB * matErrorDiffusion[rr, cc] * DitheringStrenght;
                            }
                        }
                    }
                }

                if (ColorDefaults.Trace) 
                    Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering completed");
                return oDataRet;

            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Exception raised : {ex.Message}");
                return null;
            }
        }





        private void SpreadErrorDiffusion(double[,] oChannelOrig, double[,] oChannelProc, int r, int c, HashSet<double> hPalette)
        {

            double dOldPixel = Math.Max(0, Math.Min(255, oChannelOrig[r, c]));

            var dMin = hPalette.Min(X => Math.Abs(X - dOldPixel));
            double dNewPixel = hPalette.FirstOrDefault(X => Math.Abs(X - dOldPixel) == dMin);
            oChannelOrig[r, c] = dNewPixel;

            double error = dOldPixel - dNewPixel;
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
                    if (cOffset < 0)
                        continue;
                    if (cOffset >= C)
                        break;
                    oChannelOrig[rOffset, cOffset] += error * matErrorDiffusion[rr, cc] * DitheringStrenght;

                }
            }

        }

    }

}

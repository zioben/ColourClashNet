using ColourClashNet.Color;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Imaging;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Dithering;

public abstract class DitherErrorDiffusion : DitherBase
{

    static string sC = nameof(DitherErrorDiffusion);

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


    public override ImageData? Dither(ImageData imageReference, ImageData imageProcessed, ColorDistanceEvaluationMode colorEvaluationMode, CancellationToken token = default)
    {

        string sM = nameof(Dither);
        try
        {

            if (imageReference == null || !imageReference.Valid)
            {
                LogMan.Error(sC, sM, $"{Type} : Invalid reference data");
                return null;
            }
            if (imageProcessed == null || !imageProcessed.Valid)
            {
                LogMan.Error(sC, sM, $"{Type} : Invalid reduced data");
                return null;
            }
            if (!Create())
            {
                LogMan.Error(sC, sM, $"{Type} : Creation Error");
                return null;
            }

            var oDataOriginal = imageReference.DataX;
            var oDataProcessed = imageProcessed.DataX;
            var oDataProcessedPalette = imageProcessed.ColorPalette;

            LogMan.Trace(sC, sM, $"{Type} : Dithering");

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
                token.ThrowIfCancellationRequested();
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
            });

            var oDataOut = new int[R, C];

            var dStrenght = DitheringStrenght;// / 100.0;

            for (int r = 0; r < R; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < C; c++)
                {
                    var OldPixelR = Math.Max(0, Math.Min(255, oRO[r, c]));
                    var OldPixelG = Math.Max(0, Math.Min(255, oGO[r, c]));
                    var OldPixelB = Math.Max(0, Math.Min(255, oBO[r, c]));

                    var OldPixel = ColorIntExt.FromRGB(OldPixelR, OldPixelG, OldPixelB);
                    var NewPixel = ColorIntExt.GetNearestColor(OldPixel, oDataProcessedPalette, colorEvaluationMode);
                    oDataOut[r, c] = NewPixel;

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
                            oRO[rOffset, cOffset] += ErrorR * matErrorDiffusion[rr, cc] * dStrenght;
                            oGO[rOffset, cOffset] += ErrorG * matErrorDiffusion[rr, cc] * dStrenght;
                            oBO[rOffset, cOffset] += ErrorB * matErrorDiffusion[rr, cc] * dStrenght;
                        }
                    }
                }
            }

            for (int r = 0; r < R; r++)
            {
                token.ThrowIfCancellationRequested();
                for (int c = 0; c < C; c++)
                {
                    if (oDataProcessed[r, c] < 0)
                    {
                        oDataOut[r, c] = oDataProcessed[r, c];
                    }
                }
            }

            LogMan.Trace(sC, sM, $"{Type} : Dithering completed");
            return new ImageData().Create(oDataOut);
        }
        catch (Exception ex)
        {
            LogMan.Exception(sC, sM, $"{Type}", ex);
            return null;
        }
    }
}

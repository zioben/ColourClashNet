using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ColourClashLib;
using ColourClashLib.Color;
using ColourClashNet.Colors;

namespace ColourClashNet.Colors.Dithering
{
    public class DitherOrdered : DitherBase
    {
        static string sClass = nameof(DitherOrdered);
        public new ColorDithering Type
        {
            get
            {
                if (Size == 2)
                    return ColorDithering.Ordered_2x2;
                else if (Size <= 4)
                    return ColorDithering.Ordered_4x4;
                else
                    return ColorDithering.Ordered_8x8;
            }
        }

        int iSize = 2;
        public int Size
        {
            get
            {
                return iSize;
            }
            set
            {
                iSize = Math.Max(2, value);
            }
        }


        public DitherOrdered()
        {
            Description = "2^P Scalable ordered dithering";
        }



        double[,] oBase = { { 0, 2 }, { 3, 1 } };
        double dDivider = 4;
        double[,] oThMat = null;

      

        double[,] BuildNextThMatrix(double[,] oMatrix)
        {
            int iSize = oMatrix.GetLength(0) * 2;
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
            int pow = (int)Math.Log2(iSize);
            int len = (int)Math.Pow(2, pow);
            oThMat = oBase.Clone() as double[,];
            dDivider = 4;
            for (int i = 1; i < pow; i++)
            {
                oThMat = BuildNextThMatrix(oThMat);
                dDivider *= 4;
            }
            int iS = oThMat.GetLength(0);
            double dMean = 0;
            for (int r = 0; r < iS; r++)
            {
                for (int c = 0; c < iS; c++)
                {
                    oThMat[r, c] += 1;
                    oThMat[r, c] /= dDivider;
                    oThMat[r, c] -= 0.5;
                    dMean += oThMat[r, c];
                }
            }
            //dMean /= iS * iS;
            //for (int r = 0; r < iS; r++)
            //{
            //    for (int c = 0; c < iS; c++)
            //    {
            //        oThMat[r, c] -= dMean;
            //    }
            //}

            return true;
        }

        public override int[,]? Dither(int[,]? oDataOriginal, int[,]? oDataProcessed, ColorPalette? oDataProcessedPalette, ColorDistanceEvaluationMode eMode, CancellationToken oToken)
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
                    Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Creation error");
                    return null;
                }
                if (ColorDefaults.Trace)
                    Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering");
                int S = oThMat.GetLength(0);

                double spread = 127.0;
                int R = oDataOriginal.GetLength(0);
                int C = oDataOriginal.GetLength(1);
                var oRet = new int[R, C];
                Parallel.For(0, R, r =>
                {
                    for (int c = 0; c < C; c++)
                    {
                        int col = oDataOriginal[r, c];
                        var dV = spread * DitheringStrenght * oThMat[r % S, c % S];
                        var cr = Math.Max(0, col.ToR() + dV);
                        var cg = Math.Max(0, col.ToG() + dV);
                        var cb = Math.Max(0, col.ToB() + dV);
                        var iCol = ColorIntExt.FromRGB(cr, cg, cb);
                        oRet[r, c] = ColorIntExt.GetNearestColor(iCol, oDataProcessedPalette, eMode);
                    }
                    oToken.ThrowIfCancellationRequested();
                });

                Trace.TraceInformation($"{sClass}.{sMethod} ({Type}) : Dithering completed");
                return oRet;
            }
            catch (Exception ex)
            {
                Trace.TraceError($"{sClass}.{sMethod} ({Type}) : Invalid input data");
                return null;
            }
        }
    }
}

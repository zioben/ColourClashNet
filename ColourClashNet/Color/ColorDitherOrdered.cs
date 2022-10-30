using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors
{
    public class ColorDitherOrdered : ColorDitherBase
    {

        public ColorDitherOrdered()
        {
            Name = "Ordered dithering";
            Description = "2^P Scalable ordered dithering";
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

        double[,] oBase = { { 0, 2 }, { 3, 1 } };
        double dDivider = 4;
        double[,] oThMat = null;

        //double[,] BuildNextThMatrix(double[,] oMatrix)
        //{
        //    int R = oMatrix.GetLength(0);
        //    double[,] oNext = new double[R*2,R*2];
        //    for (int r = 0; r < R; r++)
        //    {
        //        for (int c = 0; c < R; c++)
        //        {
        //            oNext[R + r, R + c] = oNext[r, R + c] = oNext[R + r, c] = oNext[r, c] = oMatrix[r, c];
        //        }
        //    }

        //    for (int r = 0; r < R*2; r++)
        //    {
        //        for (int c = 0; c < R*2; c++)
        //        {
        //            if (c % 2 == 1 || r % 2 == 1)
        //            {
        //                oNext[r, c] *= R*2;
        //            }
        //        }
        //    }
        //    for (int r = 0; r < R * 2; r++)
        //    {
        //        for (int c = 0; c < R * 2; c++)
        //        {
        //            oNext[r, c] += oMatrix[r/2,c/2];
        //        }
        //    }
        //    return oNext;
        //}

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
            for (int r = 0; r < iS; r++)
            {
                for (int c = 0; c < iS; c++)
                {
                    oThMat[r, c] += 1;
                    oThMat[r, c] /= dDivider;
                    oThMat[r, c] -= 0.5;
                }
            }
            return true;            
        }

        public override int[,] Dither(int[,] oDataProcessed, List<int> oDataProcessedPalette, int[,] oDataOriginal, ColorDistanceEvaluationMode eMode )
        {
            if (oDataProcessedPalette == null || oDataProcessedPalette.Count == 0)
            {
                return null;
            }
            if (!Create())
            {
                return null;
            }
            int S = oThMat.GetLength(0);
            double spread =255.0 / (S);// oDataProcessedPalette.Count;
            int R = oDataOriginal.GetLength(0);
            int C = oDataOriginal.GetLength(1);
            var oRet = new int[R, C];
            for (int r = 0; r < R; r++)
            {
                for (int c = 0; c < C; c++)
                {
                    int col = oDataOriginal[r, c];
                    var dV = spread * oThMat[r % S, c % S];
                    var cr = Math.Max(0, col.ToR() + dV);
                    var cg = Math.Max(0, col.ToG() + dV);
                    var cb = Math.Max(0, col.ToB() + dV);
                    var iCol = ColorIntExt.FromRGB(cr, cg, cb);
                    oRet[r, c] = iCol;
                }
            }
            //var oRetPro = Transform(oRet, oDataProcessedPalette, eMode);
            return oRet;// Pro;
        }
    }
}

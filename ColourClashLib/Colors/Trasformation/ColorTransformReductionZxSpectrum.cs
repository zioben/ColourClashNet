using ColourClashNet.Colors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionZxSpectrum : ColorTransformToPalette
    {

        static int iArea = 8;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public ColorTransformReductionZxSpectrum()
        {
            Type = ColorTransform.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        HashSet<int> hPalette = new HashSet<int>();

        public int ColL { get; set; } = 0x00AA;
        public int ColH { get; set; } = 0x00FF;
        public bool AutoEqualize { get; set; } = true;

        int iColOutL = 0x00CC;
        int iColOutH = 0x00FF;

        void OtsuXX(double[] oHist, out int iValLo, out int iValHi)
        {
            iValHi = 0;
            iValLo = 0;
            double sum1 = 0;
            double total = 0;
            for (int i = 0; i < oHist.Length; i++)
            {
                sum1 += i * oHist[i];
                total += oHist[i];
            }

            double maximum = 0;
            int level = 0;
            double sumB = 0;
            double wB = 0;

            for (int ii = 1; ii < oHist.Length; ii++)
            {
                double wF = total - wB;
                if (wB > 0 && wF > 0)
                {
                    double mF = (sum1 - sumB) / wF;
                    double val = wB * wF * (sumB / wB - mF) * (sumB / wB - mF);
                    if (val >= maximum)
                    {
                        level = ii;
                        maximum = val;
                    }
                }
                wB = wB + oHist[ii];
                sumB = sumB + (ii - 1) * oHist[ii];
            }

            {
                double s = 0;
                double w = 0;
                for (int i = 0; i < level; i++)
                {
                    s += oHist[i];
                    w += i * oHist[i];
                }
                iValLo = s > 0 ? (int)(w / s) : 0;
            }
            {
                double s = 0;
                double w = 0;
                for (int i = level; i < oHist.Length; i++)
                {
                    s += oHist[i];
                    w += i * oHist[i];
                }
                iValHi = s > 0 ? (int)(w / s) : 0;
            }
        }

        public int GetOtsuThreshold(double[] aHistData)
        {
            double HistogramAccu = 0;
            for (int i = 0; i < aHistData.Length; i++)
            {
                HistogramAccu += aHistData[i];
            }
            if (HistogramAccu > 0)
            {
                double sum = 0;
                for (int i = 0; i < 256; i++)
                {
                    sum += i * aHistData[i];
                }
                double sumB = 0;
                double wB = 0;
                double wF = 0;
                double mB;
                double mF;
                double max = 0;
                double between = 0;
                int iTH1 = 0;
                int iTH2 = 0;
                for (int i = 0; i < 256; i++)
                {
                    wB += aHistData[i];
                    if (wB == 0)
                        continue;
                    wF = HistogramAccu - wB;
                    if (wF == 0)
                        break;
                    sumB += i * aHistData[i];
                    mB = sumB / wB;
                    mF = (sum - sumB) / wF;
                    between = wB * wF * (mB - mF) * (mB - mF);
                    if (between >= max)
                    {
                        iTH1 = i;
                        if (between > max)
                        {
                            iTH2 = i;
                        }
                        max = between;
                    }
                }
                int iTH = (iTH1 + iTH2) / 2;
                return iTH;
            }
            return 0;
        }



        void Otsu(double[] oHist, out int iValLo, out int iValHi)
        {
            iValLo = 128;
            iValHi = 255;
            int level = GetOtsuThreshold(oHist);
            {
                double s = 0;
                double w = 0;
                for (int i = 0; i < level; i++)
                {
                    s += oHist[i];
                    w += i * oHist[i];
                }
                iValLo = s > 0 ? (int)(w / s) : 0;
            }
            {
                double s = 0;
                double w = 0;
                for (int i = level; i < oHist.Length; i++)
                {
                    s += oHist[i];
                    w += i * oHist[i];
                }
                iValHi = s > 0 ? (int)(w / s) : 0;
            }
            iValLo = level;
        }


        protected override void CreateTrasformationMap()
        {
            int iColLR = ColL;
            int iColLG = ColL;
            int iColLB = ColL;
            int iColHR = ColH;
            int iColHG = ColH;
            int iColHB = ColH;
            if (AutoEqualize)
            {
                double[] dR = new double[256];
                double[] dG = new double[256];
                double[] dB = new double[256];
                foreach (var kvp in SourceColorHistogram.rgbHistogram)
                {
                    if (kvp.Key < 0)
                        continue;
                    var r = kvp.Key.ToR();
                    var g = kvp.Key.ToG();
                    var b = kvp.Key.ToB();
                    dR[r] += kvp.Value;
                    dG[g] += kvp.Value;
                    dB[b] += kvp.Value;
                }
                Otsu(dR, out iColLR, out iColHR);
                Otsu(dG, out iColLG, out iColHG);
                Otsu(dB, out iColLB, out iColHB);

            }
            int c00 = ColorIntExt.FromRGB(0, 0, iColLB);
            int c01 = ColorIntExt.FromRGB(iColLR, 0, 0);
            int c02 = ColorIntExt.FromRGB(iColLR, 0, iColLB);
            int c03 = ColorIntExt.FromRGB(0, iColLG, 0);
            int c04 = ColorIntExt.FromRGB(0, iColLG, iColLB);
            int c05 = ColorIntExt.FromRGB(iColLR, iColLG, 0);
            int c06 = ColorIntExt.FromRGB(0, 0, iColHB);
            int c07 = ColorIntExt.FromRGB(iColHR, 0, 0);
            int c08 = ColorIntExt.FromRGB(iColHR, 0, iColHB);
            int c09 = ColorIntExt.FromRGB(0, iColHG, 0);
            int c10 = ColorIntExt.FromRGB(0, iColHG, iColHB);
            int c11 = ColorIntExt.FromRGB(iColHR, iColHG, 0);
            int c12 = ColorIntExt.FromRGB(0, 0, 0);
            int c13 = ColorIntExt.FromRGB(iColLR, iColLG, iColLB);
            int c14 = ColorIntExt.FromRGB(iColHR, iColHG, iColHB);

            TransformationColorPalette.Reset();
            TransformationColorPalette.rgbPalette.Add(c00);
            TransformationColorPalette.rgbPalette.Add(c01);
            TransformationColorPalette.rgbPalette.Add(c02);
            TransformationColorPalette.rgbPalette.Add(c03);
            TransformationColorPalette.rgbPalette.Add(c04);
            TransformationColorPalette.rgbPalette.Add(c05);
            TransformationColorPalette.rgbPalette.Add(c06);
            TransformationColorPalette.rgbPalette.Add(c07);
            TransformationColorPalette.rgbPalette.Add(c08);
            TransformationColorPalette.rgbPalette.Add(c09);
            TransformationColorPalette.rgbPalette.Add(c10);
            TransformationColorPalette.rgbPalette.Add(c11);
            TransformationColorPalette.rgbPalette.Add(c12);
            TransformationColorPalette.rgbPalette.Add(c13);
            TransformationColorPalette.rgbPalette.Add(c14);
        }



        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, TransformationColorPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;
            //return oTmpData;

            int R = oTmpData.GetLength(0);
            int C = oTmpData.GetLength(1);
            int[,] oRet = new int[R, C];

            List<ColorTile> lDataBlock = new List<ColorTile>();

            //Parallel.For(0, R / 8, r =>
            for (int r = 0; r < R / iTile; r++)
            {
                for (int c = 0; c < C / iTile; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    ColorTile oTile = new ColorTile()
                    {
                        r = r,
                        c = c,
                        TileData = new int[iArea, iArea],
                    };
                    for (int rr = 0; rr < iArea; rr++)
                    {
                        var rPos = Math.Min(R - 1, Math.Max(0, rr - iOffS + r * iTile));
                        for (int cc = 0; cc < iArea; cc++)
                        {
                            var cPos = Math.Min(C - 1, Math.Max(0, cc - iOffS + c * iTile));
                            var rgb = oTmpData[rPos, cPos];
                            oTile.TileData[rr, cc] = rgb;
                        }
                    }
                    lDataBlock.Add(oTile);
                }
            }

            Parallel.ForEach(lDataBlock, oTile =>
                {

                    var TileDataProc = oTile.Process(null);

                    for (int rr = 0; rr < iTile; rr++)
                    {
                        for (int cc = 0; cc < iTile; cc++)
                        {
                            oRet[oTile.r * iTile + rr, oTile.c * iTile + cc] = TileDataProc[rr + iOffS, cc + iOffS];
                        }
                    }
                });

            //return oRet;
            //int c00 = ColorIntExt.FromRGB(0, 0, ColL);
            //int c01 = ColorIntExt.FromRGB(ColL, 0, 0);
            //int c02 = ColorIntExt.FromRGB(ColL, 0, ColL);
            //int c03 = ColorIntExt.FromRGB(0, ColL, 0);
            //int c04 = ColorIntExt.FromRGB(0, ColL, ColL);
            //int c05 = ColorIntExt.FromRGB(ColL, ColL, 0);
            //int c06 = ColorIntExt.FromRGB(0, 0, ColH);
            //int c07 = ColorIntExt.FromRGB(ColH, 0, 0);
            //int c08 = ColorIntExt.FromRGB(ColH, 0, ColH);
            //int c09 = ColorIntExt.FromRGB(0, ColH, 0);
            //int c10 = ColorIntExt.FromRGB(0, ColH, ColH);
            //int c11 = ColorIntExt.FromRGB(ColH, ColH, 0);
            //int c12 = ColorIntExt.FromRGB(0, 0, 0);
            //int c13 = ColorIntExt.FromRGB(ColL, ColL, ColL);
            //int c14 = ColorIntExt.FromRGB(ColH, ColH, ColH);
            TransformationColorMap.Clear();
            var lPalette =  TransformationColorPalette.ToList();
            TransformationColorMap.Add(lPalette[0], ColorIntExt.FromRGB(0, 0, iColOutL));
            TransformationColorMap.Add(lPalette[1], ColorIntExt.FromRGB(iColOutL, 0, 0));
            TransformationColorMap.Add(lPalette[2], ColorIntExt.FromRGB(iColOutL, 0, iColOutL));
            TransformationColorMap.Add(lPalette[3], ColorIntExt.FromRGB(0, iColOutL, 0));
            TransformationColorMap.Add(lPalette[4], ColorIntExt.FromRGB(0, iColOutL, iColOutL));
            TransformationColorMap.Add(lPalette[5], ColorIntExt.FromRGB(iColOutL, iColOutL, 0));
            TransformationColorMap.Add(lPalette[6], ColorIntExt.FromRGB(0, 0, iColOutH));
            TransformationColorMap.Add(lPalette[7], ColorIntExt.FromRGB(iColOutH, 0, 0));
            TransformationColorMap.Add(lPalette[8], ColorIntExt.FromRGB(iColOutH, 0, iColOutH));
            TransformationColorMap.Add(lPalette[9], ColorIntExt.FromRGB(0, iColOutH, 0));
            TransformationColorMap.Add(lPalette[10], ColorIntExt.FromRGB(0, iColOutH, iColOutH));
            TransformationColorMap.Add(lPalette[11], ColorIntExt.FromRGB(iColOutH, iColOutH, 0));
            TransformationColorMap.Add(lPalette[12], ColorIntExt.FromRGB(0, 0, 0));
            TransformationColorMap.Add(lPalette[13], ColorIntExt.FromRGB(iColOutL, iColOutL, iColOutL));
            TransformationColorMap.Add(lPalette[14], ColorIntExt.FromRGB(iColOutH, iColOutH, iColOutH));
            var oZxRet = ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}
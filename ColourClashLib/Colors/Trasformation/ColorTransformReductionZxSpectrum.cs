﻿using ColourClashLib.Color;
using ColourClashLib.Colors;
using ColourClashNet.Colors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
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

        public int ColTH { get; set; } = 0x00AA;

        int iColOutL = 0x00CC;
        int iColOutH = 0x00FF;

        protected override void CreateTrasformationMap()
        {
            int iColLR = ColTH/2;
            int iColLG = ColTH/2;
            int iColLB = ColTH/2;
            int iColHR = (255 + ColTH)/2;
            int iColHG = (255 + ColTH)/2;
            int iColHB = (255 + ColTH)/2;

            ColorPalette oPal = new ColorPalette();        
            oPal.Add( ColorIntExt.FromRGB(0, 0, iColLB));
            oPal.Add( ColorIntExt.FromRGB(iColLR, 0, 0));
            oPal.Add( ColorIntExt.FromRGB(iColLR, 0, iColLB));
            oPal.Add( ColorIntExt.FromRGB(0, iColLG, 0));
            oPal.Add( ColorIntExt.FromRGB(0, iColLG, iColLB));
            oPal.Add( ColorIntExt.FromRGB(iColLR, iColLG, 0));
            oPal.Add( ColorIntExt.FromRGB(0, 0, iColHB));
            oPal.Add( ColorIntExt.FromRGB(iColHR, 0, 0));
            oPal.Add( ColorIntExt.FromRGB(iColHR, 0, iColHB));
            oPal.Add( ColorIntExt.FromRGB(0, iColHG, 0));
            oPal.Add( ColorIntExt.FromRGB(0, iColHG, iColHB));
            oPal.Add( ColorIntExt.FromRGB(iColHR, iColHG, 0));
            oPal.Add( ColorIntExt.FromRGB(0, 0, 0));
            oPal.Add( ColorIntExt.FromRGB(iColLR, iColLG, iColLB));
            oPal.Add( ColorIntExt.FromRGB(iColHR, iColHG, iColHB));

            ColorHistogram.Create(oPal);
            ColorPalette = ColorHistogram.ToColorPalette();
        }



        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, ColorPalette, ColorDistanceEvaluationMode);
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
            ColorTransformationMap.Reset();
            var lPalette = ColorPalette.ToList();
            ColorTransformationMap.Add(lPalette[0], ColorIntExt.FromRGB(0, 0, iColOutL));
            ColorTransformationMap.Add(lPalette[1], ColorIntExt.FromRGB(iColOutL, 0, 0));
            ColorTransformationMap.Add(lPalette[2], ColorIntExt.FromRGB(iColOutL, 0, iColOutL));
            ColorTransformationMap.Add(lPalette[3], ColorIntExt.FromRGB(0, iColOutL, 0));
            ColorTransformationMap.Add(lPalette[4], ColorIntExt.FromRGB(0, iColOutL, iColOutL));
            ColorTransformationMap.Add(lPalette[5], ColorIntExt.FromRGB(iColOutL, iColOutL, 0));
            ColorTransformationMap.Add(lPalette[6], ColorIntExt.FromRGB(0, 0, iColOutH));
            ColorTransformationMap.Add(lPalette[7], ColorIntExt.FromRGB(iColOutH, 0, 0));
            ColorTransformationMap.Add(lPalette[8], ColorIntExt.FromRGB(iColOutH, 0, iColOutH));
            ColorTransformationMap.Add(lPalette[9], ColorIntExt.FromRGB(0, iColOutH, 0));
            ColorTransformationMap.Add(lPalette[10], ColorIntExt.FromRGB(0, iColOutH, iColOutH));
            ColorTransformationMap.Add(lPalette[11], ColorIntExt.FromRGB(iColOutH, iColOutH, 0));
            ColorTransformationMap.Add(lPalette[12], ColorIntExt.FromRGB(0, 0, 0));
            ColorTransformationMap.Add(lPalette[13], ColorIntExt.FromRGB(iColOutL, iColOutL, iColOutL));
            ColorTransformationMap.Add(lPalette[14], ColorIntExt.FromRGB(iColOutH, iColOutH, iColOutH));
            var oZxRet = ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}
using ColourClashLib.Color;
using ColourClashLib.Colors;
using ColourClashNet.Colors;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Colors.Transformation
{
    public class ColorTransformReductionZxSpectrumV2 : ColorTransformReductionPalette
    {

        static int iArea = 10;
        static int iTile = 8;
        static int iOffS = iArea - iTile;

        public enum ZxPaletteMode
        { 
            PaletteLo = 0,
            PaletteHi = 1,
            Both = 2,
        }

        public ColorTransformReductionZxSpectrumV2()
        {
            Type = ColorTransform.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        HashSet<int> hPalette = new HashSet<int>();

        public int ColL { get; set; } = 0x0080;
        public int ColH { get; set; } = 0x00FF;
        public ZxPaletteMode PaletteMode { get; set; } = ZxPaletteMode.Both;
      //  public bool AutoEqualize { get; set; } = true;

        int iColOutL = 0x00D8;
        int iColOutH = 0x00FF;

       // ColorPalette oPaletteLo;
       // ColorPalette oPaletteHi;

        ColorPalette CreatePalette(int iCol)
        {
            int iColR = iCol;
            int iColG = iCol;
            int iColB = iCol;
            ColorPalette oPalette = new ColorPalette();
            oPalette.Add(ColorIntExt.FromRGB(0, 0, 0));
            oPalette.Add(ColorIntExt.FromRGB(0, 0, iColB));
            oPalette.Add(ColorIntExt.FromRGB(iColR, 0, 0));
            oPalette.Add(ColorIntExt.FromRGB(iColR, 0, iColB));
            oPalette.Add(ColorIntExt.FromRGB(0, iColG, 0));
            oPalette.Add(ColorIntExt.FromRGB(0, iColG, iColB));
            oPalette.Add(ColorIntExt.FromRGB(iColR, iColG, 0));
            oPalette.Add(ColorIntExt.FromRGB(iColR, iColG, iColB));
            return oPalette;
        }



        protected override void CreateTrasformationMap()
        {
        }

        int[,]? CreateImage(int[,]? oDataSource, int iCol)
        {
            var oPalette = CreatePalette(iCol);
            ColorHistogram.Create(oPalette);
            ColorPalette = ColorHistogram.ToColorPalette();
            var oTmpData = base.ExecuteTransform(oDataSource);
            if (Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, oPalette, ColorDistanceEvaluationMode);
            }
            return oTmpData;
        }



        protected override int[,]? ExecuteTransform(int[,]? oDataSource)
        {
            if (oDataSource == null)
                return null;

            var icl = ColL;
            var ich = ColH;
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteHi:
                    icl = ColH;
                    break;
                case ZxPaletteMode.PaletteLo:
                    ich = ColL;
                    break;
                default: 
                    break;

            }

            var oTmpDataLo = CreateImage(oDataSource, icl);
            var oTmpDataHi = CreateImage(oDataSource, ich);

            BypassDithering = true;

         //   return oTmpDataLo;
          
            
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            int[,] oRet = new int[R, C];

//            List<ColorTile> lDataBlock = new List<ColorTile>();
            List<ColorTile> lDataBlockLo = new List<ColorTile>();
            List<ColorTile> lDataBlockHi = new List<ColorTile>();

            //Parallel.For(0, R / 8, r =>
            for (int r = 0; r < R / iTile; r++)
            {
                for (int c = 0; c < C / iTile; c++)
                //  Parallel.For(0, C / 8, c =>
                {
                    ColorTile oTileLo = new ColorTile()
                    {
                        r = r,
                        c = c,
                        TileData = new int[iArea, iArea],
                    };
                    ColorTile oTileHi = new ColorTile()
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
                            var rgbLo = oTmpDataLo[rPos, cPos];
                            var rgbHi = oTmpDataHi[rPos, cPos];
                            oTileLo.TileData[rr, cc] = rgbLo;
                            oTileHi.TileData[rr, cc] = rgbHi;
                        }
                    }
                    lDataBlockLo.Add(oTileLo);
                    lDataBlockHi.Add(oTileHi);
                }
            }


            Parallel.For( 0, lDataBlockLo.Count, i =>
                {
                    var oTileLo = lDataBlockLo[i];
                    var oTileHi = lDataBlockHi[i];
                    var TileDataProcLo = oTileLo.Process(null);
                    var TileDataProcHi = oTileHi.Process(null);
                    int[,] TileDataProc = null;
                    if (oTileLo.Error < oTileHi.Error)
                    {
                        TileDataProc = TileDataProcLo;
                    }
                    else
                    {
                        TileDataProc = TileDataProcHi;
                    }
                    for (int rr = 0; rr < iTile; rr++)
                    {
                        for (int cc = 0; cc < iTile; cc++)
                        {
                            oRet[oTileLo.r * iTile + rr, oTileLo.c * iTile + cc] = TileDataProc[rr + iOffS, cc + iOffS];
                        }
                    }
                });
          
            ColorTransformationMap.Reset();
            //
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, ColL), ColorIntExt.FromRGB(0, 0, iColOutL));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColL,0,0), ColorIntExt.FromRGB(iColOutL, 0, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColL, 0, ColL), ColorIntExt.FromRGB(iColOutL, 0, iColOutL));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, ColL,0 ), ColorIntExt.FromRGB(0, iColOutL, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, ColL,ColL), ColorIntExt.FromRGB(0, iColOutL, iColOutL));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColL, ColL,0), ColorIntExt.FromRGB(iColOutL, iColOutL, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColL, ColL, ColL), ColorIntExt.FromRGB(iColOutL, iColOutL, iColOutL));
            //
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, ColH), ColorIntExt.FromRGB(0, 0, iColOutH));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColH, 0, 0), ColorIntExt.FromRGB(iColOutH, 0, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColH, 0, ColH), ColorIntExt.FromRGB(iColOutH, 0, iColOutH));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0 , ColH, 0), ColorIntExt.FromRGB(0, iColOutH, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(0, ColH, ColH), ColorIntExt.FromRGB(0, iColOutH, iColOutH));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColH, ColH, 0 ), ColorIntExt.FromRGB(iColOutH, iColOutH, 0));
            ColorTransformationMap.Add(ColorIntExt.FromRGB(ColH, ColH, ColH), ColorIntExt.FromRGB(iColOutH, iColOutH, iColOutH));
            var oZxRet = ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}
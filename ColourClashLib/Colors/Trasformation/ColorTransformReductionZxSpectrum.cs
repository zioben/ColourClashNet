using ColourClashLib.Color;
using ColourClashLib.Colors;
using ColourClashLib.Colors.Tile;
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
    public class ColorTransformReductionZxSpectrum : ColorTransformReductionPalette
    {

        public enum ZxPaletteMode
        { 
            PaletteLo = 0,
            PaletteHi = 1,
            Both = 2,
        }

        public ColorTransformReductionZxSpectrum()
        {
            type = ColorTransform.ColorReductionZxSpectrum;
            description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
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
            if( iCol > 128  ) 
            {
                iColR = iCol;
                iColG = iCol;
                iColB = iCol;
            }
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
            colorHistogram.Create(oPalette);
            colorPalette = colorHistogram.ToColorPalette();
            var oTmpData = base.ExecuteTransform(oDataSource);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oDataSource, oTmpData, oPalette, ColorDistanceEvaluationMode);
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

            var oPaletteLO = CreatePalette(icl);
            var oPaletteHI = CreatePalette(ich);

            var oPaletteZX = new ColorPalette();

            foreach (var rgb in oPaletteLO.rgbPalette)
            {
                oPaletteZX.Add(rgb);
            }
            foreach (var rgb in oPaletteHI.rgbPalette)
            {
                oPaletteZX.Add(rgb);
            }

            colorHistogram.Create(oPaletteZX);
            colorPalette = colorHistogram.ToColorPalette();
            var oTmpData = base.ExecuteTransform(oDataSource);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oDataSource, oTmpData, oPaletteZX, ColorDistanceEvaluationMode);
            }

            BypassDithering = true;

            var oTmpDataLo = CreateImage(oDataSource, icl);
            TileManager oTileManagerL = new TileManager();
            oTileManagerL.Create(oTmpDataLo, 8, 8, 2, null, TileBase.EnumColorReductionMode.Detailed);
            var oRetL = oTileManagerL.TransformAndDither(oTmpDataLo);

            var oTmpDataHi = CreateImage(oDataSource, ich);
            TileManager oTileManagerH = new TileManager();
            oTileManagerH.Create(oTmpDataHi, 8, 8, 2, null, TileBase.EnumColorReductionMode.Detailed);
            var oRetH = oTileManagerH.TransformAndDither(oTmpDataHi);

            oTileManagerL.CalcExternalImageError(oTmpData);
            oTileManagerH.CalcExternalImageError(oTmpData);
        
            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            var oRet = new int[R, C];

            int RT = oTileManagerL.TileData.GetLength(0);
            int CT = oTileManagerL.TileData.GetLength(1);

            for (int r = 0; r < RT; r++)
            {
                for( int c = 0; c < CT; c++)
                {
                    TileBase.MergeData(oRet, oTileManagerL.TileData[r, c], oTileManagerH.TileData[r, c], TileBase.EnumErrorSourceMode.ExternalImageError);
                }
            }
          
            colorTransformationMap.Reset();
            //
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, ColL), ColorIntExt.FromRGB(0, 0, iColOutL));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColL,0,0), ColorIntExt.FromRGB(iColOutL, 0, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColL, 0, ColL), ColorIntExt.FromRGB(iColOutL, 0, iColOutL));
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, ColL,0 ), ColorIntExt.FromRGB(0, iColOutL, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, ColL,ColL), ColorIntExt.FromRGB(0, iColOutL, iColOutL));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColL, ColL,0), ColorIntExt.FromRGB(iColOutL, iColOutL, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColL, ColL, ColL), ColorIntExt.FromRGB(iColOutL, iColOutL, iColOutL));
            //
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, 0, ColH), ColorIntExt.FromRGB(0, 0, iColOutH));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColH, 0, 0), ColorIntExt.FromRGB(iColOutH, 0, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColH, 0, ColH), ColorIntExt.FromRGB(iColOutH, 0, iColOutH));
            colorTransformationMap.Add(ColorIntExt.FromRGB(0 , ColH, 0), ColorIntExt.FromRGB(0, iColOutH, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(0, ColH, ColH), ColorIntExt.FromRGB(0, iColOutH, iColOutH));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColH, ColH, 0 ), ColorIntExt.FromRGB(iColOutH, iColOutH, 0));
            colorTransformationMap.Add(ColorIntExt.FromRGB(ColH, ColH, ColH), ColorIntExt.FromRGB(iColOutH, iColOutH, iColOutH));
            var oZxRet = ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}
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

        int iColOutL = 0x00D8;
        int iColOutH = 0x00FF;

        ColorTransformationMap  CreateZxMap(int iCol, int iColOut)
        {
            int iColR = iCol;
            int iColG = iCol+1;
            int iColB = iCol+2;
            if (iCol > 128)
            {
                iColR = iCol;
                iColG = iCol-1;
                iColB = iCol-2;
            }
            ColorTransformationMap oMap = new ColorTransformationMap();
            oMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0) );
            oMap.Add(ColorIntExt.FromRGB(0, 0, iColB), ColorIntExt.FromRGB(0, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColR, 0, 0), ColorIntExt.FromRGB(iColOut, 0, 0));
            oMap.Add(ColorIntExt.FromRGB(iColR, 0, iColB), ColorIntExt.FromRGB(iColOut, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(0, iColG, 0), ColorIntExt.FromRGB(0, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(0, iColG, iColB), ColorIntExt.FromRGB(0, iColOut, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColR, iColG, 0), ColorIntExt.FromRGB(iColOut, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(iColR, iColG, iColB), ColorIntExt.FromRGB(iColOut, iColOut, iColOut));
            return oMap;
        }

        protected override void CreateTrasformationMap()
        {
        }

        int[,]? CreateImage(int[,]? oDataSource, int iCol, int iColOut)
        {
            var oMap = CreateZxMap(iCol,iColOut);
            oPalette = new ColorPalette();
            foreach (var rgb in oMap.rgbTransformationMap)
            {
                oPalette.Add(rgb.Key);
            }
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
            int iol = iColOutL;
            int ioh = iColOutH;
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteHi:
                    icl = ColH;
                    iol = iColOutH;
                    break;
                case ZxPaletteMode.PaletteLo:
                    ich = ColL;
                    ioh = iColOutL;
                    break;
                default: 
                    break;

            }

            var oZxMapLO = CreateZxMap(icl, iol);
            var oZxMapHI = CreateZxMap(ich, ioh);
            oPalette = new ColorPalette();
            foreach (var rgb in oZxMapLO.rgbTransformationMap)
            {
                oPalette.Add(rgb.Key);
            }
            foreach (var rgb in oZxMapHI.rgbTransformationMap)
            {
                oPalette.Add(rgb.Key);
            }

            var oTmpData = base.ExecuteTransform(oDataSource);
            if (dithering != null)
            {
                oTmpData = dithering.Dither(oDataSource, oTmpData, oPalette, ColorDistanceEvaluationMode);
            }
            BypassDithering = true;

            var oTmpDataLo = CreateImage(oDataSource, icl,iol);
            TileManager oTileManagerL = new TileManager();
            oTileManagerL.Create(oTmpDataLo, 8, 8, 2, null, TileBase.EnumColorReductionMode.Detailed);
            var oRetL = oTileManagerL.TransformAndDither(oTmpDataLo);
            var oTmpDataHi = CreateImage(oDataSource, ich, ioh);
            TileManager oTileManagerH = new TileManager();
            oTileManagerH.Create(oTmpDataHi, 8, 8, 2, null, TileBase.EnumColorReductionMode.Detailed);
            var oRetH = oTileManagerH.TransformAndDither(oTmpDataHi);
            return oRetH;

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
            foreach (var rgb in oZxMapLO.rgbTransformationMap)
            {
                colorTransformationMap.Add(rgb.Key, rgb.Value);
            }
            foreach (var rgb in oZxMapHI.rgbTransformationMap)
            {
                colorTransformationMap.Add(rgb.Key, rgb.Value);
            }

            var oZxRet = ExecuteStdTransform(oRet, this);
            return oZxRet;
        }
    }
}
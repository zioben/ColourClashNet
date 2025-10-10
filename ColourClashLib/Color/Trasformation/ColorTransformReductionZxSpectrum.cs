using ColourClashNet.Color;
using ColourClashNet.Color.Trasformation;
using ColourClashNet.Color;
using ColourClashNet.Color.Tile;
using ColourClashNet.Color;
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
using ColourClashNet.Log;

namespace ColourClashNet.Color.Transformation
{
    public class ColorTransformReductionZxSpectrum : ColorTransformReductionPalette
    {
        static string sClass = nameof(ColorTransformReductionZxSpectrum);

        public enum ZxPaletteMode
        { 
            PaletteLo = 0,
            PaletteHi = 1,
            Both = 2,
        }


        public ColorTransformReductionZxSpectrum()
        {
            Type = ColorTransformType.ColorReductionZxSpectrum;
            Description = "Reduce color to ZX Spectrum color map and apply Colourclash reduction";
        }

        HashSet<int> hPalette = new HashSet<int>();

        public int ColL { get; set; } = 0x0080;
        public int ColH { get; set; } = 0x00FF;
        public ZxPaletteMode PaletteMode { get; set; } = ZxPaletteMode.Both;
        public bool IncludeBlackInHighColor { get; set; } = true;

        public bool AutoTune { get; set; } = true;  
        public bool DitherHighColor { get; set; } = true;

        int iColOutL = 0x00D8;
        int iColOutH = 0x00FF;

        public override ColorTransformInterface SetProperty( ColorTransformProperties eProperty, object oValue )
        {
            if (base.SetProperty(eProperty, oValue) != null)
                return this;
            switch( eProperty) 
            {
                case ColorTransformProperties.Zx_ColL:
                    if (int.TryParse(oValue.ToString(), out var l))
                    {
                        ColL= l;
                        return this;
                    }
                    break;
                case ColorTransformProperties.Zx_ColH:
                    if (int.TryParse(oValue.ToString(), out var h))
                    {
                        ColH = h;
                        return this;
                    }
                    break;
                case ColorTransformProperties.Zx_DitherHighColor:
                    if (bool.TryParse(oValue?.ToString(), out var d))
                    {
                        DitherHighColor = d;    
                        return this;    
                    }
                    break;
                case ColorTransformProperties.Zx_IncludeBlackInHighColor:
                    if (bool.TryParse(oValue?.ToString(), out var b))
                    {
                        DitherHighColor = b;
                        return this;
                    }
                    break;
                case ColorTransformProperties.Zx_PaletteMode:
                    {
                        if (Enum.TryParse<ZxPaletteMode>(oValue?.ToString(), out var eMode))
                        {
                            PaletteMode = eMode;
                            return this;
                        }
                    }
                    break;
                default:
                    break;
            }
            return null;
        }

        ColorTransformationMap  CreateZxMap(int iCol, int iColOut, bool bUseBlack )
        {
            int iOffset = 0;
            int iColR = iCol + 0 * iOffset;
            int iColG = iCol + 1 * iOffset;
            int iColB = iCol + 2 * iOffset;
            if (iCol > 128)
            {
                iColR = iCol - 0 * iOffset;
                iColG = iCol - 1 * iOffset;
                iColB = iCol - 2 * iOffset;
            }
            ColorTransformationMap oMap = new ColorTransformationMap();
            if (bUseBlack)
            {
                oMap.Add(ColorIntExt.FromRGB(0, 0, 0), ColorIntExt.FromRGB(0, 0, 0));
            }
            oMap.Add(ColorIntExt.FromRGB(0, 0, iColB), ColorIntExt.FromRGB(0, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColR, 0, 0), ColorIntExt.FromRGB(iColOut, 0, 0));
            oMap.Add(ColorIntExt.FromRGB(iColR, 0, iColB), ColorIntExt.FromRGB(iColOut, 0, iColOut));
            oMap.Add(ColorIntExt.FromRGB(0, iColG, 0), ColorIntExt.FromRGB(0, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(0, iColG, iColB), ColorIntExt.FromRGB(0, iColOut, iColOut));
            oMap.Add(ColorIntExt.FromRGB(iColR, iColG, 0), ColorIntExt.FromRGB(iColOut, iColOut, 0));
            oMap.Add(ColorIntExt.FromRGB(iColR, iColG, iColB), ColorIntExt.FromRGB(iColOut, iColOut, iColOut));
            return oMap;
        }

       
        int[,]? CreateImage(int[,]? oDataSource, int iCol, int iColOut, bool bUseBlack, bool bDither, ColorDistanceEvaluationMode eColorMode, CancellationToken oToken )
        {
            var oMap = CreateZxMap(iCol,iColOut, bUseBlack);
            OutputPalette = new Palette();
            foreach (var rgb in oMap.rgbTransformationMap)
            {
                OutputPalette.Add(rgb.Key);
            }
            var oOld = ColorDistanceEvaluationMode;
            ColorDistanceEvaluationMode = eColorMode;
            var oTmpData = base.ExecuteTransform(oDataSource, oToken );
            ColorDistanceEvaluationMode = oOld;
            if (bDither && Dithering != null)
            {
                oTmpData = Dithering.Dither(oDataSource, oTmpData, OutputPalette, eColorMode, oToken );
            }
            return oTmpData;
        }

        TileManager CreateTiles(int[,]? oDataSource, int iCol, int iColOut, bool bUseBlack, bool bDither, ColorDistanceEvaluationMode eColorMode, CancellationToken oToken)
        { 
            TileManager oTileManager = new TileManager();
            var oTmpData = CreateImage(oDataSource, iCol, iColOut, bUseBlack, bDither, eColorMode, oToken);
            oTileManager.Init(oTmpData, 8, 8, 2, null, eColorMode, TileBase.EnumColorReductionMode.Detailed);
            oTileManager.CreateTiles(oTmpData);
            oTileManager.CalcExternalImageError(oDataSource);
            return oTileManager;
        }

        protected Tuple<int[,]?,double, ColorTransformationMap, ColorTransformationMap> ExecuteTransformZX(int[,]? oDataSource, int iColL, int iColH, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;

            var icl = iColL;
            var ich = iColH;
            int iol = iColOutL;
            int ioh = iColOutH;
            switch (PaletteMode)
            {
                case ZxPaletteMode.PaletteHi:
                    icl = iColH;
                    iol = iColOutH;
                    break;
                case ZxPaletteMode.PaletteLo:
                    ich = iColL;
                    ioh = iColOutL;
                    break;
                default: 
                    break;

            }

            var oZxMapLO = CreateZxMap(icl, iol, true);
            var oZxMapHI = CreateZxMap(ich, ioh, true);

            BypassDithering = true;

            List<TileManager> lTM = new List<TileManager>();
            TileManager oTileManagerL1 = CreateTiles(oDataSource, icl, iol, true, true, ColorDistanceEvaluationMode, oToken);
            TileManager oTileManagerH1 = CreateTiles(oDataSource, ich, ioh, IncludeBlackInHighColor, DitherHighColor, ColorDistanceEvaluationMode, oToken );
            lTM.Add(oTileManagerL1);    
            lTM.Add(oTileManagerH1);

            var oTileRet = TileManager.MergeData(oDataSource, lTM, TileBase.EnumErrorSourceMode.ExternalImageError);
          
            ColorTransformationMapper.Reset();
            foreach (var rgb in oZxMapLO.rgbTransformationMap)
            {
                ColorTransformationMapper.Add(rgb.Key, rgb.Value);
            }
            foreach (var rgb in oZxMapHI.rgbTransformationMap)
            {
                ColorTransformationMapper.Add(rgb.Key, rgb.Value);
            }

            var oRet = ExecuteStdTransform(oTileRet, this, oToken);
            var dError = ColorTransformBase.EvaluateError(oDataSource, oTileRet, ColorDistanceEvaluationMode);
            return new Tuple<int[,]?, double, ColorTransformationMap, ColorTransformationMap>(oTileRet, dError, oZxMapLO, oZxMapHI);
        }

        object locker = new object();

        protected override int[,]? ExecuteTransform(int[,]? oDataSource, CancellationToken oToken)
        {
            string sMethod = nameof(ExecuteTransform);

            if (oDataSource == null)
                return null;

            BypassDithering = true;

            if (!AutoTune)
            {
                return ExecuteTransformZX(oDataSource, ColL,ColH, oToken).Item1;
            }


            int LBest = ColL;
            int HBest = ColH;
            var oBest = ExecuteTransformZX(oDataSource, LBest, HBest, oToken);
            var dMinError = oBest.Item2;
            var dError = double.PositiveInfinity;
            int iL = 0;
            int iH = 5000;
            bool bExit = false;
            while (!bExit)// (iL + 1) <= iH && dError <= dMinError)
            {
                iL = LBest + 8;
                iH = HBest - 8;
                var oBaseL = ExecuteTransformZX(oDataSource, iL, HBest, oToken);
                var oBaseH = ExecuteTransformZX(oDataSource, LBest, iH, oToken);
                var dErrL = oBaseL.Item2;
                var dErrH = oBaseH.Item2;
                if (dErrL < dErrH)
                {
                    LBest = iL;
                    if (dErrL < dMinError)
                    {
                        ColL = iL;
                        oBest = oBaseL;
                    }
                }
                else
                {
                    HBest = iH;
                    if (dErrH < dMinError)
                    {
                        ColH = iH;
                        oBest = oBaseH;
                    }
                }
                dError = Math.Min(dErrL, dErrH);
                LogMan.Message(sClass, sMethod, $"Working with : LO = {iL:D3} and HI = {iH:D3} : Error = {dError} --- Best Error = {oBest.Item2}");
                if (iH < iL)
                    bExit = true;
                if (dError > dMinError )
                    bExit = true;
                if (dMinError > dError)
                {
                    dMinError = dError;
                }
            }

            ColorTransformationMapper.Reset();
            foreach (var rgb in oBest.Item3.rgbTransformationMap)
            {
                ColorTransformationMapper.Add(rgb.Key, rgb.Value);
            }
            foreach (var rgb in oBest.Item4.rgbTransformationMap)
            {
                ColorTransformationMapper.Add(rgb.Key, rgb.Value);
            }

            var oRet = ExecuteStdTransform(oBest.Item1, this, oToken);
            return oRet;
        }



    }
}
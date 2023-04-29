﻿using ColourClashLib.Color;
using ColourClashNet.Colors;
using ColourClashNet.Colors.Dithering;
using ColourClashNet.Colors.Transformation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Colors.Tile
{
    public class TileManager
    {
        TileBase[,] TileData { get; set; }
        public int TileW { get; private set; } = 8;
        public int TileH { get; private set; } = 8;
        public int TileR { get; private set; } = 0;
        public int TileC { get; private set; } = 0;

        public ColorPalette FixedColorPalette { get; set; } = new ColorPalette();

        ColorDistanceEvaluationMode ColorDistanceMode { get; set; } = ColorDistanceEvaluationMode.RGB;

        TileBase.EnumColorReductionMode ColorReductionMode { get; set; } = TileBase.EnumColorReductionMode.Detailed;
        public int MaxColors { get; set; } = 2;

        public TileBase? GetTileData(int r, int c)
        {
            if( r >= 0 && r<TileR && c >= 0 && c < TileC )
            {
                return TileData[r, c];
            }
            return null;
        }

        void Free()
        {
            TileData = null;
            TileW = 0;
            TileH = 0;
            TileR = 0;
            TileC = 0;  
            MaxColors = 0;
        }


        public bool Init(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, ColorPalette oFixedColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode)
        {
            Free();
            if (oDataSource == null)
            {
                return false;
            }
            if (iTileW <= 0 || iTileH <= 0 || iMaxTileColors <= 0)
            {
                return false;
            }
            TileW = iTileW;
            TileH = iTileH;
            ColorDistanceMode = eColorDistanceMode;
            ColorReductionMode = eColorReductionMode;
            FixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            MaxColors = iMaxTileColors;
            TileR = (oDataSource.GetLength(0) + TileH - 1) / TileH;
            TileC = (oDataSource.GetLength(1) + TileW - 1) / TileW;
            TileData = new TileBase[TileR, TileC];
            for (int r = 0; r < TileR; r++)
            {
                for (int c = 0; c < TileC; c++)
                {
                    TileData[r, c] = new TileBase()
                    {
                        TileW = TileW,
                        TileH = TileH,
                        TileMaxColors = MaxColors,
                        ColorDistanceMode = eColorDistanceMode,
                        ColorReductionMode = ColorReductionMode,
                        FixedPalette = FixedColorPalette,
                    };
                }
            }
            return true;
        }

        public bool CreateTiles(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return false;
            }
            if (TileW == 0 || TileH == 0)
            {
                return false;
            }
            if (TileData == null)
            {
                return false;
            }
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            {
                Parallel.For(0, RT, r =>
                {
                    Parallel.For(0, CT, c =>
                    {
                        TileData[r, c].ExecuteTrasform(oDataSource, r * TileH, c * TileW);
                    });
                });
            }
            return true;
        }

        public int[,] CreateImageFromTiles(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return null;
            }
            if (TileW == 0 || TileH == 0)
            {
                return null;
            }
            if (TileData == null)
            {
                return null;
            }
            var R = oDataSource.GetLength(0);
            var C = oDataSource.GetLength(1);
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            var oRet = new int[R, C];
            {
                Parallel.For(0, RT, r =>
                {
                    Parallel.For(0, CT, c =>
                    {
                        TileData[r, c].MergeData(oRet);
                    });
                });
            }
            return oRet;
        }

        public int[,]? TransformAndDither(int[,]? oDataSource)
        { 
            if( CreateTiles(oDataSource) ) 
            {
                return CreateImageFromTiles(oDataSource);
            }
            return null;
        }

        public bool CalcExternalImageError(int[,]? oDataSource)
        {
            if (oDataSource == null)
            {
                return false;
            }
            if (TileW == 0 || TileH == 0)
            {
                return false;
            }
            if (TileData == null)
            {
                return false;
            }
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            {
                Parallel.For(0, RT, r =>
                {
                    Parallel.For(0, CT, c =>
                    {
                        TileData[r, c].CalcExternalImageError(oDataSource);
                    });
                });
            }
            return true;
        }

        public static TileManager Create(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, ColorPalette oFixedColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode)
        {
            var oRet = new TileManager();
            if (!oRet.Init(oDataSource, iTileW, iTileH, iMaxTileColors, oFixedColorPalette, eColorDistanceMode, eColorReductionMode))
            {
                Trace.TraceError("Cannot create TileManager");
            }
            return oRet;
        }

        public static int[,]? MergeData(int[,]? oDataSource, List<TileManager> lTileMan, TileBase.EnumErrorSourceMode eErrorMode)
        {
            if (oDataSource == null)
                return null;
            if (lTileMan == null || lTileMan.Count == 0)
                return null;
            var lTileManF = lTileMan.Where(X=>X != null).ToList();
            if (lTileManF == null || lTileManF.Count == 0)
                return null;

            int R = oDataSource.GetLength(0);
            int C = oDataSource.GetLength(1);
            var oRet = new int[R, C];

            var RT = lTileManF.Min(X => X.TileData.GetLength(0));
            var CT = lTileManF.Min(X => X.TileData.GetLength(1));
            var lTiles = new List<TileBase>();
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    lTiles.Clear();
                    lTileManF.ForEach(X=>lTiles.Add(X.GetTileData(r, c)));   
                    TileBase.MergeData(oRet, lTiles, eErrorMode);
                }
            }

            return oRet;
        }


        public static int[,]? MergeData(int[,]? oDataSource, TileManager oTileA, TileManager oTileB, TileBase.EnumErrorSourceMode eErrorMode)
        {
            if (oDataSource == null)
                return null;
            if (oTileA == null && oTileB == null)
                return null;

            return MergeData(oDataSource, new List<TileManager> { oTileA, oTileB }, eErrorMode );
        }
    }
}

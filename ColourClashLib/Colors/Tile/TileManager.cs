using ColourClashLib.Color;
using ColourClashNet.Colors;
using ColourClashNet.Colors.Dithering;
using ColourClashNet.Colors.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashLib.Colors.Tile
{
    public class TileManager
    {
        public TileBase[,] TileData { get; set; }
        public int TileW { get; set; } = 8;
        public int TileH { get; set; } = 8;

        public ColorPalette FixedColorPalette { get; set; } = new ColorPalette();

        TileBase.EnumColorReductionMode ColorReductionMode { get; set; } = TileBase.EnumColorReductionMode.Fast;
        public int MaxColors { get; set; } = 2;

        void Free()
        {
            TileData = null;
            TileW = 0;  
            TileH = 0;
            MaxColors = 0;
        }


        public bool Create(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, ColorPalette oFixedColorPalette, TileBase.EnumColorReductionMode eColorReductionMode )
        {
            Free();
            if (oDataSource == null)
            {
                return false;
            }
            if( iTileW <= 0 || iTileH <= 0 || iMaxTileColors <= 0) 
            {
                return false;
            }
            TileW = iTileW;
            TileH = iTileH;
            ColorReductionMode = eColorReductionMode;
            FixedColorPalette = oFixedColorPalette ?? new ColorPalette();
            MaxColors = iMaxTileColors;
            int R = (oDataSource.GetLength(0)+TileH-1)/TileH;
            int C = (oDataSource.GetLength(1)+TileW-1)/TileW;
            TileData = new TileBase[R, C];
            for (int r = 0; r < R; r++)
            {
                for( int c= 0; c < C; c++) 
                {
                    TileData[r, c] = new TileBase()
                    {
                        TileW = TileW,
                        TileH = TileH,
                        TileMaxColors = MaxColors,
                        ColorReductionMode = ColorReductionMode,
                        FixedPalette = FixedColorPalette,
                    };
                }
            }
            return true;
        }

        public int[,]? TransformAndDither(int[,]? oDataSource)
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
            var R = oDataSource.GetLength(0);
            var C = oDataSource.GetLength(1);
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

    }
}

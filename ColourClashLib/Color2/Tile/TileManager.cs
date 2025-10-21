using ColourClashNet.Color;
using ColourClashNet.Color;
using ColourClashNet.Color.Dithering;
using ColourClashNet.Color.Transformation;
using ColourClashNet.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColourClashNet.Color.Tile
{
    public class TileManager
    {
        static string sClass = nameof(TileManager); 

        TileBase[,] TileData { get; set; }
        public int TileW { get; private set; } = 8;
        public int TileH { get; private set; } = 8;
        public int TileR { get; private set; } = 0;
        public int TileC { get; private set; } = 0;

        public Palette FixedColorPalette { get; set; } = new Palette();

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


        public bool Init(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, Palette oFixedColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode)
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
            FixedColorPalette = oFixedColorPalette ?? new Palette();
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

        public async Task<bool> CreateTilesAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            return await Task.Run(() =>
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
                        Parallel.For(0, CT, async c =>
                        {
                            await TileData[r, c].ExecuteTrasformAsync(oDataSource, r * TileH, c * TileW, oToken);
                        });
                    });
                }
                return true;
            });
        }

        public async Task<int[,]> CreateImageFromTilesAsync(int[,]? oDataSource, CancellationToken? oToken)
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

            Parallel.For(0, RT, r =>
            {
                Parallel.For(0, CT, async c =>
                {
                    await TileData[r, c].MergeDataAsync(oRet,oToken);
                });
            });
            return oRet;
        }

        public async Task <int[,]?> TransformAndDitherAsync(int[,]? oDataSource, CancellationToken? oToken)
        { 
            if( await CreateTilesAsync(oDataSource,oToken) ) 
            {
                return await CreateImageFromTilesAsync(oDataSource,oToken);
            }
            return null;
        }

        public async Task<double> CalcExternalImageErrorAsync(int[,]? oDataSource, CancellationToken? oToken)
        {
            if (oDataSource == null)
            {
                return double.NaN;
            }
            if (TileW == 0 || TileH == 0)
            {
                return double.NaN;
            }
            if (TileData == null)
            {
                return double.NaN;
            }
            int RT = TileData.GetLength(0);
            int CT = TileData.GetLength(1);
            var tasks = new List<Task>();

            for( int r =0; r < RT; r++)
            //Parallel.For(0, RT, r =>
            {
                for (int c = 0; c < CT; c++)
                //    Parallel.For(0, CT, async c =>
                {
                    tasks.Add(Task.Run(()=>TileData[r, c].CalcExternalImageErrorAsync(oDataSource,oToken)));
                }
            }
            await Task.WhenAll(tasks);
            double dError = 0;
            for (int r = 0; r < RT; r++)
            {
                for (int c = 0; c < CT; c++)
                {
                    dError += TileData[r, c].ExternalImageError;
                }
            }
            return dError;
        }


        public static TileManager CreateTileManager(int[,]? oDataSource, int iTileW, int iTileH, int iMaxTileColors, Palette oFixedColorPalette, ColorDistanceEvaluationMode eColorDistanceMode, TileBase.EnumColorReductionMode eColorReductionMode)
        {
            string sMethod = nameof(CreateTileManager); 
            var oRet = new TileManager();
            if (oRet.Init(oDataSource, iTileW, iTileH, iMaxTileColors, oFixedColorPalette, eColorDistanceMode, eColorReductionMode))
            {
                return oRet;
            }
            else
            { 
                LogMan.Error(sClass, sMethod, "Cannot init TileManager");
                return null;
            }
        }

        public static async Task<int[,]?> MergeDataAsync(int[,]? oDataSource, List<TileManager> lTileMan, TileBase.EnumErrorSourceMode eErrorMode, CancellationToken? oToken)
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
                    await TileBase.MergeDataAsync(oRet, lTiles, eErrorMode, oToken);
                }
            }

            return oRet;
        }


        public static async Task<int[,]?> MergeDataAsync(int[,]? oDataSource, TileManager oTileA, TileManager oTileB, TileBase.EnumErrorSourceMode eErrorMode, CancellationToken oToken)
        {
            if (oDataSource == null)
                return null;
            if (oTileA == null && oTileB == null)
                return null;

            return await MergeDataAsync(oDataSource, new List<TileManager> { oTileA, oTileB }, eErrorMode, oToken );
        }
    }
}
